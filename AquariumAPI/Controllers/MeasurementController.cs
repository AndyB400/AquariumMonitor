using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AquariumAPI.Filters;
using AquariumMonitor.APIModels;
using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.Net.Http.Headers;

namespace AquariumAPI.Controllers
{
    [AquariumSecurityCheck]
    [EnableCors("AquariumMonitor")]
    [Produces("application/json")]
    [Route("api/aquariums/{aquariumId}/measurements")]
    [ValidateModel]
    public class MeasurementController : BaseController
    {
        private readonly IMeasurementRepository _repository;
        private readonly IAquariumRepository _aquariumRepository;
        private readonly IValidationManager _validationManager;
        private readonly IUnitManager _unitManager;
        private readonly IMeasurementManager _measurementManager;

        public MeasurementController(IMeasurementRepository repository, 
            ILogger logger,  IMapper mapper,
            IAquariumRepository aquariumRepository, IValidationManager validationManager, 
            IUnitManager unitManager,
            IMeasurementManager measurementManager) : base(logger, mapper)
        {
            _repository = repository;
            _aquariumRepository = aquariumRepository;
            _validationManager = validationManager;
            _unitManager = unitManager;
            _measurementManager = measurementManager;
        }

        [HttpGet("{id}", Name = "MeasurementGet")]
        public async Task<IActionResult> Get(int aquariumId, int measurementId)
        {
            var measurement = await _repository.Get(measurementId);

            if (measurement == null) return NotFound();

            AddETag(measurement.RowVersion);

            return Ok(_mapper.Map<MeasurementModel>(measurement));
        }

        [HttpGet]
        public async Task<IActionResult> Get(int aquariumId)
        {
            var measurements = await _repository.GetForAquarium(UserId, aquariumId);

            var models = _mapper.Map<List<MeasurementModel>>(measurements);

            return Ok(models);
        }

        [HttpPost]
        public async Task<IActionResult> Post(int aquariumId, [FromBody]MeasurementModel model)
        {
            try
            {
                var measurement = _mapper.Map<Measurement>(model);

                if (measurement == null)
                {
                    return UnprocessableEntity();
                }

                // User URL values over model values
                measurement.UserId = UserId;
                measurement.AquariumId = aquariumId;

                // Measurement Type
                measurement.MeasurementType = _measurementManager.LookupFromName(measurement.MeasurementType);

                var results = _validationManager.Validate(measurement);

                if(results.Count != 0)
                {
                    return UnprocessableEntity(results);
                }

                // Lookup unit
                measurement.Unit = await _unitManager.LookUpByName(measurement.Unit);

                await _repository.Add(measurement);

                AddETag(measurement.RowVersion);

                var url = Url.Link("MeasurementGet", new { UserId, aquariumId, ((Measurement)measurement).Id });
                return Created(url, _mapper.Map<MeasurementModel>(measurement));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occured whilst trying to create Measurement.");
            }
            return BadRequest("Could not create Measurement");
        }

        // PUT api/Measurement/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int aquariumId, int measurementId, [FromBody]MeasurementModel model)
        {
            try
            {
                var measurement = await _repository.Get(measurementId);
                if (measurement == null) return NotFound();
                if (measurement.AquariumId != aquariumId) return BadRequest("Aquarium and Measurement don't match");

                _mapper.Map(model, measurement);

                // Validate
                var results = _validationManager.Validate(measurement);

                if (results.Count != 0)
                {
                    return UnprocessableEntity(results);
                }

                // Lookup unit
                measurement.Unit = await _unitManager.LookUpByName(measurement.Unit);

                await _repository.Update(measurement);

                AddETag(measurement.RowVersion);
                return Ok(_mapper.Map<MeasurementModel>(measurement));
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "An error occured whilst trying to update Measurement.");
            }
            return BadRequest("Could not update Measurment");
        }

        // DELETE api/Measurement/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int aquariumId, int measurementId)
        {
            try
            {
                var measurement = await _repository.Get(measurementId);
                if (measurement == null) return NotFound();
                if (measurement.UserId != UserId) return BadRequest("User and Measurement don't match");
                if (measurement.AquariumId != aquariumId) return BadRequest("Aquarium and Measurement don't match");

                await _repository.Delete(measurement.Id);
      
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occured whilst trying to delete Measurement.");
            }
            return BadRequest("Could not delete Measurment");
        }
    }
}
