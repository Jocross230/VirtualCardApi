using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Utilities.Encoders;
using VisualCard.Helper;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class CryptoUtils :ICryptoUtils
{
    private byte[] GetAESKey(string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);  
        }
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);  
        }
        return keyBytes;
    }
   

    public string EncryptData(string data, string enc_key)
    {
        using (var aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GetAESKey(enc_key);
            aesAlg.IV = new byte[16]; 

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        };

    }

    
    public string DecryptData(string encryptedData, string denc_key)
    {
        using (var aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GetAESKey(denc_key);
            aesAlg.IV = new byte[16]; 

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }


    }

}




