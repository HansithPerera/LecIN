using System.Security.Claims;
using Backend.Database;
using Backend.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Auth;

namespace Backend.Controllers;

[ApiController]
//[Route("api/attendance")]   //old to avoid clash
[Route("api/student")] // new
[AllowAnonymous]
public class StudentCheckInController(Repository repo) : ControllerBase
{

    [HttpGet("ping")]
    public IActionResult Ping() => Ok("StudentCheckInController OK");

    //[Authorize]
    [HttpPost("check-in")]
    public async Task<ActionResult<CheckInResponse>> CheckIn([FromBody] CheckInRequest req)
    {


        
        var sub = User.FindFirstValue("sub");
        //if (!Guid.TryParse(sub, out var studentId))
        //    return Forbid();
        Guid studentId = Guid.TryParse(sub, out var sid)
            ? sid
            : Guid.Parse("49bf8c3d-f7c9-4b2c-862a-9477ddcff815");

        var cls = await repo.GetClassByIdAsync(req.ClassId);
        if (cls is null) return NotFound("Class not found.");

        // comment out already checked in causing issue
        //var existing = await repo.GetAttendanceAsync(studentId, req.ClassId);
        //if (existing is not null)
        //{
        //    var statusExisting = existing.Timestamp <= cls.StartTime.AddMinutes(5) ? "Present" : "Late";
        //    return Ok(new CheckInResponse("Already checked in.",cls.CourseCode,existing.Timestamp,statusExisting));
        //}

        var att = await repo.CreateAttendanceAsync(studentId, req.ClassId);
        var status = att.Timestamp <= cls.StartTime.AddMinutes(5) ? "Present" : "Late";


        return Created(
            uri: string.Empty,value: new CheckInResponse("Attendance recorded.",cls.CourseCode,att.Timestamp,status));
    }
}