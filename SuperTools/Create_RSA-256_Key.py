from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization as ser

def generate_rsa_keys():
    key = rsa.generate_private_key(
        public_exponent = 65537,
        key_size = 2048
        )
    
    public_key = key.public_key()
    
    pem_private_key = key.private_bytes(
        encoding = ser.Encoding.PEM,
        format = ser.PrivateFormat.TraditionalOpenSSL,
        encryption_algorithm = ser.NoEncryption()
    )

    pem_public_key = public_key.public_bytes(
        encoding = ser.Encoding.PEM,
        format = ser.PublicFormat.SubjectPublicKeyInfo
    )
    return pem_private_key, pem_public_key

def main():
    print("鍵を作成しています")
    private_key, public_key = generate_rsa_keys()

    with open("private_key.pem","wb") as file:
        file.write(private_key)
    with open("public_key.pem","wb") as file:
        file.write(public_key)
    print("作成が完了しました")

main()