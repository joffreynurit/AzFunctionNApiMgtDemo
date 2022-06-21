using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml.XPath;
using AngleSharp.Html.Dom;
using AngleSharp;
using System.Threading;
using AngleSharp.Io;
using System.Net.Http.Headers;
using System.Linq;

namespace AzFuncNApiMgtDemo
{
    public static class GetMetaDescOfHttpPage
    {
        [FunctionName("GetMetaDescOfHttpPage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try 
            { 
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string url = data?.URL;
                string metaDesc = string.Empty;

                using (var httpClient = new HttpClient())
                {
                    using(var result = await httpClient.GetAsync(url))
                    {
                        if (result.IsSuccessStatusCode)
                        {
                            var xml = await GetDocumentAsync(result);
                            var metas = xml.QuerySelectorAll("meta");
                            var balise = metas.Where(el => el.GetAttribute("property") == "og:description" || el.GetAttribute("name") == "description").FirstOrDefault();

                            metaDesc = balise.GetAttribute("content");
                        }
                    }
                }

                return new OkObjectResult(metaDesc);
            }
            catch (Exception ex) { }

            return new BadRequestObjectResult(string.Empty);
        }

        public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var config = new Configuration().WithDefaultLoader();
            var content = await response.Content.ReadAsStringAsync();
            var document = await BrowsingContext.New(config)
                .OpenAsync(ResponseFactory, CancellationToken.None);
            return (IHtmlDocument)document;

            void ResponseFactory(VirtualResponse htmlResponse)
            {
                htmlResponse
                    .Address(response.RequestMessage.RequestUri)
                    .Status(response.StatusCode);

                MapHeaders(response.Headers);
                MapHeaders(response.Content.Headers);

                htmlResponse.Content(content);

                void MapHeaders(HttpHeaders headers)
                {
                    foreach (var header in headers)
                    {
                        foreach (var value in header.Value)
                        {
                            htmlResponse.Header(header.Key, value);
                        }
                    }
                }
            }
        }
    }
}
