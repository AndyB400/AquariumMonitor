using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AquariumMonitor.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AquariumMonitor.Models;
using AquariumMonitor.API.Filters;
using AquariumMonitor.Models.ViewModels;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Cors;

namespace AquariumMonitor.API.Controllers
{
    [AquariumSecurityCheck]
    [EnableCors("AquariumMonitor")]
    [Produces("application/json")]
    [Route("api/aquariums/{aquariumId}/waterchanges")]
    [ValidateModel]
    public class WaterChangeController : BaseController
    {
        private readonly IWaterChangeRepository _repository;

        public WaterChangeController(IWaterChangeRepository repository, 
            ILoggerAdapter<BaseController> logger, 
            IMapper mapper) : base(logger, mapper)
        {
            _repository = repository;
        }

        // GET: api/WaterChange
        [HttpGet]
        public async Task<IActionResult> Get(int aquariumId)
        {
            var waterChanges = await _repository.GetForAquarium(UserId, aquariumId);
            return Ok(Mapper.Map<IEnumerable<WaterChangeModel>>(waterChanges));
        }

        // GET: api/WaterChange/5
        [HttpGet("{waterChangeId}", Name = "WaterChangeGet")]
        public async Task<IActionResult> Get(int aquariumId, int waterChangeId)
        {
            var waterChange = await _repository.Get(waterChangeId);

            if (waterChange == null) return NotFound();

            AddETag(waterChange.RowVersion);

            return Ok(Mapper.Map<WaterChangeModel>(waterChange));
        }

        // POST: api/WaterChange
        [HttpPost]
        public async Task<IActionResult> Post(int aquariumId, [FromBody]WaterChangeModel model)
        {
            try
            {
                var waterChange = Mapper.Map<WaterChange>(model);

                // User URL values over model values
                waterChange.UserId = UserId;
                waterChange.AquariumId = aquariumId;

                if (waterChange == null)
                {
                    return UnprocessableEntity();
                }

                await _repository.Add(waterChange);

                AddETag(waterChange.RowVersion);

                var url = Url.Link("WaterChangeGet", new { UserId, aquariumId, waterChangeId = waterChange.Id });
                return Created(url, Mapper.Map<WaterChangeModel>(waterChange));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured whilst trying to create waterChange.");
            }
            return BadRequest("Could not create waterChange");
        }
        
        // PUT: api/WaterChange/5
        [HttpPut("{waterChangeId}")]
        public async Task<IActionResult> Put(int aquariumId, int waterChangeId, [FromBody]WaterChangeModel model)
        {
            try
            {
                var waterChange = await _repository.Get(waterChangeId);
                if (waterChange == null) return NotFound();
                if (waterChange.AquariumId != aquariumId) return BadRequest("Aquarium and WaterChange don't match");

                Mapper.Map(model, waterChange);
                await _repository.Update(waterChange);

                AddETag(waterChange.RowVersion);

                return Ok(Mapper.Map <WaterChangeModel>(waterChange));
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "An error occured whilst trying to update WaterChange.");
            }
            return BadRequest("Could not update WaterChange");
        }

        // DELETE: api/WaterChange/5
        [HttpDelete("{waterChangeId}")]
        public async Task<IActionResult> Delete(int aquariumId, int waterChangeId)
        {
            try
            {
                var waterChange = await _repository.Get(waterChangeId);
                if (waterChange == null) return NotFound();
                if (waterChange.UserId != UserId) return BadRequest("User and Waterchange don't match");
                if (waterChange.AquariumId != aquariumId) return BadRequest("Aquarium and WaterChange don't match");

                await _repository.Delete(waterChange.Id);

                return Ok();

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured whilst trying to delete WaterChange.");
            }
            return BadRequest("Could not delete WaterChange");
        }
    }
}
