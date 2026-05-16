using Brimborium.Worker.Web;

namespace Brimborium.Worker.WebApp;

public class Program : BWProgram {

    public static async Task Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        AppConfiguration appConfiguration = new();
        appConfiguration.Bind(builder.Configuration);
        Program program = new Program(builder, appConfiguration);
        await program.MainAsync();
    }

    public Program(WebApplicationBuilder builder, AppConfiguration appConfiguration) : base(builder, appConfiguration) { }
}

public sealed class AppConfiguration : BWAppConfiguration { }