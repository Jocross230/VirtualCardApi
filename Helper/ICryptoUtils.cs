namespace VisualCard.Helper
{
    public interface ICryptoUtils
    {
        string DecryptData(string encryptedData, string denc_key);
        string EncryptData(string data, string enc_key);
       
    }

}

