
import json
import time
import hmac
import hashlib
import base64
import urllib.request
import urllib.error

# Configuration
SECRET_KEY = "TroLiKOC_SuperSecretKey_2026_MinLength32Chars!"
ISSUER = "TroLiKOC"
AUDIENCE = "TroLiKOC"
API_URL = "http://localhost:5500/api/jobs"

def base64url_encode(data):
    return base64.urlsafe_b64encode(data).rstrip(b'=')

def generate_jwt(user_id="test-user-id", email="test@example.com"):
    header = {
        "alg": "HS256",
        "typ": "JWT"
    }
    
    now = int(time.time())
    payload = {
        "sub": user_id,
        "email": email,
        "iss": ISSUER,
        "aud": AUDIENCE,
        "nbf": now,
        "exp": now + 3600,
        "iat": now,
        "jti": str(now)
    }
    
    header_b64 = base64url_encode(json.dumps(header).encode('utf-8'))
    payload_b64 = base64url_encode(json.dumps(payload).encode('utf-8'))
    
    signature_base = header_b64 + b'.' + payload_b64
    signature = hmac.new(SECRET_KEY.encode('utf-8'), signature_base, hashlib.sha256).digest()
    signature_b64 = base64url_encode(signature)
    
    return (signature_base + b'.' + signature_b64).decode('utf-8')

def create_job(token):
    # Using a real accessible image (Lenna or similar standard test image)
    # Using a reliable placeholder image service
    job_payload = {
        "jobType": "TalkingHead",
        "priority": "High",
        "sourceImageUrl": "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png", 
        "audioUrl": "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
        "outputResolution": "720p"
    }
    
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    }
    
    print("Payload:", json.dumps(job_payload, indent=2))
    
    req = urllib.request.Request(API_URL, data=json.dumps(job_payload).encode('utf-8'), headers=headers, method='POST')
    
    try:
        with urllib.request.urlopen(req) as response:
            print(f"Status Code: {response.getcode()}")
            resp_body = response.read().decode('utf-8')
            print(f"Response: {resp_body}")
            return json.loads(resp_body)
    except urllib.error.HTTPError as e:
        print(f"HTTP Error: {e.code}")
        print(f"Reason: {e.reason}")
        print(f"Body: {e.read().decode('utf-8')}")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    print("Generating JWT...")
    token = generate_jwt()
    
    print("Sending REAL Job Request...")
    result = create_job(token)
    
    if result and 'id' in result:
        print(f"\nJob ID: {result['id']}")
        print("Waiting for processing... (Check docker logs trolikoc-ai-worker)")
