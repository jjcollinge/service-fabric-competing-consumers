using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace ProducerManager
{
    public interface IProducerManager : IService
    {
        Task AddProducerAsync();
        Task RemoveProducerAsync();
        Task<Int32> GetProducerCountAsync();
    }
}
