using Microsoft.AspNetCore.Identity;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web
{
    public class CustomUserValidator : IUserValidator<YolcuUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<YolcuUser> manager, YolcuUser user)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrEmpty(user.Email))
            {
                errors.Add(new IdentityError
                {
                    Code = "EmailRequired",
                    Description = "Email adresi gereklidir."
                });
            }

            return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
} 