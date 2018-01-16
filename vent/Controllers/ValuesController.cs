using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using E2ETests.Helpers;

namespace vent.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {

            // curl -d '{"name": "MetricData", "time":"2017-10-27T00:01:52.9586379Z", "iKey":"f4731d25-188b-4ec1-ac44-9fcf35c05812", "data":{"baseType":"MetricData","baseData": {"metrics":[{"name":"Custom metric","value":1,"count":1}]}}}' https://localhost:5000/api/values

            var items = TelemetryItemFactory.GetTelemetryItems(value);


        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
