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

        private const int MaxRetries = 3;

        /// <summary>Ceiling for a single wait, so a Function cannot be parked indefinitely.</summary>
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);

        private static readonly Random Jitter = new Random();

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

        /// <summary>How long to wait before attempt <paramref name="attempt"/> (1-based).</summary>
        /// <remarks>
        /// A rate limit needs real backoff: the shop answers 429 until its window rolls over, so the
        /// previous fixed one second simply spent all three attempts inside the same window and then
        /// gave up - which is what turned a paid licence upgrade into a retry storm. Honour
        /// <c>Retry-After</c> when the server sends one, otherwise back off exponentially
        /// (2s, 8s, 32s) with jitter so parallel callers do not line up on the same instant.
        /// </remarks>
        private static TimeSpan GetRetryDelay(int attempt, Exception exception)
        {
            if (exception is WooCommerceApiException woo && woo.RetryAfter.HasValue)
            {
                // Trust the shop, but do not let a stray header park a Function for minutes.
                var advised = woo.RetryAfter.Value;
                return advised > MaxRetryDelay ? MaxRetryDelay : advised;
            }

            var backoff = TimeSpan.FromSeconds(Math.Pow(4, attempt) / 2);
            var jitter = TimeSpan.FromMilliseconds(Jitter.Next(0, 1000));
            var delay = backoff + jitter;

            return delay > MaxRetryDelay ? MaxRetryDelay : delay;
        }

        /// <summary>Retry only what another attempt can actually fix.</summary>
        private static bool ShouldRetry(Exception exception)
        {
            if (exception is WooCommerceApiException woo)
            {
                // A 4xx other than 429 is our own bad request - repeating it just burns quota.
                return woo.IsTransient;
            }

            // Anything that never reached the shop (socket, DNS, timeout) is worth another go.
            return exception is HttpRequestException || exception is TaskCanceledException;
        }

        public virtual async Task<string> SendHttpClientRequest<T>(string endpoint, RequestMethod method, T requestBody, Dictionary<string, string> parms = null, bool removeWcFromUrl = false)
        {
            var retryPolicy = Policy<string>
                .Handle<Exception>(ShouldRetry)
                .WaitAndRetryAsync(
                    retryCount: MaxRetries,
                    sleepDurationProvider: (attempt, outcome, _) => GetRetryDelay(attempt, outcome.Exception),
                    onRetryAsync: (_, _, _, _) => Task.CompletedTask);

            // Async all the way: the old version blocked a thread on .Result for the whole sleep,
            // which is exactly the thread a rate-limited Function cannot spare.
            var result = await retryPolicy.ExecuteAndCaptureAsync(
                () => SendHttpClientRequestInternal(endpoint, method, requestBody, parms, removeWcFromUrl));

            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }

            return result.Result;
        }

        private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
        {
            var retryAfter = response?.Headers?.RetryAfter;

            if (retryAfter == null)
            {
                return null;
            }

            if (retryAfter.Delta.HasValue)
            {
                return retryAfter.Delta.Value;
            }

            if (retryAfter.Date.HasValue)
            {
                var wait = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
            }

            return null;
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
                // The message stays the response body so existing logging is unchanged; the status
                // and Retry-After ride along so the retry policy can tell a rate limit from a failure.
                throw new WooCommerceApiException(responseMessage.StatusCode, responseResult,
                    ReadRetryAfter(responseMessage));
            }


        }
    }
}
