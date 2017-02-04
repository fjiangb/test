
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
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json.Linq;

namespace Stateful1.Controllers
{
    public class ProductController : ApiController
    {
        private static readonly Uri alphabetServiceUri = new Uri(@"fabric:/partOwin/Stateful1");
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();
        private readonly HttpClient httpClient = new HttpClient();
        private static string Stub = @"{ AverageRating = 2.5f, ProductId = ""9NBLGGH0TDN2"", TotalRatingsCount = 500 };";


        // GET api/values 
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values 
        public async Task<string> Get(string id)
        {
            string output = id + "-returned";
            var myDictionary = await CustomServiceContext.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("cacheDictionary");

            using (var tx = CustomServiceContext.StateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, id);

                if (!result.HasValue)
                {
                    await myDictionary.AddOrUpdateAsync(tx, id, ProductController.Stub, (key, value) => value = ProductController.Stub);
                }
                else
                {
                    output = result.Value.ToString();
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }

            return output;
        }
    }
}
