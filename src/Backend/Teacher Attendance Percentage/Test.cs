using System.Threading.Tasks;

[Fact]
public async Task GetStudentAttendancePercentage_ReturnsCorrectPercentage()
{
    // Arrange
    var studentId = "student-123";
    var mockContext = new Mock<AppDbContext>();
    var service = new AppService(mockContext.Object);

    // Assume 10 attendance records, 8 are Present or Late
    mockContext.Setup(...); // Setup your test db or use in-memory context

    // Act
    var percentage = await service.GetStudentAttendancePercentageAsync(studentId);

    // Assert
    Assert.Equal(80.0, percentage);
}
