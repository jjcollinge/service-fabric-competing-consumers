using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSource
{
    public interface IQueue : IService
    {
        Task EnqueueAsync(String item);
        Task<String> DequeueAsync();

        Task<Int32> GetCountAsync();
    }
}
