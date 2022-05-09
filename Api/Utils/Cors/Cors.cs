namespace Api.Utils.Cors
{
    public static class Cors
    {
        public static IApplicationBuilder SetupCors(this IApplicationBuilder app, WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
                return app;
            }

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
                                     .Get<string[]>()
                                 ?? Array.Empty<string>();

            if (!allowedOrigins.Any()) Console.WriteLine("Warning: No CORS configured!");
            app.UseCors(builder => builder
                .WithOrigins(allowedOrigins)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod());
            return app;
        }
    }
}
