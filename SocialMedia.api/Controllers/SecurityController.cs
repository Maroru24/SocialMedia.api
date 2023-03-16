using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.api.Responses;
using SocialMedia.core.DTOs;
using SocialMedia.core.Entities;
using SocialMedia.core.Enumerations;
using SocialMedia.core.Interfaces;
using SocialMedia.core.Services;
using SocialMedia.infrastructure.Interfaces;
using System.Threading.Tasks;

namespace SocialMedia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public class SecurityController : ControllerBase
    {
       
        private readonly ISecurityService _securityService;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        public SecurityController(ISecurityService securityService, IMapper mapper, IPasswordService passwordService)
        {
            _securityService = securityService;
            _mapper = mapper;
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(SecurityDto securityDto)
        {
            var security = _mapper.Map<Security>(securityDto);
            security.Password = _passwordService.Hash(security.Password);
            await _securityService.RegisterUser(security);
            
            var response = new ApiResponses<Security>(security);
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int id, SecurityDto securityDto)
        {
            var security = _mapper.Map<Security>(securityDto);
            security.Password = _passwordService.Hash(security.Password);
            await _securityService.UpdateUser(security);
            var response = new ApiResponses<Security>(security);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _securityService.DeleteUser(id);
            var response = new ApiResponses<bool>(result);
            return Ok(response);
        }
    }
}
