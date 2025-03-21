using VisualCard.Model;

namespace VisualCard.Helper
{
    public interface ICryptoUtils
    {
        string DecryptData(string encryptedData, string denc_key);
        string EncryptData(string data, string denc_key);
       // string EncryptData(BlockCardRequest Data, string enc_key);
    }

}
