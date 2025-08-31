using Backend.Database;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class CacheTests: IClassFixture<MockAppBuilder>
{
    private readonly MockAppBuilder _builder;
    public CacheTests(MockAppBuilder mockAppBuilder)
    {
        _builder = mockAppBuilder;
    }
    
    private const string TeacherId = "teacher-2";
    
    [Fact]
    public async Task TestTeacherCacheEvictionOnDelete()
    {
        var service = _builder.Services.GetRequiredService<AppService>();
        var teacher = new Teacher
        {
            Id = TeacherId,
            FirstName = "Bob",
            LastName = "Johnson",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await service.AddTeacherAsync(teacher);
        
        var fetched = await service.GetTeacherByIdAsync(TeacherId);
        Assert.NotNull(fetched);
        
        await service.DeleteTeacherAsync(TeacherId);

        var fetchedAfterDelete = await service.GetTeacherByIdAsync(TeacherId);
        Assert.Null(fetchedAfterDelete);
    }
    
    [Fact]
    public async Task TestTeacherAddedToCacheOnFetch()
    {
        var service = _builder.Services.GetRequiredService<AppService>();
        var appCache = _builder.Services.GetRequiredService<AppCache>();
        
        var teacher = new Teacher
        {
            Id = TeacherId,
            FirstName = "Bob",
            LastName = "Johnson",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await service.AddTeacherAsync(teacher);
        
        var fetched1 = await service.GetTeacherByIdAsync(TeacherId);
        Assert.NotNull(fetched1);
        
        var cached = await appCache.GetTeacherAsync(TeacherId);
        Assert.NotNull(cached);
        
        Assert.Equal(fetched1.Id, cached!.Id);
        Assert.Equal(fetched1.FirstName, cached.FirstName);
        Assert.Equal(fetched1.LastName, cached.LastName);
    }
}