using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ShopingCart.Const;
using ShopingCart.Interfaces;
using ShopingCart.Models.Base;
using ShopingCart.Models.Response;
using ShopingCart.Models.Resquest;
using ShopingCart.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ShopingCart.Filters
{
    public class AuthenticationFilter : IActionFilter
    {
        private readonly IConfiguration config;

        public AuthenticationFilter(IConfiguration configuration)
        {
            config = configuration;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if(!context.HttpContext.Request.Path.Value.Contains("/Account/Login", StringComparison.InvariantCultureIgnoreCase) && !context.HttpContext.Request.Path.Value.Contains("/Account/Authenticate", StringComparison.InvariantCultureIgnoreCase))
            {
                bool valid = false;

                if (context.HttpContext.Request.Cookies.ContainsKey(Config.APPLICATION_NAME))
                {
                    try
                    {
                        LoginResponse token = JsonConvert.DeserializeObject<LoginResponse>(context.HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
                        string authorityEndpoint = config.GetValue<string>("Settings:IDP:BaseURL");
                        string openIdConfigurationEndpoint = $"{authorityEndpoint}.well-known/openid-configuration";

                        IDocumentRetriever doc = new HttpDocumentRetriever()
                        {
                            RequireHttps = false
                        };

                        IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigurationEndpoint, new OpenIdConnectConfigurationRetriever(), doc);
                        OpenIdConnectConfiguration openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

                        JwtSecurityTokenHandler tokenHandler = new();
                        tokenHandler.ValidateToken(token.Token.AccessToken, new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKeys = openIdConfig.SigningKeys,
                            ValidateIssuer = true,
                            ValidIssuer = openIdConfig.Issuer,
                            ValidateAudience = true,
                            ValidAudience = Config.APPLICATION_NAME,
                            ClockSkew = TimeSpan.Zero
                        }, out SecurityToken validatedToken);

                        if (context.HttpContext.Request.Headers.ContainsKey("Authorization"))
                            context.HttpContext.Request.Headers.Remove("Authorization");

                        context.HttpContext.Request.Headers.Add("Authorization", "Bearer " + token.Token.AccessToken);

                        valid = true;
                    }
                    catch (Exception ex)
                    {
                        LoginResponse oldToken = JsonConvert.DeserializeObject<LoginResponse>(context.HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
                        context.HttpContext.Response.Cookies.Delete(Config.APPLICATION_NAME);

                        if (ex is SecurityTokenExpiredException)
                        {
                            if (oldToken.Token.RefreshToken != null && oldToken.Token.RefreshToken.Length > 0)
                            {
                                RefreshTokenRequest refReq = new()
                                {
                                    RefreshToken = oldToken.Token.RefreshToken
                                };

                                try
                                {
                                    Service providerInfo = new()
                                    {
                                        BaseURL = config.GetValue<string>("Settings:CartAPI:BaseURL")
                                    };

                                    ICart cart = ServiceInit.GetCartInstance(providerInfo);
                                    (bool, dynamic) result = cart.RefreshToken(refReq).Result;

                                    if (result.Item1)
                                    {
                                        LoginResponse login = result.Item2;

                                        CookieOptions options = new()
                                        {
                                            Expires = CartDateTime.Now.AddDays(30),
                                            HttpOnly = true,
                                            IsEssential = true,
                                            Secure = true,
                                            Path = "/",
                                            Domain = null,
                                            SameSite = SameSiteMode.Strict
                                        };

                                        LoginResponse cookieData = new()
                                        {
                                            Token = login.Token
                                        };

                                        context.HttpContext.Response.Cookies.Append(Config.APPLICATION_NAME, JsonConvert.SerializeObject(cookieData), options);
                                        if (context.HttpContext.Request.Headers.ContainsKey("Authorization"))
                                            context.HttpContext.Request.Headers.Remove("Authorization");

                                        context.HttpContext.Request.Headers.Add("Authorization", "Bearer " + cookieData.Token.AccessToken);
                                        valid = true;
                                    }
                                }
                                catch { }
                            }
                        }
                        else
                            while (ex.InnerException != null) ex = ex.InnerException;
                    }
                }

                if (!valid)
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }            
        }
    }
}
