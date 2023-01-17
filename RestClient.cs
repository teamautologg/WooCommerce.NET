using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerceNET
{
    /// <summary>
    /// Rest API class to use HTTPClient instead of HttpWebRequest.
    /// </summary>
    public class RestClient : RestAPI
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use.
        private readonly HttpClient HttpClient;

        public RestClient(string url, string key, string secret, bool authorizedHeader = true, Dictionary<string, string> cookies = null,
                            string basicUser = null, string basicPassword = null,
                          Func<string, string> jsonSerializeFilter = null,
                          Func<string, string> jsonDeserializeFilter = null)
          : base(url, key, secret, authorizedHeader, cookies, basicUser, basicPassword, jsonSerializeFilter, jsonDeserializeFilter)
        {
            //TODO - Need to work on requestFilter and responseFilter
            if(_cookieContainer != null)
            {
                var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                HttpClient =  new HttpClient(handler);
            }
            else
            {
                HttpClient = new HttpClient();
            }
        }

        public virtual async Task<string> SendHttpClientRequest<T>(string endpoint, RequestMethod method, T requestBody, Dictionary<string, string> parms = null, bool removeWcFromUrl = false)
        {
            var retryPolicy = Policy<string>.Handle<Exception>().WaitAndRetry(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));

            await Task.CompletedTask;
            var result = retryPolicy.ExecuteAndCapture(() =>
            {
                return SendHttpClientRequestInternal(endpoint, method, requestBody, parms, removeWcFromUrl).Result;
            });

            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }

            return result.Result;
        }


        private async Task<string> SendHttpClientRequestInternal<T>(string endpoint, RequestMethod method, T requestBody, Dictionary<string, string> parms = null, bool removeWcFromUrl = false)
        {
            HttpRequestMessage request = new HttpRequestMessage();


            if (Version == APIVersion.WordPressAPI)
            {
                if (string.IsNullOrEmpty(oauth_token) || string.IsNullOrEmpty(oauth_token_secret))
                    throw new Exception($"oauth_token and oauth_token_secret parameters are required when using WordPress REST API.");
            }

            if ((Version == APIVersion.WordPressAPIJWT || WCAuthWithJWT) && JWT_Object == null)
            {
                request.RequestUri = new Uri(wc_url.Replace("wp/v2", "jwt-auth/v1/token")
                                                                                    .Replace("wc/v1", "jwt-auth/v1/token")
                                                                                    .Replace("wc/v2", "jwt-auth/v1/token")
                                                                                    .Replace("wc/v3", "jwt-auth/v1/token"));


                request.Method = HttpMethod.Post;

                //if (JWTRequestFilter != null)
                //    JWTRequestFilter.Invoke(request);                   

                request.Content = new StringContent($"username={wc_key}&password={WebUtility.UrlEncode(wc_secret)}",
                                           Encoding.UTF8,
                                           "application/x-www-form-urlencoded");

                var jwtResponseMessage = await HttpClient.SendAsync(request);

                //Check for responseMessage.IsSuccessStatusCode
                string result = await jwtResponseMessage.Content.ReadAsStringAsync();

                if (JWTDeserializeFilter != null)
                    result = JWTDeserializeFilter.Invoke(result);

                JWT_Object = DeserializeJSon<WP_JWT_Object>(result);
            }

            if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase) && Version != APIVersion.WordPressAPI && Version != APIVersion.WordPressAPIJWT)
            {
                if (AuthorizedHeader == true)
                {
                    request.RequestUri = new Uri(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));

                    if (WCAuthWithJWT && JWT_Object != null)
                        request.Headers.Add("Authorization", "Bearer " + JWT_Object.token);
                    else
                        request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(wc_key + ":" + wc_secret)));

                }
                else
                {
                    if (parms == null)
                        parms = new Dictionary<string, string>();

                    if (!parms.ContainsKey("consumer_key"))
                        parms.Add("consumer_key", wc_key);
                    if (!parms.ContainsKey("consumer_secret"))
                        parms.Add("consumer_secret", wc_secret);

                    request.RequestUri = new Uri(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                }
            }
            else
            {
                request.RequestUri = new Uri(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                if (Version == APIVersion.WordPressAPIJWT)
                    request.Headers.Add("Authorization", "Bearer " + JWT_Object.token);
            }


            request.Method = new HttpMethod(method.ToString());
            if (!String.IsNullOrEmpty(basicAuth))
            {
                request.Headers.Add("Authorization", $"Basic {basicAuth}");
            }

            //if (webRequestFilter != null)
            //    webRequestFilter.Invoke(httpWebRequest);


            if (requestBody != null && !string.IsNullOrWhiteSpace(requestBody.ToString()))
            {
                if (requestBody.GetType() == typeof(string))
                {
                    if (requestBody.ToString() == "fileupload")
                    {

                    }
                    else
                    {
                        request.Content = new StringContent(requestBody.ToString(),
                                          Encoding.UTF8,
                                          "application/json");
                    }
                }
                else
                {
                    request.Content = new StringContent(SerializeJSon(requestBody),
                                           Encoding.UTF8,
                                           "application/json");
                }
            }


            var responseMessage = await HttpClient.SendAsync(request);
            string responseResult = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.IsSuccessStatusCode)
            {
                return responseResult;
            }
            else
            {
                throw new Exception(responseResult);
            }


        }
    }
}
