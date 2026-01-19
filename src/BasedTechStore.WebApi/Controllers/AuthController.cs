using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Common.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route(ApiRoutes.Auth.Base)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, ILogger<AuthController> logger,
        IMapper mapper)
    {
        _authService = authService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost(ApiRoutes.Auth.SignIn)]
    public async Task<IActionResult> SignIn([FromBody] SignInVM signInVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Validation errors occurred.",
                    Errors = errors,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var signInRequest = _mapper.Map<SignInDto>(signInVM);
            var signInResponse = await _authService.SignInAsync(signInRequest);

            if (!signInResponse.IsSuccess)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Authentication failed.",
                    Errors = signInResponse.Errors?.ToList() ?? new List<string> { "Unknown error occurred." },
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }

            if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                expirationMinutes = 60;

            Response.Cookies.Append("access-token", signInResponse.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Path = "/"
            });

            return Ok(new ApiResponse<AuthResponseVM>
            {
                Data = new AuthResponseVM
                {
                    Token = signInResponse.Token,
                    ExpiresIn = (int)expirationMinutes * 60
                },
                IsSuccess = true,
                Message = "Authentication successful.",
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during sign-in.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An internal server error occurred.",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpPost(ApiRoutes.Auth.SignUp)]
    public async Task<IActionResult> SignUp([FromBody] SignUpVM signUpVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Validation errors occurred.",
                    Errors = errors,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var signUpRequest = _mapper.Map<SignUpDto>(signUpVM);
            var signUpResponse = await _authService.SignUpAsync(signUpRequest);

            if (!signUpResponse.IsSuccess)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Registration failed.",
                    Errors = signUpResponse.Errors?.ToList() ?? new List<string> { "Unknown error occurred." },
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                expirationMinutes = 60;

            Response.Cookies.Append("access-token", signUpResponse.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Path = "/"
            });

            return CreatedAtAction(nameof(GetCurrentUser), null, new ApiResponse<AuthResponseVM>
            {
                Data = new AuthResponseVM
                {
                    Token = signUpResponse.Token,
                    ExpiresIn = (int)expirationMinutes * 60
                },
                IsSuccess = true,
                Message = "Registration successful.",
                StatusCode = StatusCodes.Status201Created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during sign-up.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An internal server error occurred.",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [Authorize]
    [HttpPost(ApiRoutes.Auth.SignOut)]
    public new async Task<IActionResult> SignOut()
    {
        try
        {
            await _authService.SignOutAsync();
            Response.Cookies.Delete("access-token");

            return Ok(new ApiResponse<string>
            {
                Data = null,
                IsSuccess = true,
                Message = "Sign-out successful.",
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during sign-out.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An internal server error occurred.",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpPost(ApiRoutes.Auth.RefreshToken)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenVM refreshTokenVM)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshTokenVM.Token))
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Refresh token is required.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _authService.RefreshJwtTokenAsync(refreshTokenVM.Token);

            if (!response.IsSuccess)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Token refresh failed",
                    Errors = response.Errors?.ToList() ?? new List<string> { "Invalid token" },
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }

            if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                expirationMinutes = 60;

            Response.Cookies.Append("access-token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Path = "/"
            });

            return CreatedAtAction(nameof(GetCurrentUser), null, new ApiResponse<AuthResponseVM>
            {
                Data = new AuthResponseVM
                {
                    Token = response.Token,
                    ExpiresIn = (int)expirationMinutes * 60
                },
                IsSuccess = true,
                Message = "Token refresh successful.",
                StatusCode = StatusCodes.Status201Created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Auth.CheckAuth)]
    public IActionResult CheckAuth()
    {
        try
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return Ok(new ApiResponse<AuthStatusVM>
                {
                    Data = new AuthStatusVM
                    {
                        IsAuthenticated = false
                    },
                    Message = "User is not authenticated",
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK
                });
            }

            var authStatus = new AuthStatusVM
            {
                IsAuthenticated = true,
                UserName = User.Identity?.Name,
                Claims = User.Claims.Select(c => new ClaimsVM
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            return Ok(new ApiResponse<AuthStatusVM>
            {
                Data = authStatus,
                Message = "Authentication status retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while checking authentication status",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Auth.GetCurrentUser)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = _authService.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated.",
                    Errors = new List<string> { "Token not contain userId" },
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }

            var userInfo = new CurrentUserVM
            {
                UserId = userId,
                FullName = User.FindFirst("FullName")?.Value,
                UserName = User.FindFirst("UserName")?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            };

            return Ok(new ApiResponse<CurrentUserVM>
            {
                Data = userInfo,
                IsSuccess = true,
                Message = "User information retrieved successfully.",
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving current user information.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An internal server error occurred.",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }
}