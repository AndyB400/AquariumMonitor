using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AquariumMonitor.API.Filters
{
    public class UserSecurityCheckAttribute : TypeFilterAttribute
    {
        public UserSecurityCheckAttribute() : base(typeof(UserSecurityCheckFilterImpl))
        {
        }

        private class UserSecurityCheckFilterImpl : IAsyncActionFilter
        {

            private readonly IUserRepository _repository;
            public UserSecurityCheckFilterImpl(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.Count == 0)
                {
                    await next();
                    return;
                }

                // Check for Admin User
                var adminUser = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == SecurityPolicy.Admin);
                if (adminUser != null)
                {
                    await next(); // Admin User, let him through
                    return;
                }

                // Get the Logged in Users username
                var claimUserId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if(claimUserId == null || !int.TryParse(claimUserId.Value, out int userId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Load the User and compare their Ids
                var user = await _repository.Get(userId);

                if (user.Id != userId)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                await next();
            }
        }
    }
}
