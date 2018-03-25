using AquariumAPI.Controllers;
using AquariumMonitor.APIModels;
using AquariumMonitor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquariumAPI.Models
{
    public class AquariumUrlResolver : IValueResolver<Aquarium, AquariumModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AquariumUrlResolver(IHttpContextAccessor httpContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Aquarium source, AquariumModel destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("AquariumGet", new { userId = source.User.Id, aquariumId = source.Id });
        }
    }
}
