using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.ClaimExtensions;
using Services.Core;

namespace NotificationCenter;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var rs = await _userService.Get();
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpGet("Profile")]
    public async Task<ActionResult> GetProfile()
    {
        var rs = await _userService.Profile(Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("FcmToken")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> BindFcmToken([FromBody] BindFcmtokenModel model)
    {
        var rs = await _userService.BindFcmtoken(model, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpDelete("FcmToken")]
    public async Task<IActionResult> DeleteFcmToken([FromBody] DeleteFcmtokenModel model)
    {
        var rs = await _userService.DeleteFcmToken(model.FcmToken, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }
}

