using System.Net.Http.Headers;
using LiBookerWasmApp.Services.Storage;

namespace LiBookerWasmApp.Services.Auth
{
    public class CustomAuthorizationMessageHandler(IBrowserStorage storage) : DelegatingHandler
    {
        private const string AuthTokenKey = "authToken";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // get token from local _storage in browser
            var rawToken = await storage.GetItemAsync(AuthTokenKey);

            if (!string.IsNullOrWhiteSpace(rawToken))
            {
                // clear token
                var token = rawToken.Trim('"');

                // adding authorization token to request header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // send request further
            return await base.SendAsync(request, cancellationToken);
        }
    }
}