using System.Threading.Tasks;
using Xunit;

public class AttendanceTests
{
    [Fact]
    public async Task GetAttendanceHistory_ReturnsAttendanceList()
    {
        // Arrange
        var service = new AppService(_mockDbContext.Object); // You must mock this properly
        var studentId = "123";

        // Act
        var result = await service.GetAttendanceHistoryForStudentAsync(studentId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item =>
        {
            Assert.False(string.IsNullOrEmpty(item.ClassName));
            Assert.NotEqual(DateTime.MinValue, item.ClassStartTime);
        });
    }
}
    