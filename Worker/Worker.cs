using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using DataSource;
using Microsoft.ServiceFabric.Services.Client;

namespace Worker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Worker : StatelessService, IWorker
    {
        private Int32 _timeToWorkInSeconds = 1;

        public Worker(StatelessServiceContext context)
            : base(context)
        { }

        public Task SetWorkRateAsync(int timeToWorkInSeconds)
        {
            _timeToWorkInSeconds = timeToWorkInSeconds;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //await Task.Delay(TimeSpan.FromSeconds(45));

            var dataSource = ServiceProxy.Create<IQueue>(new Uri("fabric:/ElasticScaleDemo/DataSource"), new ServicePartitionKey(0));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await dataSource.DequeueAsync();

                ServiceEventSource.Current.Message("Dequeued: {item}");

                await Task.Delay(TimeSpan.FromSeconds(_timeToWorkInSeconds));
            }
        }
    }
}
