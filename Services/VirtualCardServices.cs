using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http.Headers;
using System.Text;
using VirtualCard.Data;
using VirtualCard.Help;
using VirtualCard.Request;
using VirtualCard.TokenResponses;
using VisualCard.Helper;
using VisualCard.Interface;
using VisualCard.Model;
using RestClientOptions = RestSharp.RestClientOptions;
/*using static SunTrustProxy;
using Org.BouncyCastle.Asn1.Ocsp;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
*/
namespace VisualCard.Services
{
    public class VirtualCardServices : IVirtualCard
    {



        public readonly HttpClient _httpClient;
        public readonly ILogger<VirtualCardServices> _logger;
        public readonly VirtualCardDbContext _context;

        private readonly IConfiguration _configuration;
        private readonly ICryptoUtils _cryptoUtils;
        private readonly GenerateTokens _generateToken;
        private readonly RestClient _client;
        private readonly IDataEncryption _dataEncryption;

        public VirtualCardServices(VirtualCardDbContext context, ILogger<VirtualCardServices> logger, GenerateTokens generateToken, IDataEncryption dataEncryption,
            IConfiguration configuration, HttpClient httpClient, ICryptoUtils cryptoUtils)
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
        }

        public async Task<EncryptResponse> BlockCardAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                // string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:denc_key"]);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<BlockCard>(decrypt);
                /*if (decrypted?.cards == null)
                    throw new Exception("Decrypted object or cards property is null.");*/
                var newRequest = new BlockedCard
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                // Serialize the request
                /* var json = JsonConvert.SerializeObject(BlockedCards);
                 var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                 var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/cardBlock", Method.Post) // Corrected endpoint
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

                // return decryptedData;

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

        public async Task<EncryptResponse> ChangeCardPinAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {

                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<ChangePinRequest>(decrypt);

                /*var json = JsonConvert.SerializeObject(pinChangeRequest);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/

                var newRequest = new ChangePinRequest
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference,
                    oldPin = decrypted.oldPin,
                    newPin = decrypted.newPin,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/pinChange", Method.Post) // Corrected endpoint
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

                //return decryptedData;
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

                var newRequest = new ChangePinRequest
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference,


                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                /*var json = JsonConvert.SerializeObject(ResetPinRequests);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/pinReset", Method.Post) // Corrected endpoint
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


        public async Task<EncryptResponse> GetCardStatusAsync(EncryptRequest encryptRequest)//CardStatusRequest erequest)
        {
            //StandardResponse response = new StandardResponse();
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                /* if (decrypt == "99")
                 {
                     var resp = new
                     {
                         statuscode = "96",
                         statusmessage = "System Malfunction"
                     };
                     sdata = JsonConvert.SerializeObject(resp);
                     encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(sdata);

                     return encryptResponse;
                 }*/

                var newRequest = new
                {
                    cardReference = decrypt
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);

                //var json = JsonConvert.SerializeObject(decrypt);
                //var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                //var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);

                string encryptedData = _cryptoUtils.EncryptData(deserialize, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/cardStatus", Method.Post) // Corrected endpoint
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


                //var sdata2 = JsonConvert.SerializeObject(decryptedData);
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

        public async Task<EncryptResponse> CreateCard2Async(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                // Generate a unique client reference
                var clientReference = Guid.NewGuid().ToString();
                encryptRequest.GetType().GetProperty("clientReference")?.SetValue(encryptRequest, clientReference);

                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<CreateCard>(decrypt);

                _logger.LogInformation("Validating account ownership for userId {UserId}", decrypted.userId);

                var ownershipValidationResult = SunTrustProxy.getAccountBy_CUS_NUM(decrypted.userId);

                if (ownershipValidationResult == null || ownershipValidationResult?.responseCode != ResponseCode.SUCCESSFUL)
                {
                    _logger.LogWarning("Ownership validation failed for userId {UserId}", decrypted.userId);
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("Account does not belong to the user. Card creation denied.");
                    return encryptResponse;
                }
               /* var ownershipValidationResult = SunTrustProxy.getAccountBy_CUS_NUM(decrypted.userId);
                *//*if (!ownershipValidationResult..ToLower().Contains(decrypted.firstName.ToLower()) ||
    !ownershipValidationResult.AccountName.ToLower().Contains(decrypted.lastName.ToLower()))*//*
                if (ownershipValidationResult == null || ownershipValidationResult.responseCode != ResponseCode.SUCCESSFUL)

                {
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("Account does not belong to the user. Card creation denied.");
                    return encryptResponse;
                }*/

                var newRequest = new CreateCard
                {
                    lastName = decrypted.lastName,
                    city = decrypted.city,
                    accountType = decrypted.accountType,
                    postalCode = decrypted.postalCode,
                    streetAddressLine2 = decrypted.streetAddressLine2,
                    userId = decrypted.userId,
                    mobileNr = decrypted.mobileNr,
                    cardProgram = decrypted.cardProgram,
                    firstName = decrypted.firstName,
                    accountId = decrypted.accountId,
                    emailAddress = decrypted.emailAddress,
                    nameOnCard = decrypted.nameOnCard,
                    streetAddress = decrypted.streetAddress,
                    countryCode = decrypted.countryCode,
                    issuerNr = decrypted.issuerNr,
                    state = decrypted.state,
                    currencyCode = decrypted.currencyCode,
                    alias = decrypted.alias,
                    clientReference = clientReference,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                // Serialize the request
                /*var json = JsonConvert.SerializeObject(CreateCards);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/

                // Encrypt the request data
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:denc_key"]);

                // Validate Base64 format
                /*if (!IsBase64String(encryptedData))
                    throw new Exception("Encryption error: Encrypted data is not a valid Base64 string.");*/


                // Get a fresh access token
                var tokenResponse = await _generateToken.GetToken2();

                // Create the HTTP client and request
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var client = new RestClient(baseUrl);
               //
                var request = new RestRequest("/virtualcard/api/v2/createCard", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID2"])
                    .AddBody(encryptedData);  // Ensure proper formatting

                // Send the request
                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                // Decrypt the response data
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:denc_key"]);
                var decryptedCardResponse = JsonConvert.DeserializeObject<Root>(decryptedData);
                var decryptedCardData = new CreatedCard
                {
                    alias = decryptedCardResponse.card.alias,
                    clientReference = decryptedCardResponse.card.clientReference,
                    cardReference = decryptedCardResponse.card.cardReference,
                    accountNumber = decryptedCardResponse.card.accountNumber,
                    pan = MaskPan(decryptedCardResponse.card.pan),
                    seqNr = decryptedCardResponse.card.seqNr,
                    issuerNr = decryptedCardResponse.card.issuerNr,
                    userId = decryptedCardResponse.card.userId,
                    pinOffset = decryptedCardResponse.card.pinOffset,
                    customerId = decryptedCardResponse.card.customerId,
                    defaultAccountType = decryptedCardResponse.card.defaultAccountType,
                    blocked = decryptedCardResponse.card.blocked,
                    failedPinAttempts = decryptedCardResponse.card.failedPinAttempts,
                    creationChannel = decryptedCardResponse.card.creationChannel,


                };
                // Add to DbContext and save
                await _context.CreatedCards.AddAsync(decryptedCardData);
                await _context.SaveChangesAsync();

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                //return decryptedData;
            }
            /* catch (Exception ex)
             {
                 throw new Exception($"Virtual Card Creation Failed: {ex.Message}");
             }*/
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
        public async Task<EncryptResponse> CreateCardAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                // Generate a unique client reference
                var clientReference = Guid.NewGuid().ToString();
                encryptRequest.GetType().GetProperty("clientReference")?.SetValue(encryptRequest, clientReference);

                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<CreateCard>(decrypt);

                // ✅ Step: Validate account ownership


                _logger.LogInformation("Validating account ownership for userId {UserId}", decrypted.userId);

                var ownershipValidationResult = SunTrustProxy.getAccountBy_CUS_NUM(decrypted.userId);

                if (ownershipValidationResult == null || ownershipValidationResult?.responseCode != ResponseCode.SUCCESSFUL)
                {
                    _logger.LogWarning("Ownership validation failed for userId {UserId}", decrypted.userId);
                    encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes("Account does not belong to the user. Card creation denied.");
                    return encryptResponse;
                }


                var newRequest = new CreateCard
                {
                    lastName = decrypted.lastName,
                    city = decrypted.city,
                    accountType = decrypted.accountType,
                    postalCode = decrypted.postalCode,
                    streetAddressLine2 = decrypted.streetAddressLine2,
                    userId = decrypted.userId,
                    mobileNr = decrypted.mobileNr,
                    cardProgram = decrypted.cardProgram,
                    firstName = decrypted.firstName,
                    accountId = decrypted.accountId,
                    emailAddress = decrypted.emailAddress,
                    nameOnCard = decrypted.nameOnCard,
                    streetAddress = decrypted.streetAddress,
                    countryCode = decrypted.countryCode,
                    issuerNr = decrypted.issuerNr,
                    state = decrypted.state,
                    currencyCode = decrypted.currencyCode,
                    alias = decrypted.alias,
                    clientReference = clientReference,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);

                // Serialize the request
                /*var json = JsonConvert.SerializeObject(CreateCards);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/
                // Encrypt the request data
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:denc_key"]);
                //string decryptedData = _cryptoUtils.DecryptData(encryptedData, _configuration["AppSettings:denc_key"]);

                // Validate Base64 format
                /*if (!IsBase64String(encryptedData))
                    throw new Exception("Encryption error: Encrypted data is not a valid Base64 string.");*/

                // Get a fresh access token
                var tokenResponse = await _generateToken.GetToken2();

                // Create the HTTP client and request
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var client = new RestClient(baseUrl);
                //var client = new RestClient("");
                var request = new RestRequest("/virtualcard/api/v2/createCard", Method.Post)
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

                // Decrypt the response data
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:denc_key"]);
                var decryptedCardResponse = JsonConvert.DeserializeObject<Roots>(decryptedData);
                var decryptedCardData = new CreatedCard
                {
                    alias = decryptedCardResponse.card.alias,
                    clientReference = decryptedCardResponse.card.clientReference,
                    cardReference = decryptedCardResponse.card.cardReference,
                    accountNumber = decryptedCardResponse.card.accountNumber,
                    pan = MaskPan(decryptedCardResponse.card.pan),
                    seqNr = decryptedCardResponse.card.seqNr,
                    issuerNr = decryptedCardResponse.card.issuerNr,
                    userId = decryptedCardResponse.card.userId,
                    pinOffset = decryptedCardResponse.card.pinOffset,
                    customerId = decryptedCardResponse.card.customerId,
                    defaultAccountType = decryptedCardResponse.card.defaultAccountType,
                    blocked = decryptedCardResponse.card.blocked,
                    failedPinAttempts = decryptedCardResponse.card.failedPinAttempts,
                    creationChannel = decryptedCardResponse.card.creationChannel,

                };
                // Add to DbContext and save
                await _context.CreatedCards.AddAsync(decryptedCardData);
                await _context.SaveChangesAsync();
                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptedData);

                return encryptResponse;
                //return decryptedData;
            }
            /*catch (Exception ex)
            {
                throw new Exception($"Virtual Card Creation Failed: {ex.Message}");
            }*/
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
                /* var json = JsonConvert.SerializeObject(erequest);
                 var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                 var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/
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

                var newRequest = new FetchCardRefandPin
                {
                    clientReference = decrypted.clientReference,
                    pin = decrypted.pin,
                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                // Generate a unique client reference
                //var clientReference = Guid.NewGuid().ToString();
                //req.GetType().GetProperty("clientReference")?.SetValue(req, clientReference);

                // Serialize the request
                /*var json = JsonConvert.SerializeObject(req);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/

                // Encrypt the request data
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // Validate Base64 format
                /*if (!IsBase64String(encryptedData))
                    throw new Exception("Encryption error: Encrypted data is not a valid Base64 string.");*/

                // Get a fresh access token
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

                // Decrypt the response data
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



        public async Task<EncryptResponse> FetchCardIncludedAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<FetchCardRequest1>(decrypt);

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
                /* var json = JsonConvert.SerializeObject(erequest);
                 var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                 var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // 3. Validate Base64 encoding


                // 4. Retrieve a fresh access to
                var tokenResponse = await _generateToken.GetToken2();

                // 5. Create HTTP client and request
                var client = new RestClient("https://virtualcard.interswitchng.com");
                var request = new RestRequest("/virtualcard/api/v1/statement", Method.Post) // Corrected endpoint
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

        public async Task<EncryptResponse> UnblockCardAsync(EncryptRequest encryptRequest)
        {
            EncryptResponse encryptResponse = new EncryptResponse();
            var sdata = string.Empty;
            try
            {
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(encryptRequest.Request);
                var decrypted = JsonConvert.DeserializeObject<UnBlockCard>(decrypt);

                var newRequest = new ChangePinRequest
                {
                    accountNumber = decrypted.accountNumber,
                    cardReference = decrypted.cardReference,


                };

                var deserialize = JsonConvert.SerializeObject(newRequest);
                /*var json = JsonConvert.SerializeObject(UnBlockedCards);
                var businesstypeencrypt = _dataEncryption.EncryptStringToBytes_Aes(json);
                var decrypt = _dataEncryption.DecryptStringFromBytes_Aes(businesstypeencrypt);*/

                // Encrypt the request data and ensure proper encoding
                string encryptedData = _cryptoUtils.EncryptData(decrypt, _configuration["AppSettings:enc_key"]);

                // Validate Base64 format


                // Get a fresh access token
                var tokenResponse = await _generateToken.GetToken2();
                var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/cardUnblock";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

                // Set required headers
                httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
                httpRequest.Headers.Add("Accept", "application/json");
                httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
                httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

                // Set request content FIRST before modifying headers
                httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

                // Now modify Content-Type correctly
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

                // Send the request
                var response = await _httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var jsonresponse = await response.Content.ReadAsStringAsync();
                string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);

                encryptResponse.Response = _dataEncryption.EncryptStringToBytes_Aes(decryptdata);

                return encryptResponse;
                //return decryptdata;
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

        public async Task<CreatedCard> GetByUserIdAsync(string UserId)
        {
            var result = await _context.CreatedCards.FirstOrDefaultAsync();
            if (result != null)
            {
                var staff = await _context.CreatedCards.FirstOrDefaultAsync(s => s.userId == UserId);

            }
            return result;
        }

        public async Task<string> TransactionDisputeAsync(TransectionDispute dis)
        {
            string To = "jocrossonyejo@gmail.com";//"esupport@suntrustng.com, cashandchannelsmgt@suntrustng.com";
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

            // Check if response is successful
            if (emailResponse?.ResponseCode == "00")
            {
                // Save the dispute to the database


                _context.VirtualCardTransactionDisputes.Add(dis); // Use context instead of _context
                await _context.SaveChangesAsync();


                return "Dispute sent Successfully ";
            }

            return "Email sending failed!";

        }



    }
}

