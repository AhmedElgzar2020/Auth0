using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Auth0.Models;
using Microsoft.AspNetCore.Authentication;

namespace Auth0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        private static readonly HttpClient _httpClient = new HttpClient();
        public UsersController( IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("grant_type", "password"));
            data.Add(new KeyValuePair<string, string>("username", userName));
            data.Add(new KeyValuePair<string, string>("password", password));
            data.Add(new KeyValuePair<string, string>("audience", Configuration["Auth0:Audience"]));
            data.Add(new KeyValuePair<string, string>("client_id", Configuration["Auth0:ClientId"]));
            data.Add(new KeyValuePair<string, string>("client_secret", Configuration["Auth0:ClientSecret"]));
            data.Add(new KeyValuePair<string, string>("scope", "openid"));

            var dataContent = new FormUrlEncodedContent(data);

            var response = await _httpClient.PostAsync(Configuration["Auth0:AuthURL"], dataContent);
            var responseString = await response.Content.ReadAsStringAsync();
            var data_ = JsonSerializer.Deserialize<AuthResult>(responseString);
            return Ok(new
            {
                Token = data_.access_token,
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> profile()
        {
            var token = HttpContext.GetTokenAsync("Bearer", "access_token");
            _httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue( "Bearer",  token.Result);
            //_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Result);
            var response = await _httpClient.GetAsync(Configuration["Auth0:URL"] + "userinfo");
            var responseString = await response.Content.ReadAsStringAsync();
            var data_ = JsonSerializer.Deserialize<object>(responseString);
            return Ok(data_);
        }
    }
}
