using DataSource;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ProducerManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ManagementAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class ProducerController : ApiController
    {
        private IProducerManager _producerManager;
        public IProducerManager ProducerManager
        {
            get
            {
                if(_producerManager == null)
                {
                    _producerManager = ServiceProxy.Create<IProducerManager>(new Uri("fabric:/ElasticScaleDemo/ProducerManager"), new ServicePartitionKey(0));
                }
                return _producerManager;
            }
        }

        // GET api/producer
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task Post()
        {
            await ProducerManager.AddProducerAsync();
        }

        // DELETE api/producer
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task Delete()
        {
            await ProducerManager.RemoveProducerAsync();
        }

        // GET api/producer
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<Int32> Get()
        {
            return await ProducerManager.GetProducerCountAsync();
        }
    }
}
