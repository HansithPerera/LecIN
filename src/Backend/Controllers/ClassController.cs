using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly ClassService _classService;

        public ClassController(ClassService classService)
        {
            _classService = classService;
        }

        [HttpGet("{classId}/teacher-profile")]
        public ActionResult<TeacherProfile> GetTeacherProfileByClassId(int classId)
        {
            var profile = _classService.GetTeacherProfileByClassId(classId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }
    }
}
