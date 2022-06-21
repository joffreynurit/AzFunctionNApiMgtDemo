using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzFuncNApiMgtDemo
{
    public static class RngGenerator
    {
        [FunctionName("RngGenerator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                Random random = new Random();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                int maxValue = data.MaxValue;

                return new OkObjectResult(random.Next(maxValue));
            }
            catch (Exception ex) { }

            return new BadRequestObjectResult(string.Empty);
        }
    }
}
