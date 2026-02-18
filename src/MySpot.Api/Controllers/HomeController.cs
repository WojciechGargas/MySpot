using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySpot.Infrastructure;

namespace MySpot.Api.Controllers;

[Route("")]
public class HomeController : ControllerBase
{
    private readonly string _name;

    public HomeController(IOptionsSnapshot<AppOptions> options)
    {
        _name = options.Value.Name;
    }

    [HttpGet]
    public ActionResult<string> Get() => _name;
}