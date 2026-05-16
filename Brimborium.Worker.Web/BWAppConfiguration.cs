namespace Brimborium.Worker.Web;

public class BWAppConfiguration {
    /// <summary>
    /// Negotiate | OpenIdConnect | Bearer
    /// </summary>
    public string Authentication { get; set; } = Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme;
}

public static class BWAppConfigurationExtension {
    extension(BWAppConfiguration appConfiguration) {
        public void Bind(IConfiguration configuration) {
            configuration.Bind(appConfiguration);
        }

        public bool IsAuthenticationNegotiate => string.Equals(
            Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme,
            appConfiguration.Authentication,
            StringComparison.OrdinalIgnoreCase);


        public bool IsAuthenticationOpenIdConnect => string.Equals(
            Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
            appConfiguration.Authentication,
            StringComparison.OrdinalIgnoreCase);

        public bool IsAuthenticationJwtBearer => string.Equals(
            Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            appConfiguration.Authentication,
            StringComparison.OrdinalIgnoreCase);
    }
}
