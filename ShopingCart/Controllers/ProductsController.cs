using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ShopingCart.Const;
using ShopingCart.Models;
using ShopingCart.Models.Base;
using ShopingCart.Models.Response;
using ShopingCart.Services.Base;
using System.IdentityModel.Tokens.Jwt;

namespace ShopingCart.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Settings settings;
        private readonly Service cartAPI;
        private readonly Service stockAPI;
        public ProductsController(IOptions<Settings> _settings)
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

        private static List<ProductViewModel> products = new List<ProductViewModel>
        {
            new ProductViewModel { Id = 1, Name = "Product 1", Price = 10 },
            new ProductViewModel { Id = 2, Name = "Product 2", Price = 19 },
            new ProductViewModel { Id = 3, Name = "Product 3", Price = 5 }
        };

        [HttpGet]
        public async Task<ActionResult> GetProduct(int id)
        {

            LoginResponse cookieData = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
            string authtoken = cookieData.Token.AccessToken;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(cookieData.Token.AccessToken);

            //token.Claims.ToList().Where(a => a.Type == "role").Select(x => x.Value).First() 
            ViewBag.Rights = token.Claims.ToList().Where(a => a.Type == "scope").Select(x => x.Value).ToList();

            RestClient rest = new();

            (bool, string) ProductResult = await rest.PostAPIInvoke(cartAPI.BaseURL, Routes.API_GET_PRODUCT_ID_REQUEST, id.ToString(), authtoken, Config.AUTHORIZATION_TYPE_BEARER);
            return Json(JsonConvert.DeserializeObject<ProductResponse>(ProductResult.Item2));

           
        }

        [HttpPost]
        public ActionResult SaveProduct([FromBody] ProductViewModel context)
        {
            LoginResponse cookieData = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
            string authtoken = cookieData.Token.AccessToken;

            RestClient rest = new();

            (bool, string) ProductResult = rest.PostAPIInvoke(cartAPI.BaseURL, Routes.API_SAVE_PRODUCT_REQUEST, JsonConvert.SerializeObject(context), authtoken, Config.AUTHORIZATION_TYPE_BEARER).Result;
            return Ok();
        }

        [HttpPost]
        public ActionResult UpdateProduct([FromBody] ProductViewModel context)
        {
            LoginResponse cookieData = JsonConvert.DeserializeObject<LoginResponse>(HttpContext.Request.Cookies[Config.APPLICATION_NAME]);
            string authtoken = cookieData.Token.AccessToken;

            RestClient rest = new();

            (bool, string) ProductResult = rest.PostAPIInvoke(cartAPI.BaseURL, Routes.API_UPDATE_PRODUCT_REQUEST, JsonConvert.SerializeObject(context), authtoken, Config.AUTHORIZATION_TYPE_BEARER).Result;
            return Ok();
        }
    }
}
