using System;
using System.Net;

namespace WooCommerceNET
{
    /// <summary>
    /// A non-success response from the WooCommerce REST API.
    /// </summary>
    /// <remarks>
    /// The previous code threw a bare <see cref="Exception"/> carrying only the response body, which
    /// meant a rate limit was indistinguishable from a real failure - callers just saw the nginx HTML
    /// for "429 Too Many Requests". Keeping the body as the message preserves every existing log line
    /// and <c>catch (Exception)</c>, while <see cref="StatusCode"/> and <see cref="RetryAfter"/> let the
    /// retry policy wait instead of hammering the shop.
    /// </remarks>
    public class WooCommerceApiException : Exception
    {
        public WooCommerceApiException(HttpStatusCode statusCode, string responseBody, TimeSpan? retryAfter)
            : base(responseBody)
        {
            StatusCode = statusCode;
            RetryAfter = retryAfter;
        }

        public HttpStatusCode StatusCode { get; }

        /// <summary>Value of the <c>Retry-After</c> header, when the server sent one.</summary>
        public TimeSpan? RetryAfter { get; }

        /// <summary>The shop is rate limiting us; retrying immediately only makes it worse.</summary>
        public bool IsRateLimited => (int)StatusCode == 429;

        /// <summary>Transient server-side states that are worth another attempt.</summary>
        public bool IsTransient =>
            IsRateLimited ||
            StatusCode == HttpStatusCode.BadGateway ||
            StatusCode == HttpStatusCode.ServiceUnavailable ||
            StatusCode == HttpStatusCode.GatewayTimeout;
    }
}
