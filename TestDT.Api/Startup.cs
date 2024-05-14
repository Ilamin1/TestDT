using System.Globalization;
using TestDT.Api.Extensions;

namespace TestDT.Api;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        SetupAppCulture();
        services.ServiceCollectionDi();
        services.SetupDbContext(Configuration);
    }
    
    private static void SetupAppCulture()
    {
        var culture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.SetupApplicationBuilder(env);
    }
}