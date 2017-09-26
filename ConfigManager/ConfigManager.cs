using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace ConfigManager
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ConfigManager : StatefulService, IConfigManager
    {
        private Task<IReliableDictionary<String, String>> _config
            => this.StateManager.GetOrAddAsync<IReliableDictionary<String, String>>("Config");

        public ConfigManager(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<bool> AddAsync(string key, string value)
        {
            var success = false;
            var config = await _config;
            using (var tx = StateManager.CreateTransaction())
            {
                success = await config.TryAddAsync(tx, key, value);
                await tx.CommitAsync();
            }
            return success;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            var success = false;
            var config = await _config;
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await config.TryRemoveAsync(tx, key);

                if(result.HasValue)
                {
                    success = true;
                    await tx.CommitAsync();
                }
            }
            return success;
        }

        public async Task<string> GetAsync(string key)
        {
            var value = String.Empty;
            var config = await _config;
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await config.TryGetValueAsync(tx, key);
                value = result.Value;
            }
            return value;
        }

        public async Task UpdateAsync(string key, string value)
        {
            var config = await _config;
            using (var tx = StateManager.CreateTransaction()) 
            {
                var currentValue = await config.TryGetValueAsync(tx, key);

                if(currentValue.HasValue)
                {
                    await config.SetAsync(tx, key, value);
                    await tx.CommitAsync();
                }
            }
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
