
using System.Collections.Generic;
using System.Web.Http;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json.Linq;

namespace Stateless1.Controllers
{
    public class ValuesController : ApiController
    {
        private static readonly Uri alphabetServiceUri = new Uri(@"fabric:/partOwin/Stateful1");
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();
        private readonly HttpClient httpClient = new HttpClient();

        // GET api/values 
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5 
        public async Task<string> Get(int id)
        {
            String output = null;

            try
            {
                //return id.ToString();
                ServicePartitionKey partitionKey = new ServicePartitionKey(id % 10);
                ResolvedServicePartition partition = await this.servicePartitionResolver.ResolveAsync(alphabetServiceUri, partitionKey, new CancellationToken());

                ResolvedServiceEndpoint ep = partition.GetEndpoint();
                JObject addresses = JObject.Parse(ep.Address);
                string primaryReplicaAddress = (string)addresses["Endpoints"].First() + @"/api/values/" + id;
                UriBuilder primaryReplicaUriBuilder = new UriBuilder(primaryReplicaAddress);
                //primaryReplicaUriBuilder.Query = "id=" + id;

                string result = await this.httpClient.GetStringAsync(primaryReplicaUriBuilder.Uri);

                output = String.Format(
                        "Result: {0}. <p>Partition key: '{1}' generated from the first letter '{2}' of input value '{3}'. <br>Processing service partition ID: {4}. <br>Processing service replica address: {5}",
                        result,
                        partitionKey,
                        id % 10,
                        id,
                        partition.Info.Id,
                        primaryReplicaAddress);
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            return output;
        }

        // POST api/values 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}