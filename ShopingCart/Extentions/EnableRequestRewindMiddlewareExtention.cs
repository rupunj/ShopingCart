namespace ShopingCart.Extentions
{
    public static class EnableRequestRewindMiddlewareExtention
    {
        public static void UseEnableRequestRewindMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<Handlers.EnableRequestRewindMiddleware>();
        }
    }
}
