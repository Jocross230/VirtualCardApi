
using VirtualCard.Model;
using VisualCard.Model;

namespace VisualCard.Interface
{
    public interface IVirtualCard
    {
        
        Task<string> BlockCardAsync(BlockCardRequest req);
        Task<string> ChangeCardPinAsync(CardPinChangeRequest pinChangeRequest);
        Task<string> ResetCardPinAsync(CardPinResetRequest pinResetRequest);
        Task<string> GetCardStatusAsync(CardStatusRequest request);
        //Task<string> CreateCardAsync(CreateCardRequest req);
        Task<string> FetchCardExcludedAsync(FetchCardRequest req);
        Task<string> FetchCardIncludedAsync(FetchCardRequest1 req);
        Task<string> FetchCardsByCreationChannelAsync(FetchCardsByCreationChannelRequest request);
        Task<string> GetStatementAsync(GetStatementRequest request);
        Task<string> UnblockCardAsync(UnBlockCardRequest request);
        Task<string> CreateCardAsync(CreateCardRequest request);
    }
}
