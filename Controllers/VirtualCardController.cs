using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VisualCard.Interface;
using VisualCard.Model;

namespace VirtualCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VirtualCardController : ControllerBase
    {
        private readonly IVirtualCard _visualCard;
        public VirtualCardController(IVirtualCard visualCard)
        {
            _visualCard = visualCard;
        }
        [HttpPost("create-card")]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.CreateCardAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("BlockCard")]
        public async Task<IActionResult> BlockCard([FromBody] BlockCardRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.accountNumber))
            {
                return BadRequest("Invalid request data.");
            }

            var response = await _visualCard.BlockCardAsync(req);
            return Ok(response);
        }
        
        [HttpPost("FetchCard_ClientReference")]
        public async Task<IActionResult> FetchCards([FromBody] FetchCardRequest request)
        {
            var response = await _visualCard.FetchCardExcludedAsync(request);
            return Ok(response);
        }
        [HttpPost("FetchCard-CardReference")]
        public async Task<IActionResult> FetchCard([FromBody] FetchCardRequest1 req)
        {
            var response = await _visualCard.FetchCardIncludedAsync(req);
            return Ok(response);
        }
        [HttpPost("Fetch-by-creation-channel")]
        public async Task<IActionResult> FetchCardsByCreationChannel([FromBody] FetchCardsByCreationChannelRequest request)
        {
            var response = await _visualCard.FetchCardsByCreationChannelAsync(request);
            return Ok(response);
        }
        [HttpPost("Change-pin")]
        public async Task<IActionResult> ChangeCardPin([FromBody] CardPinChangeRequest pinChangeRequest)
        {
            if (pinChangeRequest == null)
            {
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ChangeCardPinAsync(pinChangeRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Reset-pin")]
        public async Task<IActionResult> ResetCardPin([FromBody] CardPinResetRequest pinResetRequest)
        {
            if (pinResetRequest == null)
            {
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ResetCardPinAsync(pinResetRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Get-statement")]
        public async Task<IActionResult> GetStatement([FromBody] GetStatementRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.GetStatementAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Get-card-status")]
        public async Task<IActionResult> GetCardStatus([FromBody] CardStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.GetCardStatusAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Unblock-card")]
        public async Task<IActionResult> UnblockCard([FromBody] UnBlockCardRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.UnblockCardAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
