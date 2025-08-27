using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using GranjaTech.Infrastructure.Services.Interfaces;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    if (cachedValue is string jsonString)
                    {
                        var result = JsonSerializer.Deserialize<T>(jsonString);
                        _logger.LogDebug("Cache hit for key: {Key}", key);
                        return result;
                    }
                    
                    if (cachedValue is T directValue)
                    {
                        _logger.LogDebug("Cache hit for key: {Key}", key);
                        return directValue;
                    }
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter item do cache: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var expirationTime = expiration ?? _defaultExpiration;
                
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationTime,
                    SlidingExpiration = TimeSpan.FromMinutes(5), // Renova se acessado nos últimos 5 min
                    Priority = CacheItemPriority.Normal
                };

                // Serializar para JSON para garantir consistência
                var jsonString = JsonSerializer.Serialize(value);
                _memoryCache.Set(key, jsonString, options);
                
                _logger.LogDebug("Item cached with key: {Key}, expiration: {Expiration}", key, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir item no cache: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Cache item removed: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover item do cache: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                // IMemoryCache não suporta remoção por padrão nativamente
                // Esta implementação é limitada - para produção, considere Redis
                _logger.LogWarning("RemoveByPattern não totalmente suportado com MemoryCache. Pattern: {Pattern}", pattern);
                
                // Para implementação completa, seria necessário manter um registro das chaves
                // ou usar uma implementação de cache mais avançada como Redis
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover itens do cache por padrão: {Pattern}", pattern);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class
        {
            try
            {
                // Tentar obter do cache primeiro
                var cachedItem = await GetAsync<T>(key);
                if (cachedItem != null)
                {
                    return cachedItem;
                }

                // Se não estiver no cache, buscar o item
                _logger.LogDebug("Cache miss, fetching item for key: {Key}", key);
                var item = await getItem();
                
                if (item != null)
                {
                    // Armazenar no cache
                    await SetAsync(key, item, expiration);
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em GetOrSetAsync para key: {Key}", key);
                // Em caso de erro, tentar buscar o item diretamente
                return await getItem();
            }
        }
    }
}
