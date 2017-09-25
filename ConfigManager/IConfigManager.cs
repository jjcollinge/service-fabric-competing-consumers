using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace ConfigManager
{
    public interface IConfigManager : IService
    {
        Task<Boolean> AddAsync(String key, String value);
        Task UpdateAsync(String key, String value);
        Task<Boolean> RemoveAsync(String key);
        Task<String> GetAsync(String key);
    }
}
