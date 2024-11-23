using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using git_shopping_apps_prod.Databases.Models;

using BCr = BCrypt.Net;

namespace SyanStudios.gitshoppingappsprod.cardauth
{

    public static class git_shopping_apps_prod_main_cardauth
    {

        private static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        private static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLCollection;


        [FunctionName("git_shopping_apps_prod_card_auth")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Card Authentication HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Cards NewCardDetails = new Cards();
            NewCardDetails.CardHolderFullName = FormData["CardHolderFullName"];
            NewCardDetails.CardNo = (int)Convert.ToInt64(FormData["CardNumber"]);
            NewCardDetails.CardExpMonth = (int)Convert.ToInt64(FormData["CardExpiryMonth"]);
            NewCardDetails.CardExpYear = (int)Convert.ToInt64(FormData["CardExpiryYear"]);
            NewCardDetails.CardCVV = (int)Convert.ToInt64(FormData["CardCVV"]);

            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (NewCardDetails.CardNo != null && NewCardDetails.CardExpMonth != null && NewCardDetails.CardExpYear != null && NewCardDetails.CardCVV != null)
            {

                //Card Config
                var EncryptionType = BCr.HashType.SHA256;
                int SaltRounds = 10;
                var DefaultSalt = BCr.BCrypt.GenerateSalt(15);
                var ClientCardUUID = BCr.BCrypt.HashPassword(NewCardDetails.CardHolderFullName, DefaultSalt);


                //Dummy Balances
                Random GetACardBalance = new Random();
                int DummyBalance = GetACardBalance.Next(5000, 10000);

                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Payments");

                BsonDocument NewPaymentDetails = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, 
                    { "C_CardHolderFullName", NewCardDetails.CardHolderFullName }, { "C_CardNo", NewCardDetails.CardNo }, 
                    { "C_CardExpMonth", NewCardDetails.CardExpMonth },{ "C_CardExpYear", NewCardDetails.CardExpYear },
                    { "C_CardCVV", NewCardDetails.CardExpYear }, {"C_ClientUUID", ClientCardUUID }, {"C_Balance", DummyBalance} };

                var ExistingCardFilter = Builders<BsonDocument>.Filter;
                var ExistingCardQuery = ExistingCardFilter.Eq("C_CardNo", NewCardDetails.CardNo);

                var SearchOutcome = NCLCollection.Find(ExistingCardQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    ResponseMessage = "Payment Details Captured Successfully. Now processing..";
                    return new OkObjectResult(ResponseMessage);
                }
                else 
                {
                    var SuccessPayment = NCLCollection.InsertOneAsync(NewPaymentDetails);
                    ResponseMessage = "New Payment Details Captured Successfully. Now processing..";
                    return new OkObjectResult(ResponseMessage);
                }

            }
            else if (NewCardDetails.CardNo < 16 || NewCardDetails.CardNo > 16)
            {
                ResponseMessage = "The card number you entered is too short or long" + "\n" +
                                   "Please re-enter a 16 digit card number.";

                return new BadRequestObjectResult(ResponseMessage);

            }
            else if (NewCardDetails.CardExpMonth >3 || NewCardDetails.CardExpMonth < 2)
            {
                ResponseMessage = "The expiry month you entered is too short or long" + "\n" +
                                   "Please re-enter a 2 digit expiry month for the card number.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (NewCardDetails.CardExpYear > 3 || NewCardDetails.CardExpYear < 2)
            {
                ResponseMessage = "The expiry year you entered is too short or long" + "\n" +
                                   "Please re-enter a 2 digit expiry year for the card number.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (NewCardDetails.CardCVV > 3 || NewCardDetails.CardCVV < 2)
            {
                ResponseMessage = "The CVV you entered is too short or long" + "\n" +
                                   "Please re-enter a 3 digit CVV for the card number.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            return new OkObjectResult(ResponseMessage);
        }
    }

}