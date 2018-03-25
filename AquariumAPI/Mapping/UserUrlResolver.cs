using AquariumAPI.Controllers;
using AquariumMonitor.Models;
using AquariumMonitor.Models.APIModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquariumAPI.Models
{
    public class UserUrlResolver : IValueResolver<User, UserModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserUrlResolver(IHttpContextAccessor httpContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(User source, UserModel destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("UserGet", new { userId = source.Id });
        }
    }
}
