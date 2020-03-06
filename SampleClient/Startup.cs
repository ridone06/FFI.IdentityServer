using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;

[assembly: OwinStartup(typeof(SampleClient.Startup))]

namespace SampleClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                CookieHttpOnly = true
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = ConfigurationManager.AppSettings["Authority"],
                ClientId = ConfigurationManager.AppSettings["ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                RedirectUri = ConfigurationManager.AppSettings["RedirectUri"],
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["PostLogoutRedirectUri"],
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = "openid profile api1.full_access",
                //Scope = "offline_access openid profile api1",
                RequireHttpsMetadata = bool.Parse(ConfigurationManager.AppSettings["IsProduction"]),
                MetadataAddress = ConfigurationManager.AppSettings["Authority"] + "/.well-known/openid-configuration",
                SignInAsAuthenticationType = "Cookies",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                    {
                        var disco = await DiscoveryClient.GetAsync(ConfigurationManager.AppSettings["Authority"]);
                        // use the code to get the access and refresh token
                        var tokenClient = new TokenClient(
                                    disco.TokenEndpoint,
                                    ConfigurationManager.AppSettings["ClientId"],
                                    ConfigurationManager.AppSettings["ClientSecret"]);

                        var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }

                        // use the access token to retrieve claims from userinfo
                        var userInfoClient = new UserInfoClient(disco.UserInfoEndpoint);

                        var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);

                        // create new identity
                        var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                        id.AddClaims(userInfoResponse.Claims);

                        id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                        id.AddClaim(new Claim("expires_at", DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToString()));
                        id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken ?? ""));
                        id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        id.AddClaim(new Claim("sid", n.AuthenticationTicket.Identity.FindFirst("sid").Value));

                        n.AuthenticationTicket = new AuthenticationTicket(
                                    new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, "name", "role"),
                                    n.AuthenticationTicket.Properties);
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        // if signing out, add the id_token_hint
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }

                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}
