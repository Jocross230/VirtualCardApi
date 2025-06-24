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
using VisualCard.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class CryptoUtils :ICryptoUtils
{
    private byte[] GetAESKey(string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);  // Pad key to 32 bytes if smaller
        }
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);  // Truncate if larger
        }
        return keyBytes;
    }
    /*private byte[] GetAESKey(string key)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }*/

    /*public string EncryptData(string data, string denc_key)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = GetAESKey(denc_key);
        aesAlg.GenerateIV(); // Use random IV
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

        var encryptor = aesAlg.CreateEncryptor();
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] cipherBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

        // Combine IV + Cipher
        byte[] combined = aesAlg.IV.Concat(cipherBytes).ToArray();

        return Convert.ToBase64String(combined);
    }

    public string DecryptData(string encryptedData, string denc_key)
    {
        byte[] combined = Convert.FromBase64String(encryptedData);

        byte[] iv = combined.Take(16).ToArray();
        byte[] cipherBytes = combined.Skip(16).ToArray();

        using var aesAlg = Aes.Create();
        aesAlg.Key = GetAESKey(denc_key);
        aesAlg.IV = iv;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

        var decryptor = aesAlg.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }*/

    public string EncryptData(string data, string denc_key)
    {
        using (var aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GetAESKey(denc_key);
            aesAlg.IV = new byte[16]; // IV set to zero for this example

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        };

    }

    // Decrypt the data using AES CBC mode with PKCS7 padding
    public string DecryptData(string encryptedData, string denc_key)
    {
        using (var aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GetAESKey(denc_key);
            aesAlg.IV = new byte[16]; // IV set to zero for this example

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }


    }

}




