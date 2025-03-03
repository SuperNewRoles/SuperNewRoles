from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import serialization as ser
from cryptography.hazmat.primitives import hashes
from Cryptodome.Cipher import AES
from Cryptodome.Util.Padding import unpad
import base64
import re

def GetKey():
    try:
        with open("LogOutput.log","r") as file:
            AES_E_Key = ""
            for line in file:
                AES_E_Key_list =  re.findall(re.escape("$SNRKST$")+ r'(.*?)' +re.escape("$SNRKET$"),
                                        "".join(line))
                if AES_E_Key_list != []:
                    AES_E_Key = AES_E_Key_list[0]
                    break
            with open("private_key.pem","rb") as file2:
                RSA_Key = ser.load_pem_private_key(
                    file2.read(),
                    password = None
                )
            AES_Key = RSA_Key.decrypt(
                base64.b64decode(AES_E_Key),
                padding.OAEP(
                    algorithm = hashes.SHA256(),
                    mgf = padding.MGF1(algorithm = hashes.SHA256()),
                    label = None
                )
            )
            return AES_Key
    except Exception as e:
        print(f"Error:{str(e)}")
        input("Please enter any key...")
        exit()

def decoding():
    try:
        key = GetKey()
        with open("LogOutput.log","r") as file:
            with open("DecryptedLogOutput.log","w") as file2:
                for line in file:
                    textlist =  re.findall(re.escape("$SNRST$")+ r'(.*?)' +re.escape("$SNRET$"),
                                            "".join(line))
                    keyline = re.findall(re.escape("$SNRKST$")+ r'(.*?)' +re.escape("$SNRKET$"),
                                        "".join(line))
                    if textlist != []:
                        text = base64.b64decode(textlist[0][24:])
                        iv = base64.b64decode(textlist[0][:24])
                        cipher = AES.new(bytes(key), AES.MODE_CBC, iv)
                        plainText = unpad(cipher.decrypt(text), AES.block_size)
                        newline   =  re.sub(re.escape("$SNRST$")+ r'(.*?)' +re.escape("$SNRET$"),
                                            plainText.decode("utf-8"),
                                            line)
                        file2.writelines(newline)
                    elif keyline != []:
                        newline   =  re.sub(re.escape("$SNRKST$")+ r'(.*?)' +re.escape("$SNRKET$"),
                                            f"AES.Key:{base64.b64encode(key).decode('utf-8')}",
                                            line)
                        file2.writelines(newline)
                    else:
                        file2.writelines(line)

    except Exception as e:
        print(f"Error:{str(e)}")
        input("Please enter any key...")

decoding()