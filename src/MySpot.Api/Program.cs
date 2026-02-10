using MySpot.Application;
using MySpot.Core;
using MySpot.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddCore()
    .AddApplication()
    .AddControllers();

var app = builder.Build();
app.MapControllers();

app.Run();

namespace MySpot.Api
{
    public partial class Program { }
}


