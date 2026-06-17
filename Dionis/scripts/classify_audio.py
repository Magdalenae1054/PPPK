import sys
import os
import json
import io
from datetime import datetime, timezone

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import requests
import boto3
from botocore.client import Config
from pymongo import MongoClient
import config

CLASSIFY_URL = "https://aves.regoch.net/api/classify"


def get_minio_client():
    return boto3.client(
        "s3",
        endpoint_url=config.MINIO_ENDPOINT,
        aws_access_key_id=config.MINIO_ACCESS_KEY,
        aws_secret_access_key=config.MINIO_SECRET_KEY,
        config=Config(signature_version="s3v4"),
    )


def classify_file(audio_path):
    with open(audio_path, "rb") as f:
        files = {"file": f}
        response = requests.post(CLASSIFY_URL, files=files, timeout=120)
    response.raise_for_status()
    return response.json()


def store_log_to_minio(s3, object_key, request_log):
    log_key = f"logs/{object_key}.json"
    body = json.dumps(request_log, indent=2, ensure_ascii=False).encode("utf-8")
    s3.put_object(
        Bucket=config.MINIO_BUCKET,
        Key=log_key,
        Body=io.BytesIO(body),
        ContentType="application/json",
    )
    return log_key


def main():
    s3 = get_minio_client()
    client = MongoClient(config.MONGO_URI)
    db = client[config.DB_NAME]
    classifications = db[config.CLASSIFICATIONS_COLLECTION]
    audio_files = db["audio_files"]

    for doc in audio_files.find():
        object_key = doc["object_key"]
        local_path = os.path.join(config.AUDIO_DIR, doc["filename"])

        if not os.path.exists(local_path):
            print(f"Preskačem (nema lokalne datoteke): {object_key}")
            continue

        print(f"Klasificiram: {object_key} ...")
        try:
            api_response = classify_file(local_path)
        except Exception as e:
            print(f"  Greška pri klasifikaciji: {e}")
            continue

        results = api_response.get("results", [])

        request_log = {
            "object_key": object_key,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "endpoint": CLASSIFY_URL,
            "num_results": len(results),
            "response": api_response,
        }
        log_key = store_log_to_minio(s3, object_key, request_log)

        classification_doc = {
            "object_key": object_key,
            "filename": doc["filename"],
            "location": doc.get("location"),
            "log_object_key": log_key,
            "results": results,
            "classified_at": datetime.now(timezone.utc),
        }
        classifications.update_one(
            {"object_key": object_key},
            {"$set": classification_doc},
            upsert=True,
        )

        if results:
            best = max(results, key=lambda r: r["confidence"])
            print(f"  Najbolja detekcija: {best['common_name']} "
                  f"({best['confidence']:.2f})")
        else:
            print("  Nema detekcija.")

    print("\nKlasifikacija gotova.")
    client.close()


def main_wrapper():
    main()


if __name__ == "__main__":
    main_wrapper()