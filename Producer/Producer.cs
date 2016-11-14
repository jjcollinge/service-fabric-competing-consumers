using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using DataSource;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace Producer
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Producer : StatelessService, IProducer
    {
        private Int32 _productionRateInSeconds = 1;

        public Producer(StatelessServiceContext context)
            : base(context)
        { }

        public Task SetProductionRateAsync(int productionRateInSeconds)
        {
            _productionRateInSeconds = productionRateInSeconds;
            return Task.FromResult(true);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //await Task.Delay(TimeSpan.FromSeconds(45));

            var dataSource = ServiceProxy.Create<IQueue>(new Uri("fabric:/ElasticScaleDemo/DataSource"), new ServicePartitionKey(0));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = Guid.NewGuid().ToString();

                await dataSource.EnqueueAsync(item);

                ServiceEventSource.Current.Message("Enqueued: {item}");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
