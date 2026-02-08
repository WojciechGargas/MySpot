using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySpot.Api;
using MySpot.Application.Services;

namespace MySpot.Tests.Integration.Factories;

public class MySpotApplicationFactory : WebApplicationFactory<Program>
{
    public static DateTime FixedNow => TestClock.FixedNow;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(_ => new TestClock());
        });
    }
}
