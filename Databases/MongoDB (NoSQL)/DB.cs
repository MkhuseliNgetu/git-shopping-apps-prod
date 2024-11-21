using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using MongoDB.Driver.Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;


namespace SyanStudios.gitshoppingappsprod.MongoDB
{

    public static class git_shopping_apps_prod_main_MongoDB
    {

        public static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        public static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLCollection;

        [FunctionName("git_shopping_apps_prod_ConnectionStatus")]
        public static async Task<IActionResult> RunConnectionStatus([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (AtlasDBConnectionString == null)
            {
                ResponseMessage = "You must set your 'MONGODB_URI' environment variable." +
                                         "To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/quick-start/#set-your-connection-string";
                Environment.Exit(0);
                return new BadRequestObjectResult(ResponseMessage);
            }
            else
            {
                ResponseMessage = "ATLAS DB Connection is Live!";
            }

            MongoDBClient = new MongoClient(AtlasDBConnectionString);
            NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
            NCLCollection = NightCityLab.GetCollection<BsonDocument>("Clients");

            var CurrentUsers = await NCLCollection.EstimatedDocumentCountAsync();

            if (CurrentUsers != 0)
            {
                ResponseMessage = "DB is ok :)";
            }
            else
            {
                ResponseMessage = "DB has been changed :|";
            }

            return new OkObjectResult(ResponseMessage);

        }

    }

}