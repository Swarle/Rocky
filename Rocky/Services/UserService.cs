using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models.Models;

namespace Rocky.Services
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _accessor;
        private IApplicationUserRepository _repository;

        public UserService(IHttpContextAccessor accessor, IApplicationUserRepository repository)
        {
            _accessor = accessor;
            _repository = repository;
        }

        public async Task RegisterAsync(ApplicationUser user)
        {
            var isUserExist = _repository.FirstOrDefault(u => u.Email == user.Email || u.UserName == user.UserName);

            if (isUserExist != null)
            {
                throw new Exception("User with this email or user name already exist");
            }

            _repository.Add(user);
            _repository.Save();


            await Authenticate(user);
        }

        public async Task LoginAsync(ApplicationUser user)
        {
            await Authenticate(user);
        }

        public async Task LogoutAsync()
        {
            await _accessor.HttpContext.SignOutAsync();
        }

        public bool IsUserSignedIn()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public string GetUserName()
        {
            return _accessor.HttpContext.User.FindFirst(ClaimsIdentity.DefaultNameClaimType).Value;
        }

        public int GetUserId()
        {
            return Int32.Parse(_accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        private async Task Authenticate(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.RoleName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimIdentity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await _accessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimIdentity));
        }
    }
}
