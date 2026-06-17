import sys
import os

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import boto3
from botocore.client import Config
from pymongo import MongoClient
import config


def get_minio_client():
    return boto3.client(
        "s3",
        endpoint_url=config.MINIO_ENDPOINT,
        aws_access_key_id=config.MINIO_ACCESS_KEY,
        aws_secret_access_key=config.MINIO_SECRET_KEY,
        config=Config(signature_version="s3v4"),
    )


def ensure_bucket(s3, bucket_name):
    existing = [b["Name"] for b in s3.list_buckets().get("Buckets", [])]
    if bucket_name not in existing:
        s3.create_bucket(Bucket=bucket_name)
        print(f"Bucket '{bucket_name}' kreiran.")
    else:
        print(f"Bucket '{bucket_name}' već postoji.")


def upload_audio_files():
    s3 = get_minio_client()
    ensure_bucket(s3, config.MINIO_BUCKET)

    client = MongoClient(config.MONGO_URI)
    db = client[config.DB_NAME]
    collection = db["audio_files"]
    collection.create_index("object_key", unique=True)

    location = {"latitude": 45.8150, "longitude": 15.9819}  
    audio_dir = config.AUDIO_DIR
    files = [f for f in os.listdir(audio_dir)
             if f.lower().endswith((".mp3", ".wav"))]

    if not files:
        print(f"Nema audio datoteka u {audio_dir}")
        return

    for filename in files:
        local_path = os.path.join(audio_dir, filename)
        object_key = filename  

        s3.upload_file(local_path, config.MINIO_BUCKET, object_key)
        print(f"Uploadano: {filename}")

        metadata = {
            "object_key": object_key,
            "filename": filename,
            "bucket": config.MINIO_BUCKET,
            "location": location,
            "size_bytes": os.path.getsize(local_path),
        }
        collection.update_one(
            {"object_key": object_key},
            {"$set": metadata},
            upsert=True,
        )

    print(f"\nGotovo. Uploadano {len(files)} datoteka, metapodaci spremljeni u MongoDB.")
    client.close()


def main():
    upload_audio_files()


if __name__ == "__main__":
    main()