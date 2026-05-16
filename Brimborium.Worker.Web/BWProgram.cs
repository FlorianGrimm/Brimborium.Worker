using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace Brimborium.Worker.Web;
// https://github.com/nenoNaninu/TypedSignalR.Client

public class BWProgram {
    public readonly WebApplicationBuilder Builder;
    public readonly BWAppConfiguration AppConfiguration;

    private WebApplication? _WebApplication;
    public WebApplication WebApplication => this._WebApplication ?? throw new Exception("WebApplication is null");

    public BWProgram(
            WebApplicationBuilder builder,
            BWAppConfiguration appConfiguration
    ) {
        this.Builder = builder;
        this.AppConfiguration = appConfiguration;
    }

    public async Task MainAsync() {
        try {
            await this.RunAsync();
        } catch (AggregateException error) {
            error.Handle((error) => {
                System.Console.Error.WriteLine(error.ToString());
                return true;
            });
        } catch (Exception error) {
            System.Console.Error.WriteLine(error.ToString());
        }
    }

    public virtual async Task RunAsync() {
        this.ConfigureBuilder();
        var app = this.Builder.Build();
        this._WebApplication = app;
        this.ConfigureApplication();
        await app.RunAsync();
    }

    public virtual void ConfigureBuilder() {
        this.ConfigureAuthentication();
        this.ConfigureWebServices();
    }

    public virtual AuthenticationBuilder ConfigureAuthentication() {
        // Add services to the container.
        if (this.AppConfiguration.IsAuthenticationNegotiate) {
            AuthenticationBuilder authenticationBuilder = this.Builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme);
            authenticationBuilder.AddNegotiate();
            return authenticationBuilder;
        } else if (this.AppConfiguration.IsAuthenticationOpenIdConnect) {
            AuthenticationBuilder authenticationBuilder = this.Builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme);
            authenticationBuilder.AddMicrosoftIdentityWebApp(this.Builder.Configuration.GetSection("AzureAd"));
            return authenticationBuilder;
        } else if (this.AppConfiguration.IsAuthenticationJwtBearer) {
            AuthenticationBuilder authenticationBuilder = this.Builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            authenticationBuilder.AddMicrosoftIdentityWebApi(this.Builder.Configuration.GetSection("AzureAd"));
            return authenticationBuilder;
        } else {
            throw new Exception("Authentication must be Negotiate|OpenIdConnect|Bearer");
        }
    }

    public virtual void ConfigureWebServices() {
        this.Builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //this.Builder.Services.AddOpenApi();
        this.Builder.Services.AddRazorPages(
            (razorPagesOptions) => {
            });
    }

    public virtual void ConfigureApplication() {

        this.ConfigureWWWRoot();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment()) {
        //    app.MapOpenApi();
        //}
        /*
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
        */


        //app.UseHttpsRedirection();

        var app = this.WebApplication;
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();
        //app.MapFallback

    }

    public virtual void ConfigureWWWRoot() {
        if (global::Brimborium.Worker.Frontend.FrontendLocation.GetWWWRoot() is { } wwwRoot) {
            _ = this.WebApplication.UseStaticFiles()
                .UseStaticFiles(new StaticFileOptions() {
                    FileProvider = new PhysicalFileProvider(
                            System.IO.Path.GetFullPath(wwwRoot)),
                    RequestPath = new PathString("/ui"),
                    DefaultContentType = "text/html"
                });
        }
    }
}
