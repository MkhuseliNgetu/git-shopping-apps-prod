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
using git_shopping_apps_prod.Databases.Models;

namespace SyanStudios.gitshoppingappsprod.login
{

    public static class git_shopping_apps_prod_main_login
    {
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
            IDictionary<string, string> UserLoginAttempts = req.GetQueryParameterDictionary();

            //HTTP Request Bodies
            Clients ExistingClient = new Clients();
            ExistingClient.UserName = FormData["Username"];
            ExistingClient.PassCode = FormData["Passcode"];

            //Default Credential Sizes
            int MinUsernameLength = 5;
            int MinPasscodeLength = 8;
            
            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (ExistingClient.UserName != null && ExistingClient.PassCode != null)
            {

                //Password Security
                var EncryptionType = BCr.HashType.SHA256;
                int SaltRounds = 10;
                var DefaultSalt = BCr.BCrypt.GenerateSalt(15);
                var SCREPassCode = BCr.BCrypt.HashPassword(ExistingClient.PassCode, DefaultSalt);

                //Token Assignment
                AuthTokens JWTAssignment = new AuthTokens();
                //var AuthenticatedUserJWT = JWTAssignment.AssignAuthTokens(UserName);

                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Clients");

                var ExistingUserFilter = Builders<BsonDocument>.Filter;
                var ExistingUserQuery = ExistingUserFilter.Eq("ClientUsername", ExistingClient.UserName);

                BsonDocument SearchOutcome = NCLCollection.Find(ExistingUserQuery).FirstOrDefault();

                if (SearchOutcome != null && BCr.BCrypt.Verify(ExistingClient.PassCode, SearchOutcome.GetElement(2).Value.ToString()) == true)
                {
                    
                    ResponseMessage = "Login Successful!";
                    return new OkObjectResult(ResponseMessage);

                }
                else if (SearchOutcome != null && BCr.BCrypt.Verify(ExistingClient.PassCode, SearchOutcome.GetElement(2).Value.ToString()) == false)
                {
                    ResponseMessage = "Error: Incorrect Username/Password. Please re-enter a valid username/password";
                    UserLoginAttempts.Add(ExistingClient.UserName, ExistingClient.PassCode);

                    if (UserLoginAttempts.Count == 3)
                    {
                        ResponseMessage = "Error: Too Many Login Attempts!. Shutting Down";
                        await Task.Delay(5000);
                        return new BadRequestObjectResult(ResponseMessage);

                    }

                }

            }
            else if ((ExistingClient.UserName.Length < MinUsernameLength && string.IsNullOrWhiteSpace(ExistingClient.UserName)) || ExistingClient.UserName.Length < MinUsernameLength)
            {
                ResponseMessage = "Username you entered is too short and/or contains whitespaces." + "\n" +
                                   "Please re-enter a username in accordance to the guidelines.";

                return new BadRequestObjectResult(ResponseMessage);

            }
            else if ((ExistingClient.PassCode.Length < MinPasscodeLength && string.IsNullOrWhiteSpace(ExistingClient.PassCode)) || ExistingClient.PassCode.Length < MinPasscodeLength)
            {
                ResponseMessage = "Password you entered is too short and/or contains whitespaces" + "\n" +
                                   "Please re-enter a password in accordance to the guidelines.";

                return new BadRequestObjectResult(ResponseMessage);
            }

            return new OkObjectResult(ResponseMessage);
        }
    }
}