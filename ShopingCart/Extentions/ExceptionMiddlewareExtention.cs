using ShopingCart.Handlers;

namespace ShopingCart.Extentions
{
    public static class ExceptionMiddlewareExtention
    {
        public static void UseExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
