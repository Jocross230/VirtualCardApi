
using VirtualCard.Request;
using VisualCard.Model;

namespace VisualCard.Interface
{
    public interface IVirtualCard
    {
        
        Task<string> BlockCardAsync(BlockCard BlockedCards);
        Task<string> ChangeCardPinAsync(ChangePinRequest pinChangeRequest);
        Task<string> ResetCardPinAsync(ResetPinRequest ResetPinRequests);
        Task<string> GetCardStatusAsync(CardStatusRequest request);
        
        Task<CreatedCard> GetByAccountNumberAsync(string accountNumber);

        Task<string> FetchCardExcludedAsync(FetchCardRequest req);
        Task<string> FetchCardIncludedAsync(FetchCardRequest1 req);
        Task<string> FetchCardsByCreationChannelAsync(FetchCardsByCreationChannelRequest request);
        Task<string> GetStatementAsync(GetStatementRequest request);
        Task<string> UnblockCardAsync(UnBlockCard UnBlockedCards);
        Task<string> CreateCardAsync(CreateCard CreateCards);
        
       
        Task<string> TransactionDisputeAsync(TransectionDispute dis);
        
    }
}
