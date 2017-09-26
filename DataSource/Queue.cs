using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace DataSource
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Queue : StatefulService, IQueue
    {
        private Task<IReliableConcurrentQueue<String>> _queue => this.StateManager.GetOrAddAsync<IReliableConcurrentQueue<String>>("Queue");

        public Queue(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<ConditionalValue<String>> TryDequeueAsync()
        {
            ConditionalValue<string> next = new ConditionalValue<string>(false, string.Empty);
            var queue = await _queue;
            using (var tx = StateManager.CreateTransaction())
            {
                next = await queue.TryDequeueAsync(tx);
                await tx.CommitAsync();
            }

            return next;
        }

        public async Task EnqueueAsync(String item)
        {
            var queue = await _queue;
            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, item);
                await tx.CommitAsync();
            }
        }

        public async Task<Int32> GetCountAsync()
        {
            var queue = await _queue;
            return Convert.ToInt32(queue.Count);
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
