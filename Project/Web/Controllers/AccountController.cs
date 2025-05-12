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
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
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
                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("Index", "Customer");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            //ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email,model.Password, false,lockoutOnFailure: false);


                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    // Redirect based on role
                    if (roles.Contains("Admin"))
                    {
                        //return RedirectToAction("Dashboard", "Admin");
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (roles.Contains("Customer"))
                    {
                        return RedirectToAction("Index", "Customer");
                    }
                    // Redirect to original URL or default home
                    //return LocalRedirect(returnUrl ?? Url.Content("~/"));
                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("Index", "Home");
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
    }
}
