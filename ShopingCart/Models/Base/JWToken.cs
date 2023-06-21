
namespace ShopingCart.Models.Base
{
    public partial class JWToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public partial class JWToken
    {
        public string Scope { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}
