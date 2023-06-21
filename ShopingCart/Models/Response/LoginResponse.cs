using ShopingCart.Enums;
using ShopingCart.Models.Base;

namespace ShopingCart.Models.Response
{
    public partial class LoginResponse
    {
        public AuthReason Reason { get; set; }
    }

    public partial class LoginResponse
    {
        public JWToken Token { get; set; }
    }
}
