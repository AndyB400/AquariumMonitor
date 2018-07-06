using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AquariumMonitor.API.Filters
{
    public class AquariumSecurityCheckAttribute : TypeFilterAttribute
    {
        public AquariumSecurityCheckAttribute() : base(typeof(AquariumSecurityCheckFilterImpl))
        {
        }

        private class AquariumSecurityCheckFilterImpl : IAsyncActionFilter
        {
            private readonly IAquariumRepository _repository;
            private readonly IUserRepository _userRepository;

            public AquariumSecurityCheckFilterImpl(IAquariumRepository repository, IUserRepository userRepository)
            {
                _repository = repository;
                _userRepository = userRepository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.Count == 0)
                {
                    await next(); 
                    return;
                }

                if (!context.ActionArguments.ContainsKey("aquariumId"))
                {
                    await next();  
                    return;
                }

                // Check for Admin User
                var adminUser = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == SecurityPolicy.Admin);
                if (adminUser != null) {
                    await next();  // Admin User, let him through
                    return;
                }

                if (! context.ActionArguments.ContainsKey("aquariumId"))
                {
                    context.Result = new BadRequestObjectResult(new { Error = "AquariumId is required" });
                    return;
                }

                // Ensure arguements are an int
                var aquariumIdArguement = context.ActionArguments["aquariumId"].ToString();

                if ( !int.TryParse(aquariumIdArguement, out int aquariumId))
                {
                    context.Result = new NotFoundObjectResult(new { Error = $"Invalid AquariumId:{aquariumId}" });
                    return;
                }

                // Get the Logged in User
                var claimUserId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (claimUserId == null || !int.TryParse(claimUserId.Value, out int userId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Load aquarium from the URL Id
                var exists = await _repository.Exists(userId, aquariumId);
                if (! exists)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                await next();
            }
        }
    }
}
