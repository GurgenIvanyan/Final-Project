using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace Playlist.Api.Infrastructure.Caching
{
    public class RedisCacheService
    {
        private readonly IDistributedCache _cache;

       
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,   
            WriteIndented = false
        };

        public RedisCacheService(IDistributedCache cache) => _cache = cache;

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            try
            {
                var json = await _cache.GetStringAsync(key, ct);
                return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch
            {
            
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _opts);
                await _cache.SetStringAsync(
                    key,
                    json,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                    ct
                );
            }
            catch
            {
                
            }
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            try { return _cache.RemoveAsync(key, ct); }
            catch { return Task.CompletedTask; } 
        }
    }
}
