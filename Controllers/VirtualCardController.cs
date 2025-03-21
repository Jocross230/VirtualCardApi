using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VisualCard.Interface;
using VisualCard.Model;
using VirtualCard.Request;


namespace VirtualCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VirtualCardController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IVirtualCard _visualCard;
        public readonly ILogger<VirtualCardController> _logger;
        public VirtualCardController(HttpClient httpClient,ILogger<VirtualCardController> logger,IVirtualCard visualCard)
        {
            _httpClient = httpClient;
            _logger = logger;
            _visualCard = visualCard;
        }
        [HttpPost("create-card")]
        public async Task<IActionResult> CreateCard([FromBody] CreateCard request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.CreateCardAsync(request);
                return Ok(new { Message = "Card created successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("BlockCard")]
        public async Task<IActionResult> BlockCard([FromBody] BlockCard req)
        {
            _logger.LogInformation("Received BlockCard request: {@Request}", req);

            if (req == null || string.IsNullOrEmpty(req.accountNumber))
            {
                _logger.LogWarning("BlockCard request failed: Invalid request data.");
                return BadRequest("Invalid request data.");
            }

            try
            {
                var response = await _visualCard.BlockCardAsync(req);

                // Log the response
                _logger.LogInformation("BlockCard response: {Response}", response);

                return Ok(new { Message = "Card blocked successfully", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing BlockCard request.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        
        [HttpPost("FetchCard-ClientReference")]
        public async Task<IActionResult> FetchCards([FromBody] FetchCardRequest request)
        {
            var response = await _visualCard.FetchCardExcludedAsync(request);
            return Ok(response);
        }
        [HttpPost("FetchCard-CardReference")]
        public async Task<IActionResult> FetchCard([FromBody] FetchCardRequest1 req)
        {
            var response = await _visualCard.FetchCardIncludedAsync(req);
            return Ok(new { Message = "FetchCard-CardReference successfully", Data = response });
        }
        [HttpPost("Fetch-by-creation-channel")]
        public async Task<IActionResult> FetchCardsByCreationChannel([FromBody] FetchCardsByCreationChannelRequest request)
        {
            var response = await _visualCard.FetchCardsByCreationChannelAsync(request);
            return Ok(new { Message = "FetchCardsByCreationChannel successfully", Data = response });
        }
        [HttpPost("Change-pin")]
        public async Task<IActionResult> ChangeCardPin([FromBody] ChangePinRequest pinChangeRequest)
        {
            _logger.LogInformation("Received ChangeCardPin request: {@Request}", pinChangeRequest);

            if (pinChangeRequest == null)
            {
                _logger.LogWarning("ChangeCardPin request failed: Invalid request data.");
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ChangeCardPinAsync(pinChangeRequest);

                // Log the response
                _logger.LogInformation("ChangeCardPin response: {Response}", response);

                return Ok(new { Message = " successfull", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ChangeCardPin request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Reset-pin")]
        public async Task<IActionResult> ResetCardPin([FromBody] ResetPinRequest pinResetRequest)
        {
            _logger.LogInformation("Received ResetCardPin request: {@Request}", pinResetRequest);

            if (pinResetRequest == null)
            {
                _logger.LogWarning("ResetCardPin request failed: Invalid request data.");
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ResetCardPinAsync(pinResetRequest);

                // Log the successful response
                _logger.LogInformation("ResetCardPin response: {Response}", response);

                return Ok(new { Message = "Reset pin successfully", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ResetCardPin request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Transaction-statement")]
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
                return Ok(new { Message = "Successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Unblock-card")]
        public async Task<IActionResult> UnblockCard([FromBody] UnBlockCard request)
        {
            _logger.LogInformation("Received UnblockCard request: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("UnblockCard request failed: Invalid request data.");
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.UnblockCardAsync(request);

                // Log the successful response
                _logger.LogInformation("UnblockCard response: {successfull}", result);

                return Ok(new { Message = "Card unblocked successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing UnblockCard request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("AccountId")]
        public async Task<IActionResult> FindByIdAsync( string accountNumber)
        {
            try
            {
                
                var result = await _visualCard.GetByAccountNumberAsync(accountNumber);

                // If result is found, return the result
                if (result != null)
                {
                    return Ok(result);
                }

                // If result is not found, return a 404 not found response
                return NotFound($"No attestation record found for accountNumber: {accountNumber}");
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is configured)
                _logger.LogError($"An error occurred : {accountNumber}. Error: {ex.Message}");

                // Return a 500 server error response
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPost("send-dispute")]
        public async Task<IActionResult> SendDispute([FromBody] TransectionDispute dispute)
        {
            if (dispute == null)
            {
                return BadRequest("Invalid dispute data.");
            }

            var result = await _visualCard.TransactionDisputeAsync(dispute);

            if (string.IsNullOrEmpty(result))
            {
                return BadRequest("Failed to send the dispute.");
            }

            return Ok(new { Response = result });
        }
    }
    
}


