using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Infrastructure.Persistence;
using BasedTechStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasedTechStore.Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var claimIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimIdClaim == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            if (!Guid.TryParse(claimIdClaim.Value, out var userId))
            {
                return BadRequest("Invalid user id");
            }

            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var viewModel = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(viewModel);
        }

    }
}
