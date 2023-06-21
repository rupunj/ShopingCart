using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ShopingCart.Const;
using ShopingCart.Models.Base;
using ShopingCart.Models.Response;
using ShopingCart.Services.Base;
using System.IdentityModel.Tokens.Jwt;

namespace ShopingCart.Controllers
{
    public class HomeController : Controller
    {
        private readonly Settings settings;
        private readonly Service cartAPI;
        private readonly Service stockAPI;
        public HomeController(IOptions<Settings> _settings)
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
        public async Task<IActionResult> Index()
        {
            LoginResponse cookieData = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
            string authtoken = cookieData.Token.AccessToken;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(cookieData.Token.AccessToken);

            //token.Claims.ToList().Where(a => a.Type == "role").Select(x => x.Value).First() 
            ViewBag.Rights = token.Claims.ToList().Where(a => a.Type == "scope").Select(x => x.Value).ToList();

            RestClient rest = new();

            (bool, string) ProductResult = await rest.PostAPIInvoke(cartAPI.BaseURL, Routes.API_GET_PRODUCT_REQUEST, "", authtoken, Config.AUTHORIZATION_TYPE_BEARER);
            return View(JsonConvert.DeserializeObject<ProductResponse>(ProductResult.Item2));
        }
    }
}
