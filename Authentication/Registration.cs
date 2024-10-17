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


namespace SyanStudios.gitshoppingappsprod.registration
{

    public static class git_shopping_apps_prod_main_registration
    {
        [FunctionName("git_shopping_apps_prod_registration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Registration HTTP trigger function processed a request.");

           //Capture form data
            var FormData = await req.ReadFormAsync();
            
            //HTTP Request Bodies
            string UserName = FormData["Username"];
            string PassCode = FormData["Passcode"];

            //Default Credential Sizes
            int MinUsernameLength = 5;
            int MinPasscodeLength = 8;
            
            //The body of what is being sent back to users.
            string ResponseMessage = "";

               if(UserName.Length >= MinUsernameLength && PassCode.Length >= MinPasscodeLength && Regex.IsMatch(UserName, @"^[A-Za-z]{3,}$")){
                //Password Security
                var SCREPassCode = BCr.BCrypt.HashPassword(PassCode);
                ResponseMessage = "User created successfully";
                
                }else if((UserName.Length < MinUsernameLength && string.IsNullOrWhiteSpace(UserName)) || UserName.Length < MinUsernameLength){
                     ResponseMessage = "Username you entered is too short and/or contains whitespaces."+"\n"+
                                        "Please re-enter a username in accordance to the guidelines.";

                     return new BadRequestObjectResult(ResponseMessage);

                }else if((PassCode.Length < MinPasscodeLength && string.IsNullOrWhiteSpace(PassCode)) || PassCode.Length < MinPasscodeLength){
                     ResponseMessage = "Password you entered is too short and/or contains whitespaces"+"\n"+
                                        "Please re-enter a password in accordance to the guidelines.";

                     return new BadRequestObjectResult(ResponseMessage);
                }


            return new OkObjectResult(ResponseMessage);
        }
    }
}