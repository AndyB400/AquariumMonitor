using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BusinessLogic.Interfaces;

namespace AquariumMonitor.API.Controllers
{
    public abstract class BaseController : Controller
    {
        public const string URLHELPER = "URLHELPER";
        protected readonly IMapper Mapper;
        protected readonly ILoggerAdapter<BaseController> Logger;

        protected BaseController(ILoggerAdapter<BaseController> _logger, IMapper mapper)
        {
            Mapper = mapper;
            Logger = _logger;
        }

        private int _userId;
        protected int UserId
        {
            get
            {
                if (_userId == 0)
                { 
                    var claimUserId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                    _userId = int.Parse(claimUserId.Value);
                }
                return _userId;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            context.HttpContext.Items[URLHELPER] = this.Url;
        }

        protected ObjectResult UnprocessableEntity(List<ValidationResult> results)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { Errors = results.Select(e => e.ErrorMessage).ToList() });
        }

        protected StatusCodeResult UnprocessableEntity()
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        protected void AddETag(byte[] rowVersion)
        {
            var etag = Convert.ToBase64String(rowVersion);
            Response.Headers.Add(HeaderNames.ETag, etag);
        }
    }
}
