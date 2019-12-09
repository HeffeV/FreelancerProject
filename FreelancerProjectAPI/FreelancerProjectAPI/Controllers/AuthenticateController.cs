using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreelancerProjectAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private IUserService _userService;
        public AuthenticateController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]Models.User userParam)
        {
            //authenticate user
            var user = _userService.Authenticate(userParam.Email, userParam.Password);
            if (user == null) return BadRequest(new { message = "Email or password is incorrect" });
            return Ok(user);
        }
    }
}