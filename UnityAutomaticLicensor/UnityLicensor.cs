using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnityAutomaticLicensor
{
    public class UnityLicensor
    {
        private readonly UnityLicensorRequest _request;

        public UnityLicensor(UnityLicensorRequest request)
        {
            _request = request;
        }

        public async Task Run()
        {
            var licensePath = $@"C:\ProgramData\Unity\Unity_{_request.UnityVersion}.ulf";

            var licenseKeyCheck = await RunUnityAndCaptureMachineKeys();
            if (licenseKeyCheck.IsActivated)
            {
                Console.WriteLine("Unity has already been licensed, no need to obtain new license!");
                return;
            }

            Console.WriteLine("Logging into Unity Cloud...");
            var coreClient = new RestClient("https://core.cloud.unity3d.com");
            var loginRequest = new RestRequest("api/login", Method.POST);
            loginRequest.AddCookie("unity_version", "5.4.1f1");
            loginRequest.AddCookie("unity_version_full", "5.4.1f1 (649f48bbbf0f)");
            loginRequest.AddJsonBody(new
            {
                grant_type = "password",
                username = _request.Username,
                password = _request.Password,
            });
            var response = await coreClient.ExecuteTaskAsync(loginRequest);
            var loginResponse = JsonConvert.DeserializeObject<UnityCloudLoginResponse>(response.Content);

            Console.WriteLine("Login response:");
            Console.WriteLine(response.Content);

            Console.WriteLine("Discovering user info for licensing...");
            var meRequest = new RestRequest("api/users/me", Method.GET);
            meRequest.AddCookie("unity_version", "5.4.1f1");
            meRequest.AddCookie("unity_version_full", "5.4.1f1 (649f48bbbf0f)");
            meRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
            response = await coreClient.ExecuteTaskAsync(meRequest);
            var userResponse = JsonConvert.DeserializeObject<UnityCloudUserResponse>(response.Content);

            Console.WriteLine("User response:");
            Console.WriteLine(response.Content);

            Console.WriteLine("Sending poll request to licensing server with machine keys...");
            var txId = GenerateTxId();
            var licenseClient = new RestClient("https://license.unity3d.com");
            var pollRequest = new RestRequest("update/poll", Method.POST);
            pollRequest.AddParameter("cmd", "9", ParameterType.QueryString);
            pollRequest.AddParameter("tx_id", txId, ParameterType.QueryString);
            pollRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
            pollRequest.AddHeader("Content-Type", "text/xml");
            pollRequest.AddParameter("text/xml", licenseKeyCheck.PostedLicenseAttemptXml, ParameterType.RequestBody);
            response = await licenseClient.ExecuteTaskAsync(pollRequest);
            // Pretty sure the XML response isn't used any more.

            Console.WriteLine("Poll request response:");
            Console.WriteLine(response.Content);

            UnityLicenseTransactionResponse licenseResponse = null;
            for (var i = 0; i < 30; i++)
            {
                Console.WriteLine("Sending initial license request to licensing server...");
                var licenseRequest = new RestRequest("api/transactions/{txId}", Method.PUT);
                licenseRequest.AddUrlSegment("txId", txId);
                licenseRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
                licenseRequest.AddJsonBody(new
                {
                    transaction = new
                    {
                        serial = new
                        {
                            type = "personal"
                        }
                    }
                });
                response = await licenseClient.ExecuteTaskAsync(licenseRequest);
                licenseResponse = JsonConvert.DeserializeObject<UnityLicenseTransactionResponse>(response.Content);

                Console.WriteLine("Licensing response:");
                Console.WriteLine(response.Content);

                if (licenseResponse?.Transaction == null)
                {
                    Console.WriteLine("Licensing didn't include transaction in response, retrying soon...");
                    await Task.Delay(i * 1000);
                }
                else
                {
                    break;
                }
            }

            if (licenseResponse.Transaction.Survey.Required && !licenseResponse.Transaction.Survey.Answered)
            {
                // Fake a survey response so we can move forward in the licensing.
                using (var client = new HttpClient())
                {
                    Console.WriteLine("Unity requires a survey to be filled in, just randomly selecting options and adding junk data...");

                    var surveyRequest = JsonConvert.DeserializeObject<UnitySurvey>(await client.GetStringAsync(licenseResponse.Transaction.Survey.Url));
                    var surveyAnswers = new Dictionary<int, object>();

                    foreach (var option in surveyRequest.Questions)
                    {
                        switch (option.Widget)
                        {
                            case "country":
                                surveyAnswers[option.Id] = new
                                {
                                    cn_province = "",
                                    country = "AU",
                                    province = "",
                                    state = "",
                                };
                                break;
                            case "radio":
                                {
                                    var o = new JObject();
                                    o["0"] = option.Options.First().Id.ToString();
                                    surveyAnswers[option.Id] = o;
                                    break;
                                }
                            case "checkbox":
                                {
                                    var o = new JObject();
                                    o["0"] = option.Options.First().Id.ToString();
                                    surveyAnswers[option.Id] = o;
                                    break;
                                }
                            case "select":
                                {
                                    var o = new JObject();
                                    o["0"] = option.Options.First().Id.ToString();
                                    surveyAnswers[option.Id] = o;
                                    break;
                                }
                            case "string":
                                surveyAnswers[option.Id] = "N/A";
                                break;
                        }
                    }

                    var surveyBody = new
                    {
                        transaction = new
                        {
                            serial = new
                            {
                                type = "personal"
                            },
                            survey_answer = new
                            {
                                skipped = false,
                                answers = surveyAnswers,
                            }
                        }
                    };

                    Console.WriteLine("Submitting request with survey answers:");
                    Console.WriteLine(JsonConvert.SerializeObject(surveyBody, Formatting.Indented));

                    Console.WriteLine("Sending follow-up license request with survey results to licensing server...");
                    var licenseRequest = new RestRequest("api/transactions/{txId}", Method.PUT);
                    licenseRequest.AddUrlSegment("txId", txId);
                    licenseRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
                    licenseRequest.AddJsonBody(surveyBody);
                    response = await licenseClient.ExecuteTaskAsync(licenseRequest);
                    licenseResponse = JsonConvert.DeserializeObject<UnityLicenseTransactionResponse>(response.Content);

                    Console.WriteLine("Licensing response with survey:");
                    Console.WriteLine(response.Content);
                }
            }
            else if (!licenseResponse.Transaction.Survey.Required && !licenseResponse.Transaction.Survey.Answered)
            {
                Console.WriteLine("Sending follow-up license request with skipped survey to licensing server...");
                var licenseRequest = new RestRequest("api/transactions/{txId}", Method.PUT);
                licenseRequest.AddUrlSegment("txId", txId);
                licenseRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
                licenseRequest.AddJsonBody(new
                {
                    transaction = new
                    {
                        serial = new
                        {
                            type = "personal"
                        },
                        survey_answer = new
                        {
                            skipped = true,
                        }
                    }
                });
                response = await licenseClient.ExecuteTaskAsync(licenseRequest);
                licenseResponse = JsonConvert.DeserializeObject<UnityLicenseTransactionResponse>(response.Content);

                Console.WriteLine("Licensing response with skipped survey:");
                Console.WriteLine(response.Content);
            }

            Console.WriteLine("Sending download request to activation server...");
            var activationClient = new RestClient("https://activation.unity3d.com");
            var activationRequest = new RestRequest("license.fcgi", Method.POST);
            activationRequest.AddParameter("CMD", "9", ParameterType.QueryString);
            activationRequest.AddParameter("TX", txId, ParameterType.QueryString);
            activationRequest.AddParameter("RX", licenseResponse.Transaction.Rx, ParameterType.QueryString);
            activationRequest.AddHeader("Authorization", "Bearer " + loginResponse.AccessToken);
            activationRequest.AddParameter("text/xml", licenseKeyCheck.PostedLicenseAttemptXml, ParameterType.RequestBody);
            response = await activationClient.ExecuteTaskAsync(activationRequest);
            var licenseContent = response.Content;
            
            Console.WriteLine("Activation response:");
            Console.WriteLine(licenseContent);

            using (var writer = new StreamWriter(new FileStream(licensePath, FileMode.Create, FileAccess.Write)))
            {
                await writer.WriteAsync(licenseContent);
            }
            Console.WriteLine("Successfully obtained a Unity license!");

            Console.WriteLine("Finalising license by running Unity...");
            var finaliseResponse = await RunUnityToFinaliseLicense();
            if (finaliseResponse.IsActivated)
            {
                Console.WriteLine("Successfully finalised Unity license!");
                return;
            }
            
            throw new InvalidOperationException("Unable to finalise Unity license!");
        }

        private string GenerateTxId()
        {
            using (var sha = SHA1.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString()))).Replace("-", "");
            }
        }

        private Regex _machineKeyCapture = new Regex("Posting (.*)$", RegexOptions.Multiline);
        private Regex _nextLicenseCheck = new Regex("Next license update check is after", RegexOptions.Multiline);

        private async Task<UnityLicenseStatusCheck> RunUnityAndCaptureMachineKeys()
        {
            var executor = new UnityExecutor();
            for (var i = 0; i < 30; i++)
            {
                var response = await executor.ExecuteAsync(new UnityExecutorRequest
                {
                    UnityExecutablePath = _request.UnityExecutablePath,
                    ArgumentList =
                    {
                        "-quit",
                        "-batchmode",
                        "-username",
                        _request.Username,
                        "-password",
                        _request.Password,
                        "-force-free"
                    },
                    CustomBufferHandler = (buffer) =>
                    {
                        if (_machineKeyCapture.IsMatch(buffer))
                        {
                            return Task.FromResult((UnityExecutorResponseResult?)UnityExecutorResponseResult.Retry);
                        }
                        if (_nextLicenseCheck.IsMatch(buffer))
                        {
                            return Task.FromResult((UnityExecutorResponseResult?)UnityExecutorResponseResult.Success);
                        }

                        return Task.FromResult((UnityExecutorResponseResult?)null);
                    }
                });

                if (response.Result == UnityExecutorResponseResult.Retry && _machineKeyCapture.IsMatch(response.Output))
                {
                    Console.WriteLine("Capturing machine keys response required for license requests...");
                    return new UnityLicenseStatusCheck
                    {
                        IsActivated = false,
                        PostedLicenseAttemptXml = _machineKeyCapture.Match(response.Output).Groups[1].Value,
                    };
                }
                else if (response.Result == UnityExecutorResponseResult.Success)
                {
                    return new UnityLicenseStatusCheck
                    {
                        IsActivated = true,
                    };
                }
                else if (response.Result == UnityExecutorResponseResult.Error)
                {
                    throw new InvalidOperationException(response.Output);
                }
            }

            throw new InvalidOperationException("Unity didn't provide us with machine keys after 30 licensing attempts...");
        }
        
        private async Task<UnityLicenseStatusCheck> RunUnityToFinaliseLicense()
        {
            var executor = new UnityExecutor();
            for (var i = 0; i < 30; i++)
            {
                var response = await executor.ExecuteAsync(new UnityExecutorRequest
                {
                    UnityExecutablePath = _request.UnityExecutablePath,
                    ArgumentList =
                    {
                        "-quit",
                        "-batchmode",
                        "-username",
                        _request.Username,
                        "-password",
                        _request.Password,
                        "-force-free"
                    }
                });
                
                if (response.Result == UnityExecutorResponseResult.Success)
                {
                    return new UnityLicenseStatusCheck
                    {
                        IsActivated = true,
                    };
                }
                else if (response.Result == UnityExecutorResponseResult.Error)
                {
                    throw new InvalidOperationException(response.Output);
                }
            }

            throw new InvalidOperationException("Unity didn't provide us with machine keys after 30 licensing attempts...");
        }

        private class UnityLicenseStatusCheck
        {
            public bool IsActivated { get; set; }

            public string PostedLicenseAttemptXml { get; set; }
        }
    }
}
