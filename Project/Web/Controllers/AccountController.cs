using Domain.Models;
using Domain.ViewModel;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }
                var user = new User
                {
                    Name = model.Name,
                    UserName = model.Email,
                    Email = model.Email
                    
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                 

                if (result.Succeeded)
                {
                    // Assign default role
                    await _userManager.AddToRoleAsync(user, "Customer");
                    var customer = new Domain.Models.Customer
                    {
                        UserId = user.Id,
                        PhoneNumber = model.PhoneNumber
                    };
                    _unitOfWork._customer.Add(customer);
                    await _unitOfWork.CompleteAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email,model.Password, false,lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Redirect to original URL or default home
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> CreateRoles()
        //{
        //    string[] roleNames = { "Admin", "Customer" };

        //    foreach (var roleName in roleNames)
        //    {
        //        // Check if role exists
        //        var roleExist = await _roleManager.RoleExistsAsync(roleName);

        //        if (!roleExist)
        //        {
        //            // Create the roles and seed them to the database
        //            await _roleManager.CreateAsync(new IdentityRole(roleName));
        //        }
        //    }

        //    return Content("Roles created successfully");
        //}
    }
}
