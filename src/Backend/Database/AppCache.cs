using Backend.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Backend.Database;

public class AppCache(IDistributedCache cache, ILogger<AppCache> logger)
{
    #region Teacher Cache

    private const string TeacherCacheKeyPrefix = "teacher-";

    public async Task<Teacher?> GetTeacherAsync(Guid teacherId)
    {
        var key = Util.GetCacheKey(TeacherCacheKeyPrefix, teacherId);
        var cached = await cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cached)) return null;
        try
        {
            var teacher = JsonConvert.DeserializeObject<Teacher>(cached);
            if (teacher != null)
            {
                logger.LogDebug("Cache hit for teacher {TeacherId}", teacherId);
                return teacher;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize teacher from cache. Removing corrupted entry {TeacherId}",
                teacherId);
            await cache.RemoveAsync(key);
        }

        return null;
    }

    public async Task SetTeacherAsync(Teacher teacher)
    {
        var serialized = JsonConvert.SerializeObject(teacher);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
        var key = Util.GetCacheKey(TeacherCacheKeyPrefix, teacher.Id);
        await cache.SetStringAsync(key, serialized, options);
        logger.LogDebug("Set teacher {TeacherId} in cache", teacher.Id);
    }

    public async Task EvictTeacherAsync(Guid teacherId)
    {
        var key = Util.GetCacheKey(TeacherCacheKeyPrefix, teacherId);
        await cache.RemoveAsync(key);
        logger.LogDebug("Removed teacher {TeacherId} from cache", teacherId);
    }

    #endregion

    #region Admin Cache

    private const string AdminCacheKeyPrefix = "admin-";

    public async Task EvictAdminAsync(Guid adminId)
    {
        var key = Util.GetCacheKey(AdminCacheKeyPrefix, adminId);
        await cache.RemoveAsync(key);
        logger.LogDebug("Removed admin {AdminId} from cache", adminId);
    }

    public async Task<Admin?> GetAdminAsync(Guid adminId)
    {
        var key = Util.GetCacheKey(AdminCacheKeyPrefix, adminId);
        var cached = await cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cached)) return null;
        try
        {
            var admin = JsonConvert.DeserializeObject<Admin>(cached);
            if (admin != null)
            {
                logger.LogDebug("Cache hit for admin {AdminId}", adminId);
                return admin;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize admin from cache. Removing corrupted entry {AdminId}", adminId);
            await cache.RemoveAsync(key);
        }

        return null;
    }

    public async Task SetAdminAsync(Admin admin)
    {
        var serialized = JsonConvert.SerializeObject(admin);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
        var key = Util.GetCacheKey(AdminCacheKeyPrefix, admin.Id);
        await cache.SetStringAsync(key, serialized, options);
        logger.LogDebug("Set admin {AdminId} in cache", admin.Id);
    }

    #endregion
}