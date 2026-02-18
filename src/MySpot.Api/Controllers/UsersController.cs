using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySpot.Application.Abstractions;
using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Application.Queries;
using MySpot.Application.Security;
using MySpot.Core.ValueObjects;


namespace MySpot.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IQueryHandler<GetUsers, IEnumerable<UserDto>> _getUsersHandler;
    private readonly IQueryHandler<GetUser, UserDto> _getUserHandler;
    private readonly ITokenStorage _tokenStorage;
    private readonly ICommandHandler<SignUp> _signUpHandler;
    private readonly ICommandHandler<SignIn> _signInHandler;


    public UsersController(ICommandHandler<SignUp> signUpHandler, 
        ICommandHandler<SignIn> signInHandler,
        IQueryHandler<GetUsers, IEnumerable<UserDto>> getUsersHandler,
        IQueryHandler<GetUser, UserDto> getUserHandler,
        ITokenStorage tokenStorage)
    {
        _signUpHandler = signUpHandler;
        _signInHandler = signInHandler;
        _getUsersHandler = getUsersHandler;
        _getUserHandler = getUserHandler;
        _tokenStorage = tokenStorage;
    }
    
    [HttpGet("{UserId:guid}")]
    public async Task<ActionResult<UserDto>> Get(Guid userId)
    {
        var user = await _getUserHandler.HandleAsync(new GetUser{ UserId = userId });
        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Get()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.User.Identity?.Name))
        {
            return NotFound();
        }
        
        var userId = Guid.Parse(HttpContext.User.Identity.Name);
        var user = await _getUserHandler.HandleAsync(new GetUser{ UserId = userId });
        if (user is null)
        {
            return NotFound();
        }
        
        return user;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> Get([FromQuery] GetUsers query)
        => Ok(await _getUsersHandler.HandleAsync(query));
    
    [HttpPost]
    public async Task<ActionResult> Post(SignUp command)
    {
        command = command with { UserId = Guid.NewGuid() };
        await _signUpHandler.HandleAsync(command);
        
        return NoContent();
    }
    
    [HttpPost("sign-in")]
    public async Task<ActionResult> Post(SignIn command)
    {
        await _signInHandler.HandleAsync(command);
        var jwt = _tokenStorage.Get();
        
        return Ok(jwt);
    }
}