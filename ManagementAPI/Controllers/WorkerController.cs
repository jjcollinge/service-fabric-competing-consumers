using DataSource;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ProducerManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using WorkerManager;

namespace ManagementAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class WorkerController : ApiController
    {
        private IWorkerManager _workerManager;
        public IWorkerManager WorkerManager
        {
            get
            {
                if(_workerManager == null)
                {
                    _workerManager = ServiceProxy.Create<IWorkerManager>(new Uri("fabric:/ElasticScaleDemo/WorkerManager"), new ServicePartitionKey(0));
                }
                return _workerManager;
            }
        }

        // POST api/worker
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task Post()
        {
            await WorkerManager.AddWorkerAsync();
        }

        // DELETE api/worker
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task Delete()
        {
            await WorkerManager.RemoveWorkerAsync();
        }

        // GET api/worker
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<Int32> Get()
        {
            return await WorkerManager.GetWorkerCountAsync();
        }
    }
}
