using System.Security.Cryptography;
using System.Text;

namespace VirtualCard.Help
{
    public class DataEncryption :IDataEncryption
    {
        private readonly string _key;

        public DataEncryption(IConfiguration configuration)
        {
            
            _key = configuration["key:AesKey"];

           
        }
        public string EncryptStringToBytes_Aes(string plainText)
        {
            try
            {
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(_key);

                using (AesManaged aes = new AesManaged())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = keyBytes;

                    using ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");
                throw;
            }
            
        }

        public string DecryptStringFromBytes_Aes(string cipherText)
        {
            try
            {
                byte[] ciphertextBytes = Convert.FromBase64String(cipherText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(_key);

                using (AesManaged aes = new AesManaged())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = keyBytes;

                    using ICryptoTransform decryptor = aes.CreateDecryptor();
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
                return "96"; // your system error code
            }
        }
       

    }
    
}
