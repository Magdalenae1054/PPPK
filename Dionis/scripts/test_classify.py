import requests

url = "https://aves.regoch.net/api/classify"
audio_path = "data/audio/XC1146600 - Thrush Nightingale - Luscinia luscinia.mp3"


with open(audio_path, "rb") as f:
    files = {"file": f}
    response = requests.post(url, files=files, timeout=60)

print("Status code:", response.status_code)
print("Response headers:", dict(response.headers))
print("Response body:")
print(response.text[:2000])  