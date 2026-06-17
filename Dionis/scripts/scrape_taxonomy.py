import sys
import os

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import requests
from pymongo import MongoClient
import config


def fetch_species():
    """Dohvaća JSON s podacima o pticama s vanjskog servisa."""
    print(f"Dohvaćam podatke s {config.AVES_JSON_URL} ...")
    response = requests.get(config.AVES_JSON_URL, timeout=60)
    response.raise_for_status()   
    species = response.json()
    print(f"Dohvaćeno {len(species)} vrsta.")
    return species


def store_species(species):
    client = MongoClient(config.MONGO_URI)
    db = client[config.DB_NAME]
    collection = db[config.SPECIES_COLLECTION]
    collection.create_index("key", unique=True)

    inserted = 0
    skipped = 0
    for sp in species:
        result = collection.update_one(
            {"key": sp["key"]},        
            {"$setOnInsert": sp},     
            upsert=True
        )
        if result.upserted_id is not None:
            inserted += 1
        else:
            skipped += 1

    print(f"Umetnuto novih: {inserted}, preskočeno (već postoji): {skipped}")
    client.close()


def main():
    species = fetch_species()
    store_species(species)
    print("Korak 1 (taksonomija) gotov.")


if __name__ == "__main__":
    main()