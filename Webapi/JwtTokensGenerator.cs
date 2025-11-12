using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Webapi
{
    public class JwtTokensGenerator
    {
        private readonly IConfiguration _config; 

        public JwtTokensGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string username)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],               
                audience: _config["JwtSettings:Audience"],           
                claims: claims,                                      
                expires: DateTime.UtcNow.AddMinutes(                 
                    double.Parse(_config["JwtSettings:ExpiresInMinutes"])
                ),
                signingCredentials: new SigningCredentials(          
                    new SymmetricSecurityKey(key),                   
                    SecurityAlgorithms.HmacSha256                  
                )
            );

            // 4️⃣ Convertir el objeto token a texto
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}