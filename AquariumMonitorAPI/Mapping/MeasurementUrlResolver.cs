using AquariumMonitor.API.Controllers;
using AquariumMonitor.Models;
using AquariumMonitor.Models.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquariumMonitor.API.Models
{
    public class MeasurementUrlResolver : IValueResolver<Measurement, MeasurementModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MeasurementUrlResolver(IHttpContextAccessor httpContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Measurement source, MeasurementModel destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("MeasurementGet", new { userId = source.UserId, aquariumId = source.AquariumId, measurementId = source.Id });
        }
    }
}
