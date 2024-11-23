using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using BCrypt.Net;

using BCr = BCrypt.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

using System.Configuration;
using git_shopping_apps_prod.Databases.Models;


namespace SyanStudios.gitshoppingappsprod.registration
{

    public static class git_shopping_apps_prod_main_registration
    {
        
        private static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        private static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLCollection;

        [FunctionName("git_shopping_apps_prod_registration")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Registration HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //Error Handling, should the user want to send data via query parameters
            IDictionary<string, string> UserRegistrationAttempts = req.GetQueryParameterDictionary();

            //HTTP Request Bodies
            Clients TrialClient = new Clients();
            TrialClient.UserName = FormData["Username"];
            TrialClient.PassCode = FormData["Passcode"];

            //Default Credential Sizes
            int MinUsernameLength = 5;
            int MinPasscodeLength = 8;

            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (TrialClient.UserName != null && TrialClient.PassCode != null) 
            {

                //Password Security
                var EncryptionType = BCr.HashType.SHA256;
                int SaltRounds = 10;
                var DefaultSalt = BCr.BCrypt.GenerateSalt(15);
                var SCREPassCode = BCr.BCrypt.HashPassword(TrialClient.PassCode, DefaultSalt);

                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Clients");

                BsonDocument NewClient = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, { "ClientUsername", TrialClient.UserName }, { "ClientPasscode", SCREPassCode } };

                var ExistingUserFilter = Builders<BsonDocument>.Filter;
                var ExistingUserQuery = ExistingUserFilter.Eq("ClientUsername", TrialClient.UserName);

                var SearchOutcome = NCLCollection.Find(ExistingUserQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    ResponseMessage = "Error: Incorrect Username/Password.";
                    UserRegistrationAttempts.Add(TrialClient.UserName, TrialClient.PassCode);

                    if(UserRegistrationAttempts.Count == 3)
                    {
                        ResponseMessage = "Error: Too Many Registration Attempts!. Shutting Down";
                        await Task.Delay(5000);
                        return new BadRequestObjectResult(ResponseMessage);
                    }
                    
                }
                else
                {
                    var NewRegistration = NCLCollection.InsertOneAsync(NewClient);
                    ResponseMessage = "Registration Successful!";
                    return new OkObjectResult(ResponseMessage);
                }

         


            }
            else if((TrialClient.UserName.Length < MinUsernameLength && string.IsNullOrWhiteSpace(TrialClient.UserName)) || TrialClient.UserName.Length < MinUsernameLength){
                     ResponseMessage = "Username you entered is too short and/or contains whitespaces."+"\n"+
                                        "Please re-enter a username in accordance to the guidelines.";

                     return new BadRequestObjectResult(ResponseMessage);

            }else if((TrialClient.PassCode.Length < MinPasscodeLength && string.IsNullOrWhiteSpace(TrialClient.PassCode)) || TrialClient.PassCode.Length < MinPasscodeLength){
                     ResponseMessage = "Password you entered is too short and/or contains whitespaces"+"\n"+
                                        "Please re-enter a password in accordance to the guidelines.";

                     return new BadRequestObjectResult(ResponseMessage);
            }

            return new OkObjectResult(ResponseMessage);
        }
    }
}