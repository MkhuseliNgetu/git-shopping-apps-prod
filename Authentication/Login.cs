using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using BCr = BCrypt.Net;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using git_shopping_apps_prod.AdditionalServices;

namespace SyanStudios.gitshoppingappsprod.login
{

    public static class git_shopping_apps_prod_main_login
    {
        public static IDictionary<string, byte> UA = new Dictionary<string, byte>();
        private static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        private static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLCollection;


        [FunctionName("git_shopping_apps_prod_login")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            
            log.LogInformation("Login HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //Error Handling, should the user want to send data via query parameters
            IDictionary<string, string> UserRegistrationAttempts = req.GetQueryParameterDictionary();

            //HTTP Request Bodies
            string UserName = FormData["Username"];
            string PassCode = FormData["Passcode"];

            //Default Credential Sizes
            int MinUsernameLength = 5;
            int MinPasscodeLength = 8;
            
            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (UserName.Length >= MinUsernameLength && PassCode.Length >= MinPasscodeLength)
            {

                //Password Security
                var SCREPassCode = BCr.BCrypt.HashPassword(PassCode);

                //Token Assignment
                AuthTokens JWTAssignment = new AuthTokens();
                //var AuthenticatedUserJWT = JWTAssignment.AssignAuthTokens(UserName);

                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Clients");

                BsonDocument ExistingClient = new BsonDocument { { "ClientUsername", UserName }, { "ClientPasscode", PassCode } };

                var ExistingUserFilter = Builders<BsonDocument>.Filter;
                var ExistingUserQuery = ExistingUserFilter.Eq("ClientUsername", UserName) & ExistingUserFilter.Eq("ClientPasscode", PassCode);

                var SearchOutcome = NCLCollection.Find(ExistingUserQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    
                    ResponseMessage = "Login Successful!";
                    return new OkObjectResult(ResponseMessage);

                }
                else
                {
                    ResponseMessage = "Error: Incorrect Username/Password. Please re-enter a valid username/password";
                    UserRegistrationAttempts.Add(UserName, PassCode);

                    if (UserRegistrationAttempts.Count == 3)
                    {
                        ResponseMessage = "Error: Too Many Login Attempts!. Shutting Down";
                        await Task.Delay(5000);
                        return new BadRequestObjectResult(ResponseMessage);

                    }

                }

            }
            else if ((UserName.Length < MinUsernameLength && string.IsNullOrWhiteSpace(UserName)) || UserName.Length < MinUsernameLength)
            {
                ResponseMessage = "Username you entered is too short and/or contains whitespaces." + "\n" +
                                   "Please re-enter a username in accordance to the guidelines.";

                return new BadRequestObjectResult(ResponseMessage);

            }
            else if ((PassCode.Length < MinPasscodeLength && string.IsNullOrWhiteSpace(PassCode)) || PassCode.Length < MinPasscodeLength)
            {
                ResponseMessage = "Password you entered is too short and/or contains whitespaces" + "\n" +
                                   "Please re-enter a password in accordance to the guidelines.";

                return new BadRequestObjectResult(ResponseMessage);
            }

            return new OkObjectResult(ResponseMessage);
        }
    }
}