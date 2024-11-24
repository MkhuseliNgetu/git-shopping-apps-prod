using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using git_shopping_apps_prod.Databases.Models;

namespace git_shopping_apps_prod.Products
{
    public static class git_shopping_apps_prod_product
    {
        private static readonly string AtlasDBConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:NoSQLConnectionString");
        private static MongoClient MongoDBClient;
        public static IMongoDatabase NightCityLab;
        public static IMongoCollection<BsonDocument> NCLCollection;


        [FunctionName("git_shopping_apps_prod_add_product")]
        public static async Task<IActionResult> RunAddProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {

            log.LogInformation("Add To Cart HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Product Item = new Product();
            Item.ProductID = Guid.NewGuid().ToString();
            Item.ProductName = FormData["ProductName"];
            Item.ProductPrice = (decimal)Convert.ToDecimal(FormData["ProductPrice"]);
            Item.ProductColor = FormData["ProductColor"];
            Item.ProductSize = (int)Convert.ToInt64(FormData["ProductSize"]);
            Item.ProductSpecialDiscountStatus = false;
            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (Item.ProductName != null && Item.ProductPrice != null && Item.ProductColor != null && Item.ProductSize != null)
            {
                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Products");

                BsonDocument NewItem = new BsonDocument { { "_id", ObjectId.GenerateNewId() },
                    { "P_GUID",  Item.ProductID }, { "P_ProductName", Item.ProductName },
                    { "P_ProductPrice", Item.ProductPrice },{ "P_ProductColor", Item.ProductColor },
                    { "P_ProductSize", Item.ProductSize },{ "P_DiscountStatus",  Item.ProductSpecialDiscountStatus } };

                var ExistingProductFilter = Builders<BsonDocument>.Filter;
                var ExistingProductQuery = ExistingProductFilter.Eq("P_GUID", Guid.NewGuid().ToByteArray()) & ExistingProductFilter.Eq("_id", ObjectId.GenerateNewId());

                var SearchOutcome = NCLCollection.Find(ExistingProductQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    ResponseMessage = "Error: Product already exists.";
                    return new OkObjectResult(ResponseMessage);
                }
                else
                {
                    var SaveItem = NCLCollection.InsertOneAsync(NewItem);
                    ResponseMessage = "New Product Captured Successfully.";
                    return new OkObjectResult(ResponseMessage);
                }

            }
            else if (Item.ProductName == null || Item.ProductName.Length < 3)
            {
                ResponseMessage = "The product name you entered is too short" + "\n" +
                                   "Please re-enter a product name, with a length of 3 letters or more.";

                return new BadRequestObjectResult(ResponseMessage);

            }
            else if (Item.ProductPrice < 4 || Item.ProductPrice > 4)
            {
                ResponseMessage = "The price you provided for this product is too short or long" + "\n" +
                                   "Please re-enter a 4 digit price (00.00) for this product.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (Item.ProductColor == null)
            {
                ResponseMessage = "The product color you entered is too short" + "\n" +
                                   "Please re-enter a product color, with a length of 3 letters or more.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (Item.ProductSize < 2)
            {
                ResponseMessage = "The size you provided for this product is too short" + "\n" +
                                  "Please re-enter a size of two digits or higher for this product.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            return new OkObjectResult(ResponseMessage);
        }

        [FunctionName("git_shopping_apps_prod_get_product")]
        public static async Task<IActionResult> RunGetProduct([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {

            log.LogInformation("Get Product HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Product Item = new Product();
            Item.ProductName = FormData["ProductName"];


            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (Item.ProductName != null)
            {
                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Products");

                BsonDocument ExistingItem = new BsonDocument {{ "P_ProductName", Item.ProductName }};

                var ExistingProductFilter = Builders<BsonDocument>.Filter;
                var ExistingProductQuery = ExistingProductFilter.Eq("P_ProductName", Item.ProductName);

                var SearchOutcome = NCLCollection.Find(ExistingProductQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    ResponseMessage = SearchOutcome.ToJson();
                    return new OkObjectResult(ResponseMessage);
                }
                else
                {
                    ResponseMessage = "Error: Product Does Not Exist";
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

        [FunctionName("git_shopping_apps_prod_update_product")]
        public static async Task<IActionResult> RunUpdateProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {

            log.LogInformation("Update Product HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Product Item = new Product();
            Item.ProductName = FormData["ProductName"];
            Item.ProductPrice = (decimal)Convert.ToDecimal(FormData["ProductPrice"]);
            Item.ProductColor = FormData["ProductColor"];
            Item.ProductSize = (int)Convert.ToInt64(FormData["ProductSize"]);
            Item.ProductSpecialDiscountStatus = true;
            Item.ProductSpecialDiscount = (decimal)Convert.ToDecimal(FormData["ProductSpecialDiscount"]);
            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (Item.ProductName != null && Item.ProductPrice != null && Item.ProductColor != null && Item.ProductSize != null)
            {
                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Products");

                BsonDocument ExistingItem = new BsonDocument { { "P_ProductName", Item.ProductName } };

                var ExistingProductFilter = Builders<BsonDocument>.Filter;
                var ExistingProductQuery = ExistingProductFilter.Eq("P_ProductName", Item.ProductName);

                var SearchOutcome = NCLCollection.Find(ExistingProductQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    BsonDocument UpdatedItem = new BsonDocument {  { "P_ProductName", Item.ProductName },
                    { "P_ProductPrice", Item.ProductPrice },{ "P_ProductColor", Item.ProductColor },
                    { "P_ProductSize", Item.ProductSize },{ "P_DiscountStatus",  Item.ProductSpecialDiscountStatus },
                    { "P_SpecialDiscount",  (Item.ProductSpecialDiscount/100 * Item.ProductPrice)  }};

                    var UpdatedOutcome = NCLCollection.UpdateOneAsync(ExistingItem, UpdatedItem);

                    ResponseMessage = "Error: Product already exists.";
                    return new OkObjectResult(ResponseMessage);
                }
                else
                {
                    ResponseMessage = "Error: Product update failed.";
                    return new OkObjectResult(ResponseMessage);
                }

            }
            else if (Item.ProductName == null || Item.ProductName.Length < 3)
            {
                ResponseMessage = "The product name you entered is too short" + "\n" +
                                   "Please re-enter a product name, with a length of 3 letters or more.";

                return new BadRequestObjectResult(ResponseMessage);

            }
            else if (Item.ProductPrice < 4 || Item.ProductPrice > 4)
            {
                ResponseMessage = "The price you provided for this product is too short or long" + "\n" +
                                   "Please re-enter a 4 digit price (00.00) for this product.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (Item.ProductColor == null)
            {
                ResponseMessage = "The product color you entered is too short" + "\n" +
                                   "Please re-enter a product color, with a length of 3 letters or more.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            else if (Item.ProductSize < 2)
            {
                ResponseMessage = "The size you provided for this product is too short" + "\n" +
                                  "Please re-enter a size of two digits or higher for this product.";

                return new BadRequestObjectResult(ResponseMessage);
            }
            return new OkObjectResult(ResponseMessage);
        }

        [FunctionName("git_shopping_apps_prod_delete_product")]
        public static async Task<IActionResult> RunDeleteProduct([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {

            log.LogInformation("Delete Product HTTP trigger function processed a request.");

            //Capture form data
            var FormData = await req.ReadFormAsync();

            //HTTP Request Bodies
            Product Item = new Product();
            Item.ProductName = FormData["ProductName"];


            //The body of what is being sent back to users.
            string ResponseMessage = "";

            if (Item.ProductName != null)
            {
                //Databases
                MongoDBClient = new MongoClient(AtlasDBConnectionString);
                NightCityLab = MongoDBClient.GetDatabase("NightCityLab");
                NCLCollection = NightCityLab.GetCollection<BsonDocument>("Products");

                BsonDocument ExistingItem = new BsonDocument { { "P_ProductName", Item.ProductName } };

                var ExistingProductFilter = Builders<BsonDocument>.Filter;
                var ExistingProductQuery = ExistingProductFilter.Eq("P_ProductName", Item.ProductName);

                var SearchOutcome = NCLCollection.Find(ExistingProductQuery).FirstOrDefault();

                if (SearchOutcome != null)
                {
                    NCLCollection.DeleteOneAsync(SearchOutcome);
                    ResponseMessage = "Product removed successfully";
                    return new OkObjectResult(ResponseMessage);
                }
                else
                {
                    ResponseMessage = "Error: Product Does Not Exist";
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
