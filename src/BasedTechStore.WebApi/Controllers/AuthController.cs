using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Requests;
using BasedTechStore.Application.DTOs.Identity.Responses;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Common.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route(ApiRoutes.Auth.Base)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost(ApiRoutes.Auth.SignIn)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> SignIn([FromBody] SignInDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (accessToken, refreshToken) = await _authService.SignInAsync(dto, ipAddress);

        SetRefreshTokenCookie(refreshToken);

        return Ok(ApiResponse<AuthTokenResponse>.Success(
            new AuthTokenResponse
            {
                Token = accessToken,
                TokenType = "Bearer",
                ExpiresIn = GetAccessTokenExpirySeconds()
            },
            "Authentication successful"
        ));
    }

    [HttpPost(ApiRoutes.Auth.SignUp)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> SignUp([FromBody] SignUpDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (accessToken, refreshToken) = await _authService.SignUpAsync(dto, ipAddress);

        SetRefreshTokenCookie(refreshToken);

        return CreatedAtAction(
            nameof(GetCurrentUser),
            null,
            ApiResponse<AuthTokenResponse>.Success(
                new AuthTokenResponse
                {
                    Token = accessToken,
                    TokenType = "Bearer",
                    ExpiresIn = GetAccessTokenExpirySeconds(),
                },
                "Registration successul"
            )
        );
    }

    [Authorize]
    [HttpPost(ApiRoutes.Auth.SignOut)]
    public async new Task<ActionResult<ApiResponse<object>>> SignOut()
    {
        var userId = _authService.GetUserId(User);
        var refreshToken = Request.Cookies["refreshToken"];
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!string.IsNullOrEmpty(userId))
            await _authService.SignOutAsync(userId, refreshToken, ipAddress);

        DeleteRefreshTokenCookie();

        return Ok(ApiResponse<object>.Success(null, "Sign out successful"));
    }

    [Authorize]
    [HttpPost(ApiRoutes.Auth.SignOutAll)]
    public async Task<ActionResult<ApiResponse<object>>> SignOutAllDevices()
    {
        var userId = _authService.GetUserId(User);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!string.IsNullOrEmpty(userId))
            await _authService.SignOutAllDevicesAsync(userId, ipAddress);

        DeleteRefreshTokenCookie();

        return Ok(ApiResponse<object>.Success(null, "Sign out from all devices successful"));
    }

    [HttpPost(ApiRoutes.Auth.RefreshToken)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(ApiResponse<AuthTokenResponse>.Failure("Refresh token not fount"));

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (accessToken, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

        SetRefreshTokenCookie(newRefreshToken);

        return Ok(ApiResponse<AuthTokenResponse>.Success(
            new AuthTokenResponse
            {
                Token = newRefreshToken,
                TokenType = "Bearer",
                ExpiresIn = GetAccessTokenExpirySeconds()
            },
            "Token refreshed"
        ));
    }

    [HttpGet(ApiRoutes.Auth.CheckAuth)]
    public ActionResult<ApiResponse<AuthStatusResponse>> CheckAuth()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

        return Ok(ApiResponse<AuthStatusResponse>.Success(
            new AuthStatusResponse
            {
                IsAuthenticated = isAuthenticated,
                UserId = isAuthenticated ? _authService.GetUserId(User) : null,
                UserName = User.Identity?.Name,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            }
        ));
    }

    [Authorize]
    [HttpGet(ApiRoutes.Auth.GetCurrentUser)]
    public ActionResult<ApiResponse<CurrentUserResponse>> GetCurrentUser()
    {
        var userId = _authService.GetUserId(User);

        return Ok(ApiResponse<CurrentUserResponse>.Success(
            new CurrentUserResponse
            {
                UserId = userId ?? string.Empty,
                FullName = User.FindFirst("FullName")?.Value,
                UserName = User.Identity?.Name,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            }
        ));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")),
            Path = "/"
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken");
    }

    private int GetAccessTokenExpirySeconds()
    {
        return int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15") * 60;
    }
}