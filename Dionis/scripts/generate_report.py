import sys
import os
import argparse

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import pandas as pd
from rapidfuzz import fuzz
from pymongo import MongoClient
import config


def load_classifications():
    """Učitava rezultate klasifikacije iz MongoDB i 'razmata' detekcije u redove."""
    client = MongoClient(config.MONGO_URI)
    db = client[config.DB_NAME]
    docs = list(db[config.CLASSIFICATIONS_COLLECTION].find())
    client.close()

    rows = []
    for doc in docs:
        location = doc.get("location") or {}
        for det in doc.get("results", []):
            rows.append({
                "common_name": det.get("common_name"),
                "scientific_name": det.get("scientific_name"),
                "confidence": det.get("confidence"),
                "filename": doc.get("filename"),
                "latitude": location.get("latitude"),
                "longitude": location.get("longitude"),
            })
    return pd.DataFrame(rows)


def clean_and_transform(df):
    """Čišćenje i transformacija podataka."""
    if df.empty:
        return df

    df = df.dropna(subset=["scientific_name", "common_name"])

    df["common_name"] = df["common_name"].str.strip()
    df["scientific_name"] = df["scientific_name"].str.strip()

    df = df[df["confidence"] >= config.CONFIDENCE_THRESHOLD]

    return df


def build_statistics(df):
    if df.empty:
        return pd.DataFrame()

    stats = df.groupby(["scientific_name", "common_name"]).agg(
        num_observations=("confidence", "count"),
        avg_confidence=("confidence", "mean"),
        max_confidence=("confidence", "max"),
    ).reset_index()

    stats["avg_confidence"] = stats["avg_confidence"].round(3)
    stats["max_confidence"] = stats["max_confidence"].round(3)

    stats = stats.sort_values("num_observations", ascending=False)
    return stats


def apply_fuzzy_filter(stats, query, threshold=70):
    if stats.empty:
        return stats

    def matches(row):
        score_common = fuzz.partial_ratio(query.lower(), str(row["common_name"]).lower())
        score_sci = fuzz.partial_ratio(query.lower(), str(row["scientific_name"]).lower())
        return max(score_common, score_sci) >= threshold

    mask = stats.apply(matches, axis=1)
    return stats[mask]


def main():
    parser = argparse.ArgumentParser(description="Generiraj CSV izvješće o pticama.")
    parser.add_argument("--fuzzy", type=str, default=None,
                        help="Opcionalni fuzzy filter po nazivu vrste.")
    args = parser.parse_args()

    df = load_classifications()
    df = clean_and_transform(df)
    stats = build_statistics(df)

    if args.fuzzy:
        print(f"Primjenjujem fuzzy filter: '{args.fuzzy}'")
        stats = apply_fuzzy_filter(stats, args.fuzzy)

    if stats.empty:
        print("Nema vrsta koje zadovoljavaju kriterije.")
        return

    stats.to_csv(config.OUTPUT_CSV, index=False, encoding="utf-8")
    print(f"\nIzvješće spremljeno: {config.OUTPUT_CSV}")
    print(f"Broj vrsta u izvješću: {len(stats)}\n")
    print(stats.to_string(index=False))


if __name__ == "__main__":
    main()