using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using LiBooker.Shared.DTOs;
using LiBooker.Shared.ApiResponses;
using LiBookerWasmApp.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;

namespace LiBookerWasmApp.Services.Auth
{
    public class CustomAuthStateProvider(IHttpClientFactory httpClientFactory, IBrowserStorage storage) : AuthenticationStateProvider
    {
        private const string AuthTokenKey = "authToken";
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("LiBookerApi"); 
        private readonly IBrowserStorage _storage = storage;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var rawToken = await _storage.GetItemAsync(AuthTokenKey);

            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var token = rawToken.Trim('"');

            //set token into header for all future requests
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            // find out whether it is JWT or OPAQUE token
            if (token.Split('.').Length == 3)
            {
                try // it's JWT
                {
                    var claims = ParseClaimsFromJwt(token);
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                }
                catch /* parsing failed, but token can possible still be valid OPAQUE token */
                {     /* we can return at least basic identity */
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "User") }, "jwt")));
                }
            }
            else
                return await ProcessOpaqueToken();
        }

        /// <summary>
        /// In case, a role is changed and we want to see immediate changes in Blazor, we explicitly call for it.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshUserAsync()
        {
            AuthenticationState state = await GetAuthenticationStateAsync();
            /* this way, we push Blazor to call GetAuthenticationStateAsync,
               which again call /api/auth/user-info and downloads current roles. */
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        public async Task<bool> LoginAsync(Login loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login?useCookies=false", loginModel);

            var content = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"DEBUG LOGIN STATUS: {response.StatusCode}");
            //Console.WriteLine($"DEBUG LOGIN CONTENT: {content}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // manual deserialization (CaseInsensitive)
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<AuthResponse>(content, options);

                    if (!string.IsNullOrEmpty(result?.AccessToken))
                    {
                        //Console.WriteLine("DEBUG: AccessToken found. Saving...");
                        await _storage.SetItemAsync(AuthTokenKey, result.AccessToken); // save token

                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                        return true;
                    }
                    else
                    {
                         Console.WriteLine("DEBUG: AccessToken is null or empty after deserialization.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Error during deserialization: {ex.Message}");
                }
            }
            else 
            {
                Console.WriteLine("DEBUG: Login request unsuccessful (code is not in format 2xx).");
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _storage.RemoveItemAsync(AuthTokenKey);
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<SimpleResponse> RegisterAsync(PersonRegistration registerModel)
        {
            // explicitly defined endpoint due to Person Id
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register-extended", registerModel);
            var content = await response.Content.ReadAsStringAsync();
            return new SimpleResponse
            { 
                Success = response.IsSuccessStatusCode,
                Message = content
            };
        }

        private async Task<AuthenticationState> ProcessOpaqueToken()
        {
            // Basic identity (in order to be "signed in" although token failed)
            var claims = new List<Claim>();
            try
            {
                // auth header is already set in client
                var userInfo = await _httpClient.GetFromJsonAsync<UserInfoResponse>("/api/auth/user-info");
                ParseClaims(claims, userInfo);
            }
            catch (Exception ex) // if request fails (e.g. expired token), its okay
            {
                Console.WriteLine($"DEBUG: Failed to fetch user info: {ex.Message}");
                _ = ex;
                // Fallback in case loading info failed
                if (claims.Count == 0) claims.Add(new Claim(ClaimTypes.Name, "User"));
            }

            if (claims.Count > 0)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer")));
            }
            else // if no info and token failed, then token is probably invalid/expired
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        /// <summary>
        /// Adds claims retrieved from userInfo to prepared claims structure
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="userInfo"></param>
        private static void ParseClaims(List<Claim> claims, UserInfoResponse? userInfo)
        {
            if (!string.IsNullOrEmpty(userInfo?.Email)) // name/email
            {
                claims.Add(new Claim(ClaimTypes.Name, userInfo.LoginName));
                claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));

            }
            if (userInfo?.PersonId is not null) // custom claim person ID
            {
                claims.Add(new Claim("PersonId", userInfo.PersonId.Value.ToString()));
            }
            if (userInfo?.Roles != null) // roles
            {
                foreach (var role in userInfo.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        private static List<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = new List<Claim>();

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    // DEBUG: print all found claims in encrypted token
                    //Console.WriteLine($"Token Claim Key: '{kvp.Key}', Value: '{kvp.Value}'");
                    
                    ProcessClaim(claims, kvp);
                }
            }

            return claims;
        }

        private static void ProcessClaim(List<Claim> claims, KeyValuePair<string, object> kvp)
        {
            var value = kvp.Value;
            var key = kvp.Key;

            // CLAIMS array processing (e.g. roles: ["Admin", "User"])
            if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    claims.Add(new Claim(key, item.ToString()));
                }
            }
            else
            {
                var valStr = value.ToString()!;
                claims.Add(new Claim(key, valStr));

                // mapping for UI, without this, we won't get 'Name', while we need explicit mapping:
                if (key == "email") // if this is email, add it as 'Name', in order to context.User.Identity.Name work correctly
                {
                    claims.Add(new Claim(ClaimTypes.Name, valStr));
                }
                else if (key == "sub") // if this is sub (subject ID), add it as NameIdentifier
                {
                     claims.Add(new Claim(ClaimTypes.NameIdentifier, valStr));
                }
            }
        }
        
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
             switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}