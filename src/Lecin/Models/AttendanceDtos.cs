using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecin.Models;

public record CourseAttendanceDto(string CourseCode, int TotalClasses, int Attended, double Percentage);

public record AttendancePercentageDto(
    Guid StudentId, int TotalClasses, int Attended, double Percentage, List<CourseAttendanceDto> ByCourse);
