using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.TrackClasses
{
    public class ServiceMethod
    {
        public Task<List<Attendance>> GetAttendanceHistoryAsync(string studentId)
        {
            // Your logic here
            return Task.FromResult(new List<Attendance>());
        }
    }
}
