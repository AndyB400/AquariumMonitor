using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AquariumMonitor.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AquariumMonitor.Models;
using AquariumMonitor.APIModels;
using Microsoft.AspNetCore.Cors;
using AquariumAPI.Filters;
using AquariumMonitor.BusinessLogic.Interfaces;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net;

namespace AquariumAPI.Controllers
{
    [AquariumSecurityCheck]
    [EnableCors("AquariumMonitor")]
    [Produces("application/json")]
    [Route("api/aquariums")]
    [ValidateModel]
    public class AquariumController : BaseController
    {
        private readonly IAquariumRepository _repository;
        private readonly IUnitManager _unitManager;
        private readonly IAquariumTypeManager _aquariumTypeManager;

        public AquariumController(IAquariumRepository repository,
            ILogger<AquariumController> logger, 
            IMapper mapper,
            IUnitManager unitManager,
            IAquariumTypeManager aquariumTypeManager) : base(logger, mapper)
        {
            _repository = repository;
            _unitManager = unitManager;
            _aquariumTypeManager = aquariumTypeManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetForUser()
        {
            var aquariums = await _repository.GetForUser(UserId);

            return Ok(_mapper.Map<IEnumerable<AquariumModel>>(aquariums));
        }

        [HttpGet("{aquariumId}", Name = "AquariumGet")]
        public async Task<IActionResult> Get(int aquariumId)
        {
            var aquarium = await _repository.Get(UserId, aquariumId);

            if (aquarium == null) return NotFound();

            AddETag(aquarium.RowVersion);

            return Ok(_mapper.Map<AquariumModel>(aquarium));
        }

        // POST: api/aquarium
        [HttpPost]
        [UserSecurityCheck]
        public async Task<IActionResult> Post([FromBody]AquariumModel model)
        {
            try
            {
                var aquarium = _mapper.Map<Aquarium>(model);

                if (aquarium == null)
                {
                    return UnprocessableEntity();
                }

                // Use the URL User Id
                if(aquarium.User == null) aquarium.User = new User();
                aquarium.User.Id = UserId;

                await LookupTypeAndUnits(aquarium);

                _logger.LogInformation("Creating new aquarium...");
                await _repository.Add(aquarium);
                _logger.LogInformation($"New aquarium created. AquariumID:{aquarium.Id}.");

                AddETag(aquarium.RowVersion);

                var url = Url.Link("AquariumGet", new { UserId, aquariumId = aquarium.Id });
                return Created(url, _mapper.Map<AquariumModel>(aquarium));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to create Aquarium.");
            }
            return BadRequest("Could not create Aquarium");
        }

        // PUT: api/aquarium/5
        [HttpPut("{aquariumId}")]
        public async Task<IActionResult> Put(int aquariumId, [FromBody]AquariumModel model)
        {
            try
            {
                var aquarium = await _repository.Get(UserId, aquariumId);
                if (aquarium == null) return NotFound();

                if (Request.Headers.ContainsKey(HeaderNames.IfMatch))
                {
                    var etag = Request.Headers[HeaderNames.IfMatch].First();
                    if (etag != Convert.ToBase64String(aquarium.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                _mapper.Map(model, aquarium);

                await LookupTypeAndUnits(aquarium);

                _logger.LogInformation($"Updating aquarium. AquariumID:{aquariumId}");
                await _repository.Update(aquarium);

                AddETag(aquarium.RowVersion);

                return Ok(_mapper.Map<AquariumModel>(aquarium));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to update Aquarium");
            }
            return BadRequest("Could not update Aquarium");
        }

        // DELETE: api/aquarium/5
        [HttpDelete("{aquariumId}")]
        public async Task<IActionResult> Delete(int aquariumId)
        {
            try
            {
                var aquarium = await _repository.Get(UserId, aquariumId);
                if (aquarium == null) return NotFound();

                if (Request.Headers.ContainsKey(HeaderNames.IfMatch))
                {
                    var etag = Request.Headers[HeaderNames.IfMatch].First();
                    if (etag != Convert.ToBase64String(aquarium.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                _logger.LogInformation($"Deleting Aquarium. AquariumId:{aquariumId}");
                await _repository.Delete(aquariumId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured whilst trying to delete Aquarium. AquariumId:{aquariumId}");
            }
            return BadRequest("Could not delete Aquarium");
        }

        private async Task LookupTypeAndUnits(Aquarium aquarium)
        {
            // Lookup units
            aquarium.DimensionUnit = await _unitManager.LookUpByName(aquarium.DimensionUnit);
            aquarium.VolumeUnit = await _unitManager.LookUpByName(aquarium.VolumeUnit);

            //Look Up Type
            aquarium.Type = _aquariumTypeManager.LookupFromName(aquarium.Type);
        }
    }
}