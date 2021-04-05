using JwtDeneme.Data;
using JwtDeneme.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtDeneme.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        DataContext _context;
        AppSettings _appSettings;
        public UsersController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        //[AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetAll() 
        {

            var values = await _context.Users.ToListAsync();

            return Ok(values);

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] User user) 
        {

           _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return Ok(user);
        }

        [HttpPost("Token")]
        public User Authenticate([FromBody] AuthModel model)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);
            if (user == null) return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            user.Password = null;
            return user;
        }


    }
}
