namespace VirtualCard.Help
{
    public interface IDataEncryption
    {
        string DecryptStringFromBytes_Aes(string cipherText);
        string EncryptStringToBytes_Aes(string plainText);
    }
}
