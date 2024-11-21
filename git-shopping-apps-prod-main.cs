using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SyanStudios.gitshoppingappsprod
{
    public static class git_shopping_apps_prod_main
    {
        [FunctionName("git_shopping_apps_prod_main")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["LiveStatus"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? $"The App Function for GitHub E-Commerce Projects is Live!. Current Status: {name}"
                : $"The App Function for GitHub E-Commerce Projects is Live!. Current Status: {name}";

            return new OkObjectResult(responseMessage);
        }
    }
}
