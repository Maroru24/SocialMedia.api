using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialMedia.core.Entities;
using SocialMedia.core.Interfaces;
using SocialMedia.infrastructure.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ISecurityService _securityService;
        private readonly IPasswordService _passwordHasher;
        public TokenController(IConfiguration configuration, ISecurityService securityService, IPasswordService passwordService) 
        { 
            _configuration = configuration;
            _securityService = securityService;
            _passwordHasher = passwordService;
        }

        [HttpPost]
        public async  Task<IActionResult> Authentication(UserLogin userLogin) 
        {
            var validation = await IsValidUser(userLogin);
            if(validation.Item1)
            {
                var token = GenerateToken(validation.Item2);
                return Ok(new { token });
            }
            return NotFound();
        }
        private string GenerateToken(Security security)
        {
            //la mecanica de este metodo sera generar un token de la manera en la que trabaja jwt con ellos, creando primero el header, luego un claim y por ultimo el payload
            //HEADER
            var symmetricSecurity = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
            var signingCredentials = new SigningCredentials(symmetricSecurity, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);

            //CLAIMS
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, security.UserName),
                new Claim("User",  security.User),
                new Claim(ClaimTypes.Role, security.Rol.ToString()),
            };
            //PAYLOAD
            var payload = new JwtPayload
            (
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(10)
            );

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<(bool, Security)> IsValidUser(UserLogin userLogin)
        {
            var user = await _securityService.GetLoginByCredentials(userLogin);
            var isValid = _passwordHasher.Verify((user.Password), userLogin.Password);
            return (isValid, user);
        }
    }
}
