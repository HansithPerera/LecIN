using Backend.Models;
using System.Collections.Generic;

namespace Backend.Services
{
    public class ClassService
    {
        // This would typically fetch data from a database
        public TeacherProfile GetTeacherProfileByClassId(int classId)
        {
            return new TeacherProfile
            {
                Id = 1,
                FullName = "Dr. Jane Doe",
                Email = "jane.doe@university.edu",
                Phone = "123-456-7890",
                OfficeLocation = "Room 101, Science Building",
                ProfileImageUrl = "https://university.edu/images/jane-doe.jpg"
            };
        }
    }
}
