using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Fabric.Description;

namespace WorkerManager
{
    struct ServiceInfo
    {
        public String ServiceAddress;
        public String ServiceName;
    }

    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class WorkerManager : StatefulService, IWorkerManager
    {
        private Task<IReliableDictionary<String, String>> _workers
            => StateManager.GetOrAddAsync<IReliableDictionary<String, String>>("Workers");

        public WorkerManager(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddWorkerAsync()
        {
            // Create new Worker service
            var service = await CreateNewWorkerServiceAsync();
            // Store address to worker
            var workers = await _workers;
            using (var tx = StateManager.CreateTransaction())
            {
                await workers.AddAsync(tx, service.ServiceName, service.ServiceAddress);
                await tx.CommitAsync();
            }
        }

        private async Task<ServiceInfo> CreateNewWorkerServiceAsync()
        {
            var count = await this.GetWorkerCountAsync() + 1;
            var appName = this.Context.CodePackageActivationContext.ApplicationName;
            var serviceBaseName = "Worker";
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

        public async Task RemoveWorkerAsync()
        {
            // Remove worker service
            var service = await DeleteWorkerService();

            // Remove address to worker
            var workers = await _workers;
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await workers.TryRemoveAsync(tx, service.ServiceName);

                if (result.HasValue)
                {
                    await tx.CommitAsync();
                }
            }
        }

        private async Task<ServiceInfo> DeleteWorkerService()
        {
            var count = await this.GetWorkerCountAsync();
            var appName = this.Context.CodePackageActivationContext.ApplicationName;
            var serviceBaseName = "Worker";
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

        public async Task<int> GetWorkerCountAsync()
        {
            var count = 0;
            var workers = await _workers;
            using (var tx = StateManager.CreateTransaction())
            {
                count = Convert.ToInt32(await workers.GetCountAsync(tx));
            }
            return count;
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
