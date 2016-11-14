using DataSource;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ManagementAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class QueueController : ApiController
    {
        // GET api/values 
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<Int32> Get()
        {
            var dataSource = ServiceProxy.Create<IQueue>(new Uri("fabric:/ElasticScaleDemo/DataSource"), new ServicePartitionKey(0));
            var count = await dataSource.GetCountAsync();
            return count;
        }
    }
}
