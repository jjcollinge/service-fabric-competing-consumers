using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerManager
{
    public interface IWorkerManager : IService
    {
        Task AddWorkerAsync();
        Task RemoveWorkerAsync();
        Task<Int32> GetWorkerCountAsync();
    }
}
