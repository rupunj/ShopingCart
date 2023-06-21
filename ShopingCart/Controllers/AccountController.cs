using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ShopingCart.Const;
using ShopingCart.Enums;
using ShopingCart.Filters;
using ShopingCart.Interfaces;
using ShopingCart.Models;
using ShopingCart.Models.Base;
using ShopingCart.Models.Response;
using ShopingCart.Services;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace ShopingCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly Settings settings;
        private readonly Service cartAPI;
        private readonly Service stockAPI;
        public AccountController(IOptions<Settings> _settings)
        {
            settings = _settings.Value;
            cartAPI = new()
            {
                BaseURL = _settings.Value.CartAPI.BaseURL
            };
            stockAPI = new()
            {
                BaseURL = _settings.Value.StockAPI.BaseURL
            };
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (HttpContext.Request.Cookies.ContainsKey(Config.APPLICATION_NAME))
            {
                LoginResponse token = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]); 
                HttpContext.Request.Headers.Add(new KeyValuePair<string, StringValues>("Authorization", "Bearer " + token.Token.AccessToken));
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authenticate(LoginViewModel context)
        {
            if (!TryValidateModel(context))
                return RedirectToAction("Login");

            try
            {

                ICart cart = ServiceInit.GetCartInstance(cartAPI);
                (bool, dynamic) result = cart.AccessToken(context.Username, context.Password).Result;

                if (result.Item1)
                {
                    LoginResponse login = result.Item2;

                    if (login.Reason.Equals(AuthReason.Authenticated))
                    {
                        CookieOptions options = new()
                        {
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

                        CookieOptions uwco = new()
                        {
                            HttpOnly = true,
                            IsEssential = true,
                            Secure = true,
                            Path = "/",
                            Domain = null,
                            SameSite = SameSiteMode.Strict,
                            Expires = CartDateTime.Now.AddYears(1)
                        };

                        HttpContext.Response.Cookies.Append(Config.APPLICATION_NAME, JsonConvert.SerializeObject(cookieData), options);
                        return new Context(webRespond: true).ToContextResult();
                    }
                    else
                    {
                        if (login.Reason.Equals(AuthReason.AccountInactive))
                            return new Context("Inactive Account, Please contact your system administrator.", false).ToContextResult();
                        else if (login.Reason.Equals(AuthReason.AccountLock))
                            return new Context("Account Lock, Please contact your system administrator.", false).ToContextResult();
                        else if (login.Reason.Equals(AuthReason.PasswordExpired))
                            return new Context("Password Expired, Please contact your system administrator.", false).ToContextResult();
                        else
                            return new Context("Invalid Username or Password, Please try again.", false).ToContextResult();
                    }
                }
                else
                {
                    CartFailResponse fail = result.Item2;
                    if (fail.StatusCode == Config.STATUS_CODE_FORBIDDEN)
                        return new Context("Insufficient Privilege, Please contact your system administrator.", false).ToContextResult();
                    else
                        return new Context(result.Item2, false).ToContextResult();
                }
            }
            catch (Exception ex)
            {
                return new Context(ex.Message, false).ToContextResult();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (!HttpContext.Request.Cookies.ContainsKey(Config.APPLICATION_NAME))
                return RedirectToAction("Index", "Home");

            LoginResponse token = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
            HttpContext.Request.Headers.Remove("Authorization");
            HttpContext.Request.Headers.Add(new KeyValuePair<string, StringValues>("Authorization", "Bearer " + token.Token.AccessToken));

            ICart cart = ServiceInit.GetCartInstance(cartAPI);

            JWToken req = new()
            {
                AccessToken = token.Token.AccessToken,
                RefreshToken = token.Token.RefreshToken
            };

            await cart.RevocateToken(req);
            HttpContext.Response.Cookies.Delete(Config.APPLICATION_NAME);
            return RedirectToAction("Index", "Home");
        }

    }
}

