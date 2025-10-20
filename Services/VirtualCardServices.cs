using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Http.Headers;
using System.Text;
using VirtualCard.Data;
using VirtualCard.Dtos;
using VirtualCard.Help;
using VirtualCard.Model;
using VirtualCard.Request;
using VirtualCard.TokenResponses;
using VisualCard.Helper;
using VisualCard.Interface;
using RestClientOptions = RestSharp.RestClientOptions;

namespace VisualCard.Services
{
    public class VirtualCardServices : IVirtualCard
    {



        public readonly HttpClient _httpClient;
        public readonly ILogger<VirtualCardServices> _logger;
        public readonly VirtualCardDbContext _context;
        public readonly UserProfileDbContext _contxt;
        private readonly IConfiguration _configuration;
        private readonly ICryptoUtils _cryptoUtils;
        private readonly GenerateTokens _generateToken;
        private readonly RestClient _client;
        private readonly IDataEncryption _dataEncryption;

        public VirtualCardServices(VirtualCardDbContext context, ILogger<VirtualCardServices> logger, GenerateTokens generateToken, IDataEncryption dataEncryption,
            IConfiguration configuration, HttpClient httpClient, ICryptoUtils cryptoUtils, UserProfileDbContext contxt)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _cryptoUtils = cryptoUtils;
            _generateToken = generateToken;
            _dataEncryption = dataEncryption;

            var options = new RestClientOptions(_configuration.GetValue<string>("BaseUrl"))
            {
                MaxTimeout = -1
            };
            _client = new RestClient(options);
            _contxt = contxt;
        }

       
        public async Task<EncryptResponse> BlockCardAsync(EncryptRequest encryptRequest)
        {
            var encryptResponse = new EncryptResponse();
            string sdata = string.Empty;

            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<BlockCard>(decrypt);

                if (decrypted == null || string.IsNullOrEmpty(decrypted.accountNumber) || string.IsNullOrEmpty(decrypted.cardReference))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }

                
                var newRequest = new BlockCard
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference
                };

                var serializedPayload = JsonConvert.SerializeObject(newRequest);

                
                string encryptedData = _cryptoUtils.EncryptData(serializedPayload, _configuration["AppSettings:enc_key"]);

                
                var tokenResponse = await _generateToken.GetToken2();

                
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/cardBlock", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                _logger.LogInformation("🔐 Sending card block request for account {Account} / cardRef {CardRef}",
                    decrypted.accountNumber, decrypted.cardReference);

                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogError("❌ Downstream error: {Status} - {Body}", response.StatusCode, response.Content);
                    var failResp = new BlockedCard
                    {
                        successful = false,
                        responseCode = "06",
                        responseMessage = $"Downstream error: {response.StatusCode}"
                    };
                    sdata = JsonConvert.SerializeObject(failResp);
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);
                    return encryptResponse;
                }

                
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);
                _logger.LogInformation("✅ Decrypted downstream response: {Response}", decryptedData);

                
                BlockedCard? apiResponse;
                try
                {
                    apiResponse = JsonConvert.DeserializeObject<BlockedCard>(decryptedData);
                }
                catch
                {
                    _logger.LogWarning("⚠️ Could not deserialize downstream response — returning generic failure.");
                    apiResponse = new BlockedCard
                    {
                        successful = false,
                        responseCode = "01",
                        responseMessage = "Invalid response format from downstream."
                    };
                }

                
                if (apiResponse == null)
                {
                    apiResponse = new BlockedCard
                    {
                        successful = false,
                        responseCode = "03",
                        responseMessage = "Empty response from downstream."
                    };
                }

                
                sdata = JsonConvert.SerializeObject(apiResponse);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception during BlockCardAsync.");

                var resp = new BlockedCard
                {
                    successful = false,
                    responseCode = "96",
                    responseMessage = "System Malfunction, please try again."
                };

                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }


        public async Task<EncryptResponse> ChangeCardPinAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {

                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<ChangePinRequest>(decrypt);
                if (decrypted == null || string.IsNullOrEmpty(decrypted.accountNumber) 
                    || string.IsNullOrEmpty(decrypted.oldPin) || 
                    string.IsNullOrEmpty(decrypted.cardReference) || 
                    string.IsNullOrEmpty(decrypted.newPin))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }
                var newRequest = new ChangePinRequest
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference,
                    oldPin = decrypted.oldPin,
                    newPin = decrypted.newPin,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                
                var tokenResponse = await _generateToken.GetToken2();

                
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/pinChange", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                
                RestResponse response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }

        }


        public async Task<EncryptResponse> ResetCardPinAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<ResetPinRequest>(decrypt);

                if (decrypted == null ||
                    string.IsNullOrWhiteSpace(decrypted.cardReference) ||
                    string.IsNullOrWhiteSpace(decrypted.accountNumber))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96"); 
                    return encryptResponse;
                }


                var newRequest = new ChangePinRequest
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference,


                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                var tokenResponse = await _generateToken.GetToken2();

                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/pinReset", Method.Post) 
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                RestResponse response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                

            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }



        }


        public async Task<EncryptResponse> GetCardStatusAsync(EncryptRequest encryptRequest)//CardStatusRequest erequest)
        {
            
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                
                if (decrypt== "96")
                {
                    _logger.LogWarning("❌ Decryption failed — returned system error code 96.");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("System malfunction");
                    return encryptResponse;
                }

                var newRequest = new
                {
                    cardReference = decrypt
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);

                string encryptedData = _cryptoUtils.EncryptData(deserialize, _configuration["AppSettings:enc_key"]);
                var tokenResponse = await _generateToken.GetToken2();

                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/cardStatus", Method.Post) // Corrected endpoint
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                RestResponse response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }

            return null;

        }
        public async Task<EncryptResponse> CreateCard2Async(EncryptRequest encryptRequest, string Channel)
        {
            string sdata = string.Empty;
            var encryptResponse = new EncryptResponse();

            try
            {
                var decryptedJson = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);

                if (decryptedJson == "96")
                {
                    _logger.LogWarning("Decryption failed — returned system error code 96.");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("System malfunction");
                    return encryptResponse;
                }

                var jsonObj = JObject.Parse(decryptedJson);
                var accountId = jsonObj["accountId"]?.ToString();

                _logger.LogInformation("Creating card for accountId: {AccountId}", accountId);

                if (string.IsNullOrEmpty(accountId))
                {
                    _logger.LogWarning("❌ Missing accountId in decrypted payload");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("Invalid account data.");
                    return encryptResponse;
                }

                
                var accountInfo = SunTrustProxy.getAccountBynumber(accountId);
                if (accountInfo == null || accountInfo.responseCode != "00" || accountInfo.Items == null || !accountInfo.Items.Any())
                {
                    _logger.LogWarning("Account not found or invalid for accountId: {AccountId}", accountId);
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("Account not found. Card creation denied.");
                    return encryptResponse;
                }

                var customer = accountInfo.Items[0];

                
                var newRequest = new CreateCard
                {
                    lastName = customer.LastName,
                    firstName = customer.FirstName,
                    nameOnCard = $"{customer.FirstName} {customer.LastName}".Trim(),
                    accountId = customer.Nuban,
                    accountType = customer.AccountType,
                    userId = customer.CustomerNumber,
                    mobileNr = customer.Phone,
                    emailAddress = customer.Email,
                    streetAddress = customer.Address,
                    city = null,
                    state = null,
                    countryCode = null,
                    postalCode = null,
                    cardProgram = "SUNTRUST VIRTUAL VER",
                    issuerNr = "81",
                    currencyCode = customer.CurrencyCode,
                    alias = null,
                    clientReference = Guid.NewGuid().ToString(),
                };

                string jsonToEncrypt = JsonConvert.SerializeObject(newRequest);
                string encryptedData = _cryptoUtils.EncryptData(jsonToEncrypt, _configuration["AppSettings:denc_key"]);

                var tokenResponse = await _generateToken.GetToken2();

                var client = new RestClient(_configuration["AppSettings:BaseUrl"]);
                var request = new RestRequest("/virtualcard/api/v2/createCard", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Card creation failed: {response.StatusCode}, {errorMessage}");
                }

             
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:denc_key"]);
                var decryptedCardResponse = JsonConvert.DeserializeObject<Roots>(decryptedData);

                var card = decryptedCardResponse?.card;
                if (card == null)
                {
                    _logger.LogWarning("❌ Invalid downstream card response");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("System malfunction");
                    return encryptResponse;
                }

                
                var savedCard = new CreatedCard
                {
                    alias = card.alias,
                    clientReference = card.clientReference,
                    cardReference = card.cardReference,
                    accountNumber = card.accountNumber,
                    pan = MaskPan(card.pan),
                    seqNr = card.seqNr,
                    expiryDate = card.expiryDate,
                    pinOffset = card.pinOffset,
                    cvv = card.cvv,
                    cvv2 = card.cvv2,
                    pinInfo = card.pinInfo,
                    track2 = card.track2,
                    customerId = card.customerId,
                    defaultAccountType = card.defaultAccountType,
                    blocked = card.blocked,
                    failedPinAttempts = card.failedPinAttempts,
                    creationChannel = Channel

                };

                await _context.CreatedCards.AddAsync(savedCard);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Saving card for Account {AccountNumber}", savedCard.accountNumber);



                var cardResponse = new Roots
                {
                    successful = true,
                    responseCode = "00",
                    responseMessage = "Card created successfully",
                    defaultPin = decryptedCardResponse.defaultPin,
                    card = savedCard
                };


                string responseJson = JsonConvert.SerializeObject(cardResponse);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(responseJson);
                return encryptResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during card creation.");

                var fallbackResponse = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };

                sdata = JsonConvert.SerializeObject(fallbackResponse);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        } 
        private bool IsBase64String(string base64)
        {
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private string MaskPan(string pan)
        {
            if (string.IsNullOrEmpty(pan) || pan.Length < 10)
                throw new ArgumentException("PAN must be at least 10 digits long.", nameof(pan));

            int maskLength = pan.Length - 10;
            string maskedSection = new string('*', maskLength);
            return pan.Substring(0, 6) + maskedSection + pan.Substring(pan.Length - 4);
        }

        public async Task<EncryptResponse> FetchCardsByCreationChannelAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<FetchCardsByCreationChannelRequest>(decrypt);
                if (decrypted == null || string.IsNullOrEmpty(decrypted.creationChannel))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }
                var newRequest = new FetchCardsByCreationChannelRequest
                {
                    creationChannel = decrypted.creationChannel
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);

                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/fetchCardsByChannel", Method.Post) // Corrected endpoint
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                // 6. Execute the request
                RestResponse response = await client.ExecuteAsync(request);

                // 7. Handle errors
                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                // var jsonresponse = await response.Content.ReadAsStringAsync();
                // string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
                //return decryptdata;

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                // return decryptedData;
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }

        public async Task<EncryptResponse> FetchCardExcludedAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<FetchCardRefandPin>(decrypt);

                if (decrypted == null || string.IsNullOrEmpty(decrypted.clientReference) || string.IsNullOrEmpty(decrypted.pin))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }

                var newRequest = new FetchCardRefandPin
                {
                    clientReference = decrypted.clientReference,
                    pin = decrypted.pin,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
               
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

               
                var tokenResponse = await _generateToken.GetToken2();

                // Create the HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v2/fetchCards", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);  // Ensure proper formatting

                // Send the request
                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }



        public async Task<EncryptResponse> FetchCardIncludedAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<FetchCardRequest1>(decrypt);
                

                if (decrypted == null || string.IsNullOrEmpty(decrypted.cardReference) || string.IsNullOrEmpty(decrypted.pin))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }
                var newRequest = new FetchCardRequest1
                {
                    cardReference = decrypted.cardReference,
                    pin = decrypted.pin,

                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                /*var json = JsonConvert.SerializeObject(req);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/

                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v2/fetchCards", Method.Post) // Corrected endpoint
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                // 6. Execute the request
                RestResponse response = await client.ExecuteAsync(request);

                // 7. Handle errors
                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                // var jsonresponse = await response.Content.ReadAsStringAsync();
                // string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
                //return decryptdata;

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                //return decryptedData;
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }



        public async Task<EncryptResponse> GetStatementAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<GetStatementRequest>(decrypt);

                if (decrypted == null || string.IsNullOrEmpty(decrypted.cardReference) 
                    || string.IsNullOrEmpty(decrypted.fromDate)
                    || string.IsNullOrEmpty(decrypted.toDate))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }
                var newRequest = new GetStatementRequest
                {

                    cardReference = decrypted.cardReference,
                    fromDate = decrypted.fromDate,
                    toDate = decrypted.toDate,
                    tranCount = decrypted.tranCount,
                    reference = decrypted.reference,
                    forward = decrypted.forward,
                    ordering = decrypted.ordering,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                var tokenResponse = await _generateToken.GetToken2();

                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/statement", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);

                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
            }
            catch (Exception ex)
            {
                var resp = new
                {
                    statuscode = "96",
                    statusmessage = "System Malfunction, please try again."
                };
                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }

       
        public async Task<EncryptResponse> UnblockCardAsync(EncryptRequest encryptRequest)
        {
            var encryptResponse = new EncryptResponse();
            string sdata = string.Empty;

            try
            {
                
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<UnBlockCard>(decrypt);

                
                if (decrypted == null || string.IsNullOrEmpty(decrypted.accountNumber) || string.IsNullOrEmpty(decrypted.cardReference))
                {
                    _logger.LogWarning("Decryption failed");
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("96");
                    return encryptResponse;
                }

                
                var newRequest = new UnBlockCard
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference
                };
                var serializedPayload = JsonConvert.SerializeObject(newRequest);
                string encryptedData = _cryptoUtils.EncryptData(serializedPayload, _configuration["AppSettings:enc_key"]);

                
                var tokenResponse = await _generateToken.GetToken2();

                
                var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/cardUnblock";
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
                httpRequest.Headers.Add("Accept", "application/json");
                httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
                httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);
                httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

                
                var response = await _httpClient.SendAsync(httpRequest);

                
                if (!response.IsSuccessStatusCode)
                {
                    var failResp = new UnblockedCard
                    {
                        successful = false,
                        responseCode = "06",
                        responseMessage = $"Downstream error: {response.StatusCode}"
                    };
                    sdata = JsonConvert.SerializeObject(failResp);
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);
                    return encryptResponse;
                }

                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var decryptedData = _cryptoUtils.DecryptData(jsonResponse, _configuration["AppSettings:enc_key"]);

                
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception during UnblockCardAsync.");

                var resp = new BlockedCard
                {
                    successful = false,
                    responseCode = "96",
                    responseMessage = "System Malfunction, please try again."
                };

                sdata = JsonConvert.SerializeObject(resp);
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                return encryptResponse;
            }
        }



        public async Task<CreatedCard> GetByUserIdAsync(string UserId)
        {
            var result = await _context.CreatedCards.FirstOrDefaultAsync(s => s.customerId == UserId);
           
            return result;
        }

        public async Task<string> TransactionDisputeAsync(TransectionDispute dis)
        {
            string To = "esupport@suntrustng.com, cashandchannelsmgt@suntrustng.com";
            var response = SunTrustProxy.SendEmail(
                    dis.Subject,
                    "notifications@suntrustng.com",
                    To,
                    dis.Message,
                    dis.Bcc,
                    dis.Cc,
                    false
                );
            var emailResponse = JsonConvert.DeserializeObject<EmailResponse>(response.ToString());

            if (emailResponse?.ResponseCode == "00")
            {


                _context.VirtualCardTransactionDisputes.Add(dis); 
                await _context.SaveChangesAsync();


                return "Dispute sent Successfully ";
            }

            return "Email sending failed!";

        }
        public async Task<CreatedCard> GetCardDetailsByProfileIdAsync(Guid profileId)
        {
            _logger.LogInformation("Fetching card details using profile ID: {ProfileId}", profileId);

            try
            {
                var userProfile = await _contxt.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.id == profileId);

                if (userProfile == null)
                {
                    _logger.LogWarning("No user profile found for ID: {ProfileId}", profileId);
                    return null;
                }
                _logger.LogInformation("cus_num type: {Type}", userProfile.cus_num.GetType().Name);

                string cusNum = userProfile.cus_num.ToString();

                var createdCard = await _context.CreatedCards
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.customerId == cusNum);

                if (createdCard == null)
                {
                    _logger.LogWarning("No created card found for cus_num: {CusNum}", cusNum);
                }

                return createdCard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching card details for profile ID: {ProfileId}", profileId);
                throw;
            }
        }



    }
}

