using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Nexo.Shared.Models;

namespace Nexo.Client.Services
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public ApiAuthenticationStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonimo = new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var response = await _http.GetAsync("api/account/me");
                if (!response.IsSuccessStatusCode)
                    return new AuthenticationState(anonimo);

                var sesion = await response.Content.ReadFromJsonAsync<SesionActual>();
                if (sesion is null)
                    return new AuthenticationState(anonimo);

                var identity = new ClaimsIdentity(authenticationType: "ServerCookie");
                identity.AddClaim(new Claim(ClaimTypes.Name, sesion.NombreCompleto ?? sesion.UserName));
                foreach (var rol in sesion.Roles ?? System.Array.Empty<string>())
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, rol));
                }

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(anonimo);
            }
        }
    }
}
