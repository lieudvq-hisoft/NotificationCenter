using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.ClaimExtensions;

namespace NotificationCenter;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var rs = await _notificationService.Get(Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var rs = await _notificationService.GetById(id);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }
}

