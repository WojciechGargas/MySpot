using MySpot.Application;
using MySpot.Application.Services;
using MySpot.Core;
using MySpot.Core.Repositories;
using MySpot.Infrastructure;
using MySpot.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure()
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


