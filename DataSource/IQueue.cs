namespace DataSource
{
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Remoting;
    using System;
    using System.Threading.Tasks;

    public interface IQueue : IService
    {
        Task EnqueueAsync(String item);
        Task<ConditionalValue<String>> TryDequeueAsync();

        Task<Int32> GetCountAsync();
    }
}
