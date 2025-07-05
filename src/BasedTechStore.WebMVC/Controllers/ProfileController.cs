using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasedTechStore.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is not authenticated");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"User is authenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"User Name: {User.Identity.Name}");

            var allClaims = User.Claims.ToList();
            Console.WriteLine($"Total claims: {allClaims.Count}");
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var claimIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirst("sub");

            if (claimIdClaim == null)
            {
                Console.WriteLine("NameIdentifier claim not found");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"Found User ID claim: {claimIdClaim.Value}");

            if (!Guid.TryParse(claimIdClaim.Value, out var userId))
            {
                Console.WriteLine($"Invalid user ID format: {claimIdClaim.Value}");
                return BadRequest("Invalid user id");
            }

            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"User not found in database: {userId}");
                return NotFound("User not found");
            }

            var viewModel = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            Console.WriteLine("Successfully loaded user profile");
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult TestAuth()
        {
            var result = new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Name = User.Identity.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Json(result);
        }
    }
}
