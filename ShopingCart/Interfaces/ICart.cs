using ShopingCart.Models.Base;
using ShopingCart.Models.Resquest;

namespace ShopingCart.Interfaces
{
    public interface ICart
    {
        public Task<(bool, dynamic)> AccessToken(string username, string password);
        public Task<(bool, dynamic)> RefreshToken(RefreshTokenRequest context);
        public Task<(bool, dynamic)> RevocateToken(JWToken context);
    }
}
