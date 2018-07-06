using AquariumMonitor.API.Controllers;
using AquariumMonitor.Models;
using AquariumMonitor.Models.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquariumMonitor.API.Models
{
    public class WaterChangeUrlResolver : IValueResolver<WaterChange, WaterChangeModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WaterChangeUrlResolver(IHttpContextAccessor httpContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(WaterChange source, WaterChangeModel destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("WaterChangeGet", new { userId = source.UserId, aquariumId = source.AquariumId, waterChangeId = source.Id });
        }
    }
}
