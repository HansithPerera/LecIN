using AttendanceCheckInBackend.Models;

namespace AttendanceCheckInBackend.Services
{
    public class FacialRecognitionService
    {
        public Student RecognizeFace(string inputEncoding, List<Student> students)
        {
            return students.FirstOrDefault(s => s.FaceEncoding == inputEncoding);
        }
    }
}