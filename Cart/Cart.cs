using git_shopping_apps_prod.Databases.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace SyanStudios.gitshoppingappsprod.cart
{

    public static class git_shopping_apps_prod_main_cart
    {

        private static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        private static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLProductsCollection;
        public static IMongoCollection<BsonDocument> NCLCartCollection;
        public static IMongoCollection<BsonDocument> NCLClientsCollection;

        [FunctionName("git_shopping_apps_prod_card_add_to_cart")]
        public static async Task<IActionResult> RunAddCart([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {

            log.LogInformation("Add To Cart HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Product Item = new Product();
            Item.ProductName = FormData["ProductName"];



            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (Item.ProductName != null)
            {
                Cart Carts = new Cart();
                Carts.CartID = Guid.NewGuid().ToByteArray();

                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLProductsCollection = NightCityLab.GetCollection<BsonDocument>("Products");
                NCLCartCollection = NightCityLab.GetCollection<BsonDocument>("Carts");

                BsonDocument ExistingItem = new BsonDocument { { "P_ProductName", Item.ProductName } };

                var ExistingProductFilter = Builders<BsonDocument>.Filter;
                var ExistingProductQuery = ExistingProductFilter.Eq("P_ProductName", Item.ProductName) ;

                var ProductSearchOutcome = NCLProductsCollection.Find(ExistingProductQuery).FirstOrDefault();

                BsonDocument NewCart = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, {"C_ID", Carts.CartID }, };


                if (ProductSearchOutcome != null)
                {
                    Carts.CartProducts = new System.Collections.Generic.Dictionary<string, string[]>();
                    Carts.CartProducts.Add(Guid.NewGuid().ToString(), new string[5] {ProductSearchOutcome.GetElement(1).Value.ToString(), 
                                                                                    ProductSearchOutcome.GetElement(2).Value.ToString(),
                                                                                    ProductSearchOutcome.GetElement(3).Value.ToString(),
                                                                                    ProductSearchOutcome.GetElement(4).Value.ToString(),
                                                                                    ProductSearchOutcome.GetElement(5).Value.ToString()});
                    ResponseMessage = Carts.CartProducts.Values.ToJson();
                    return new OkObjectResult(ResponseMessage);
                }
                else
                {

                    ResponseMessage = "Cart Already Exists.";
                    return new OkObjectResult(ResponseMessage);
                }

            }
            else if (Item.ProductName == null || Item.ProductName.Length < 3)
            {
                ResponseMessage = "The product name you entered is too short" + "\n" +
                                   "Please re-enter a product name, with a length of 3 letters or more.";

                return new BadRequestObjectResult(ResponseMessage);

            }
         
            return new OkObjectResult(ResponseMessage);
        }

    }

}