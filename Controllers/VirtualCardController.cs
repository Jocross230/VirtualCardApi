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
        [HttpPost("create-card,suntrust-mobile")]
        public async Task<IActionResult> CreateCard([FromBody] EncryptRequest encryptRequest)
        {
            if (encryptRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.CreateCardAsync(encryptRequest);
                return Ok(new { Message = "Card created successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }[HttpPost("create-card,suntrust-web")]
        public async Task<IActionResult> Create2Card([FromBody] EncryptRequest encryptRequest)
        {
            if (encryptRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.CreateCard2Async(encryptRequest);
                return Ok(new { Message = "Card created successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("BlockCard")]
        public async Task<IActionResult> BlockCard([FromBody] EncryptRequest encryptRequest)
        {
            _logger.LogInformation("Received BlockCard request: {@Request}", encryptRequest);

           /* if (encryptRequest == null || string.IsNullOrEmpty(req.accountNumber))
            {
                _logger.LogWarning("BlockCard request failed: Invalid request data.");
                return BadRequest("Invalid request data.");
            }*/

            try
            {
                var response = await _visualCard.BlockCardAsync(encryptRequest);

                // Log the response
                _logger.LogInformation("BlockCard response: {Response}", response);

                return Ok(new { Message = "Card blocked successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing BlockCard request.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        
        [HttpPost("FetchCard-ClientReference-pin")]
        public async Task<IActionResult> FetchCards([FromBody] EncryptRequest encryptRequest)
        {
            var response = await _visualCard.FetchCardExcludedAsync(encryptRequest);
            return Ok(response);
        }
        [HttpPost("FetchCard-CardReference-pin")]
        public async Task<IActionResult> FetchCard([FromBody] EncryptRequest encryptRequest)
        {
            var response = await _visualCard.FetchCardIncludedAsync(encryptRequest);
            return Ok(new { Message = "FetchCard-CardReference successful", Data = response });
        }
        [HttpPost("Fetch-by-creation-channel")]
        public async Task<IActionResult> FetchCardsByCreationChannel([FromBody] EncryptRequest encryptRequest)
        {
            var response = await _visualCard.FetchCardsByCreationChannelAsync(encryptRequest);
            return Ok(new { Message = "FetchCardsByCreationChannel successful", Data = response });
        }
        [HttpPost("Change-pin")]
        public async Task<IActionResult> ChangeCardPin([FromBody] EncryptRequest encryptRequest)
        {
            _logger.LogInformation("Received ChangeCardPin request: {@Request}", encryptRequest);

            if (encryptRequest == null)
            {
                _logger.LogWarning("ChangeCardPin request failed: Invalid request data.");
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ChangeCardPinAsync(encryptRequest);

                // Log the response
                _logger.LogInformation("ChangeCardPin response: {Response}", response);

                return Ok(new { Message = " successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ChangeCardPin request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Reset-pin")]
        public async Task<IActionResult> ResetCardPin([FromBody] EncryptRequest encryptRequest)
        {
            _logger.LogInformation("Received ResetCardPin request: {@Request}", encryptRequest);

            if (encryptRequest == null)
            {
                _logger.LogWarning("ResetCardPin request failed: Invalid request data.");
                return BadRequest("Invalid request data");
            }

            try
            {
                var response = await _visualCard.ResetCardPinAsync(encryptRequest);

                // Log the successful response
                _logger.LogInformation("ResetCardPin response: {Response}", response);

                return Ok(new { Message = "Reset pin successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ResetCardPin request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Transaction-statement")]
        public async Task<IActionResult> GetStatement([FromBody] EncryptRequest encryptRequest)
        {
            if (encryptRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.GetStatementAsync(encryptRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Get-card-status")]
        public async Task<IActionResult> GetCardStatus([FromBody] EncryptRequest encryptRequest)
        {
            if (encryptRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.GetCardStatusAsync(encryptRequest);
                return Ok(new { Message = "Successful", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Unblock-card")]
        public async Task<IActionResult> UnblockCard([FromBody] EncryptRequest encryptRequest)
        {
            _logger.LogInformation("Received UnblockCard request: {@Request}", encryptRequest);

            if (encryptRequest == null)
            {
                _logger.LogWarning("UnblockCard request failed: Invalid request data.");
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _visualCard.UnblockCardAsync(encryptRequest);

                // Log the successful response
                _logger.LogInformation("UnblockCard response: {successful}", result);

                return Ok(new { Message = "Card unblocked successful", Data = result });
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


