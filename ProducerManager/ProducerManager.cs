namespace ProducerManager
{
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Threading.Tasks;

    struct ServiceInfo
    {
        public String ServiceAddress;
        public String ServiceName;
    }

    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProducerManager : StatefulService, IProducerManager
    {
        private Task<IReliableDictionary<String, String>> _producers =>
            this.StateManager.GetOrAddAsync<IReliableDictionary<String, String>>("Producers");

        public ProducerManager(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddProducerAsync()
        {
            // Create new Producer service
            var service = await CreateNewProducerServiceAsync();
            // Store address to producer
            var producers = await _producers;
            using (var tx = StateManager.CreateTransaction())
            {
                await producers.AddAsync(tx, service.ServiceName, service.ServiceAddress);
                await tx.CommitAsync();
            }
        }

        private async Task<ServiceInfo> CreateNewProducerServiceAsync()
        {
            var count = await this.GetProducerCountAsync() + 1;
            var appName = this.Context.CodePackageActivationContext.ApplicationName;
            var serviceBaseName = "Producer";
            var serviceName = $"{serviceBaseName}{count}";
            var serviceAddress = $"{appName}/{serviceName}";
            var serviceType = $"{serviceBaseName}Type";

            using (var client = new FabricClient())
            {
                var serviceDescriptor = new StatelessServiceDescription()
                {
                    ApplicationName = new Uri(appName),
                    ServiceName = new Uri(serviceAddress),
                    ServiceTypeName = serviceType,
                    PartitionSchemeDescription = new SingletonPartitionSchemeDescription()
                };
                await client.ServiceManager.CreateServiceAsync(serviceDescriptor);
            }

            return new ServiceInfo
            {
                ServiceAddress = serviceAddress,
                ServiceName = serviceName
            };
        }

        public async Task<int> GetProducerCountAsync()
        {
            var count = 0;
            var producers = await _producers;
            using (var tx = StateManager.CreateTransaction())
            {
                count = Convert.ToInt32(await producers.GetCountAsync(tx));
            }
            return count;
        }

        public async Task RemoveProducerAsync()
        {
            // Remove worker service
            var service = await DeleteProducerService();

            // Remove address to worker
            var producers = await _producers;
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await producers.TryRemoveAsync(tx, service.ServiceName);

                if(result.HasValue)
                {
                    await tx.CommitAsync();
                }
            }
        }

        private async Task<ServiceInfo> DeleteProducerService()
        {
            var count = await this.GetProducerCountAsync();
            var appName = this.Context.CodePackageActivationContext.ApplicationName;
            var serviceBaseName = "Producer";
            var serviceName = $"{serviceBaseName}{count}";
            var serviceAddress = $"{appName}/{serviceName}";
            var deleteServiceDescription = new DeleteServiceDescription(new Uri(serviceAddress));

            using (var client = new FabricClient())
            {
                await client.ServiceManager.DeleteServiceAsync(deleteServiceDescription);
            }

            return new ServiceInfo
            {
                ServiceAddress = serviceAddress,
                ServiceName = serviceName
            };
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }
    }
}
