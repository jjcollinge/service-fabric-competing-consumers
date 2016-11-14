using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer
{
    public interface IProducer : IService
    {
        Task SetProductionRateAsync(Int32 productionRateInSeconds);
    }
}
