using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtualCard.Interface;
using VirtualCard.Model;
using VirtualCard.Request;
using VirtualCard.Dtos;

namespace VirtualCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VirtualCardController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IVirtualCard _visualCard;
        public readonly ILogger<VirtualCardController> _logger;
        public VirtualCardController(HttpClient httpClient, ILogger<VirtualCardController> logger, IVirtualCard visualCard)
        {
            _httpClient = httpClient;
            _logger = logger;
            _visualCard = visualCard;
        }

        [HttpPost("create-card2")]
        public async Task<IActionResult> Create2Card([FromBody] EncryptRequest encryptRequest, [FromQuery] CreateCardChannel channel)
        {
            if (encryptRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                if (!Enum.IsDefined(typeof(CreateCardChannel), channel))
                    return BadRequest(new { message = "Invalid or missing transaction channel." });

                var channelName = channel.ToString().Replace('_', '-');
                var result = await _visualCard.CreateCard2Async(encryptRequest, channelName);
                return Ok(new { Message = "Card created successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Create2Card");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
            }
        }

        [HttpPost("BlockCard")]
        public async Task<IActionResult> BlockCard([FromBody] EncryptRequest encryptRequest)
        {
            _logger.LogInformation("Received BlockCard request: {@Request}", encryptRequest);

            try
            {
                var response = await _visualCard.BlockCardAsync(encryptRequest);
                _logger.LogInformation("BlockCard response: {Response}", response);
                return Ok(new { Message = "Card blocked successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing BlockCard request.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
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
                _logger.LogInformation("ChangeCardPin response: {Response}", response);
                return Ok(new { Message = " successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ChangeCardPin request.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
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
                _logger.LogInformation("ResetCardPin response: {Response}", response);
                return Ok(new { Message = "Reset pin successful", Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ResetCardPin request.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
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
                _logger.LogError(ex, "Unhandled exception in GetStatement");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
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
                _logger.LogError(ex, "Unhandled exception in GetCardStatus");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
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
                _logger.LogInformation("UnblockCard response: {successful}", result);
                return Ok(new { Message = "Card unblocked successful", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing UnblockCard request.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
            }
        }

        [HttpGet("UsedId")]
        public async Task<IActionResult> FindByIdAsync(string UserId)
        {
            try
            {
                var result = await _visualCard.GetByUserIdAsync(UserId);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound($"No record found : {UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in FindByIdAsync for {UserId}", UserId);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
            }
        }


        [HttpGet("profileId")]
        public async Task<IActionResult> GetCustomerCardByProfileId(Guid profileId)
        {
            var card = await _visualCard.GetCardDetailsByProfileIdAsync(profileId);

            if (card == null)
                return NotFound("Customer or card not found");

            return Ok(card);
        }
    }
}
