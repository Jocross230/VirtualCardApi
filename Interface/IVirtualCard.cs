
using VirtualCard.Request;
using VirtualCard.Model;
using VirtualCard.Dtos;
using VirtualCard.Model;

namespace VisualCard.Interface
{
    public interface IVirtualCard
    {
        
        Task<EncryptResponse> BlockCardAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> ChangeCardPinAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> ResetCardPinAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> GetCardStatusAsync(EncryptRequest encryptRequest);
        
        Task<CreatedCard> GetByUserIdAsync(string UserId);

        Task<EncryptResponse> FetchCardExcludedAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> FetchCardIncludedAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> FetchCardsByCreationChannelAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> GetStatementAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> UnblockCardAsync(EncryptRequest encryptRequest);
        Task<EncryptResponse> CreateCard2Async(EncryptRequest encryptRequest, string Channel);
        Task<CreatedCard> GetCardDetailsByProfileIdAsync(Guid profileId);
        Task<string> TransactionDisputeAsync(TransectionDispute dis);
        
    }
}
