namespace Producer
{
    using Microsoft.ServiceFabric.Services.Remoting;
    using System;
    using System.Threading.Tasks;

    public interface IProducer : IService
    {
        Task SetProductionRateAsync(Int32 productionRateInSeconds);
    }
}
