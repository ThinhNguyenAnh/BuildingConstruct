﻿using Application.ClaimTokens;
using Application.System.Users;
using Data.DataContext;
using Data.Entities;
using Data.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using ViewModels.Pagination;
using ViewModels.Users;

namespace BuildingConstructApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]

    public class UserController : ControllerBase
    {
        public string userID { get; set; }
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        public UserController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("loginOthers")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithGoogle([FromBody] LoginGoogleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var rs = await _userService.LoginGoogle(request);
            //if (rs.Data == null)
            //{
            //    return Ok(new
            //    {
            //        code = BaseCode.ERROR,
            //        Message = "Username or Password is Incorrect"
            //    });
            //}
            //else
            //{
                var token = await _userService.GenerateToken(rs.Data);
                if (token != null)
                {
                    try
                    {
                        var userPrincipalac = this.ValidateToken(token.Data.AccessToken);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(30),
                            IsPersistent = true,
                            AllowRefresh = true,
                        };
                        await HttpContext.SignInAsync(userPrincipalac, authProperties);
                    }
                    catch (Exception)
                    {
                    }
                    //token.Message = "Login Success";
                    //token.Code = BaseCode.SUCCESS;
                    //rs.Code = token.Code;
                    //rs.Message = token.Message;
                    
                    rs.Data.AccessToken = token.Data.AccessToken;
                    rs.Data.RefreshToken = token.Data.RefreshToken;
                    rs.Data.RefreshTokenExpiryTime = token.Data.RefreshTokenExpiryTime;
                }
                return Ok(rs);
            //}
        }

        [HttpPost("SetRole")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var rs = await _userService.UpdateRole(request);
            if (rs.Data == null)
            {
                return Ok(new
                {
                    code = BaseCode.ERROR,
                    Message = "Username or Password is Incorrect"
                });
            }
            else
            {
                var token = await _userService.GenerateToken(rs.Data);
                if (token != null)
                {
                    try
                    {
                        var userPrincipalac = this.ValidateToken(token.Data.AccessToken);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
                            IsPersistent = true,
                            AllowRefresh = true,
                        };
                        await HttpContext.SignInAsync(userPrincipalac, authProperties);
                    }
                    catch (Exception)
                    {
                    }
                    rs.Data.AccessToken = token.Data.AccessToken;
                    rs.Data.RefreshToken = token.Data.RefreshToken;
                    rs.Message = "Update Success";
                    rs.Code = BaseCode.SUCCESS;

                }
                return Ok(rs);

            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var rs = await _userService.Login(request);
            if (rs.Data == null)
            {
                return Ok(new
                {
                    code = BaseCode.ERROR,
                    Message = "Username or Password is Incorrect"
                });
            }
            else
            {
                var userModels = new UserModels()
                {
                    UserName = rs.Data.UserName,
                    Id = rs.Data.Id,
                    Phone = rs.Data.Phone,
                    Role = rs.Data.Role
                };
                var token = await _userService.GenerateToken(userModels);
                if (token != null)
                {
                    try
                    {
                        var userPrincipalac = this.ValidateToken(token.Data.AccessToken);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
                            IsPersistent = true,
                            AllowRefresh = true,
                        };
                        await HttpContext.SignInAsync(userPrincipalac, authProperties);
                    }
                    catch (Exception)
                    {
                    }
                    token.Message = "Login Success";
                    token.Code = BaseCode.SUCCESS;
                    rs.Code = token.Code;
                    rs.Message = token.Message;
                    rs.Data.AccessToken = token.Data.AccessToken;
                    rs.Data.RefreshToken = token.Data.RefreshToken;
                    rs.Data.RefreshTokenExpiryTime = token.Data.RefreshTokenExpiryTime;
                }
                return Ok(rs);

            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenResponse refreshToken)
        {
            var rs = await _userService.RefreshToken(refreshToken);

            return Ok(rs);
        }

        private UserModels MapToDto(User user, string roleName)
        {
            var userDto = new UserModels()
            {
                Id = user.Id,
                UserName = user.UserName,
                Phone = user.PhoneNumber
            };
            return userDto;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateToken([FromBody] UserModels request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var rs = _userService.GenerateToken(request);
            return Ok(rs);
        }

        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;

            validationParameters.ValidAudience = _configuration["Tokens:Issuer"];
            validationParameters.ValidIssuer = _configuration["Tokens:Issuer"];
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            ClaimsPrincipal principal;
            principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            RegisterResponseDTO rs = await _userService.Register(request);
            return Ok(rs);
        }

        [HttpGet("getUserID")]
        public async Task<IActionResult> Validate()
        {
            var rs = User.FindFirst("UserID").Value;
            //or if u want the list of claims
            userID = rs;
            var claims = User.Claims;
            return Ok(rs);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetUserDetail(Guid userID)
        {
            if (userID==Guid.Empty)
            {
                return BadRequest();
            }

            var result = await _userService.GetProfile(userID);
            return Ok(result);

        }

        [HttpGet("detail/favorite")]
        public async Task<IActionResult> GetContrusctor([FromQuery] PaginationFilter request)
        {
            var result = await _userService.GetContractorFavorite(request);
            return Ok(result);

        }

        [HttpGet("detail/builder/favorite")]
        public async Task<IActionResult> GetBuilderFavorite([FromQuery] PaginationFilter request)
        {
            var result = await _userService.GetBuilderFavorite(request);
            return Ok(result);

        }

        [HttpGet("detail/store/favorite")]
        public async Task<IActionResult> GetStoreFavorite([FromQuery] PaginationFilter request)
        {
            var result = await _userService.GetStoreFavorite(request);
            return Ok(result);

        }



        [HttpPut("update/builder")]
        public async Task<IActionResult> UpdateBuilder([FromBody] UpdateBuilderRequest request)
        {
            var userID = User.FindFirst("UserID").Value;
            if (userID == null)
            {
                return BadRequest();
            }
            var result = await _userService.UpdateBuilderProfile(request, Guid.Parse(userID));
            return Ok(result);

        }
        [HttpPut("update/contractor")]
        public async Task<IActionResult> UpdateContractor([FromBody] UpdateContractorRequest request)
        {
            var userID = User.FindFirst("UserID").Value;
            if (userID == null)
            {
                return BadRequest();
            }
            var result = await _userService.UpdateContractorProfile(request, Guid.Parse(userID));
            return Ok(result);
        }
        [HttpPut("update/store")]
        public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreRequest request)
        {
            var userID = User.FindFirst("UserID").Value;
            if (userID == null)
            {
                return BadRequest();
            }
            var result = await _userService.UpdateStoreProfile(request, Guid.Parse(userID));
            return Ok(result);

        }

    }
}
