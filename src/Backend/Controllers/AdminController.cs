using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController(IDbContextFactory<DataContext> ctxFactory) : ControllerBase
{
    [HttpGet("teachers")]
    [ProducesResponseType(typeof(List<Teacher>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllTeachers()
    {
        await using var ctx = await ctxFactory.CreateDbContextAsync();
        var teachers = await ctx.Teachers.ToListAsync();
        return Ok(teachers);
    }
}