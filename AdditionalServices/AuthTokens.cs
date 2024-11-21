using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using SyanStudios.gitshoppingappsprod.login;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.AdditionalServices
{
    internal class AuthTokens
    {

        public string AssignAuthTokens (string UserName) {

            //Token Development 
            var TokenHandler = new JwtSecurityTokenHandler();
            var TokenKey = Encoding.ASCII.GetBytes("OuterHeaven Authenticated");
            var TokenDetails = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                            new Claim(ClaimTypes.Name, UserName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(TokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var FinalToken = TokenHandler.CreateToken(TokenDetails);
            return TokenHandler.WriteToken(FinalToken);
        }
    }
}
