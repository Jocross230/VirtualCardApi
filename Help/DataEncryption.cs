using System.Security.Cryptography;
using System.Text;

namespace VirtualCard.Help
{
    public class DataEncryption :IDataEncryption
    {
        public string EncryptStringToBytes_Aes(string plainText)
        {
            string encrypted = string.Empty;
            string Key = "f5a69a8384467e04";
            //string Key = "MukmdctNO4l6r86HBi60yg==";
            try
            {
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);
                using (AesManaged aes = new AesManaged())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = keyBytes;
                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                    encrypted = Convert.ToBase64String(encryptedBytes);

                    //Console.WriteLine(encryptedText);
                }
                // Return the encrypted bytes from the memory stream.
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                throw;
            }

            return encrypted;
        }

        public string DecryptStringFromBytes_Aes(string cipherText)
        {
            string plaintext = string.Empty;
            //string Key = "n5LRI4V36jXH1nzE";
            string key = "f5a69a8384467e04";

            try
            {
                byte[] ciphertextBytes = Convert.FromBase64String(cipherText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                using (AesManaged aes = new AesManaged())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = keyBytes;
                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
                    plaintext = Encoding.UTF8.GetString(decryptedBytes);
                    return plaintext;
                    //Console.WriteLine(decryptedText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "96";
            }

        }
    }
}
