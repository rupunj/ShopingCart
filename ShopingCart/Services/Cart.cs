using Newtonsoft.Json;
using ShopingCart.Const;
using ShopingCart.Interfaces;
using ShopingCart.Models.Base;
using ShopingCart.Models.Response;
using ShopingCart.Models.Resquest;
using ShopingCart.Services.Base;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace ShopingCart.Services
{
    public class Cart : ICart
    {
        private protected Service Provider { get; set; }

        public Cart(Service config)
        {
            Provider = config;
        }

        public async Task<(bool, dynamic)> AccessToken(string username, string password)
        {
            string authToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

            RestClient rest = new();
            (bool, string) loginResult = await rest.PostAPIInvoke(Provider.BaseURL, Routes.API_ACCESS_TOKEN_REQUEST, "", authToken, Config.AUTHORIZATION_TYPE_BASIC);

            if (loginResult.Item1)
                return (true, JsonConvert.DeserializeObject<LoginResponse>(loginResult.Item2));
            else
                return (false, new CartFailResponse() { StatusCode = rest.StatusCode, Message = loginResult.Item2 });
        }

        public async Task<(bool, dynamic)> RefreshToken(RefreshTokenRequest context)
        {
            RestClient rest = new();
            (bool, string) result = await rest.PostAPIInvoke(Provider.BaseURL, Routes.API_REFRESH_TOKEN_REQUEST, JsonConvert.SerializeObject(context), "", Config.AUTHORIZATION_TYPE_BASIC);
            if (result.Item1)
                return (true, JsonConvert.DeserializeObject<JWToken>(result.Item2));
            else
                return (false, new CartFailResponse() { StatusCode = rest.StatusCode, Message = result.Item2 });
        }
        public async Task<(bool, dynamic)> RevocateToken(JWToken context)
        {
            RestClient rest = new();
            (bool, string) result = await rest.PostAPIInvoke(Provider.BaseURL, Routes.API_TOKEN_RECOVATION_REQUEST, JsonConvert.SerializeObject(context), "", Config.AUTHORIZATION_TYPE_BASIC);
            if (result.Item1)
                return (true, JsonConvert.DeserializeObject<string>(result.Item2));
            else
                return (false, new CartFailResponse() { StatusCode = rest.StatusCode, Message = result.Item2 });
        }
    }
}
