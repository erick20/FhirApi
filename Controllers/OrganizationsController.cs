using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hl7Fhir.R4.Api.Services;
using Hl7Fhir.R4.Api.Models;
using Microsoft.AspNetCore.Http;

namespace Hl7Fhir.R4.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationsRepository _orgRepository;
        public OrganizationsController(IOrganizationsRepository organizationsRepository)
        {
            _orgRepository = organizationsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationModel>>> Get()
        {
            try
            {
                var results = await _orgRepository.GetOrganizationsAsync();
                return Ok(results);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<OrganizationModel>> Get(Guid id)
        {
            try
            {
                var results = await _orgRepository.GetOrganizationAsync(id);
                return Ok(results);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }


        [HttpPost]//
        public async Task<ActionResult<OrganizationCreationResponse>> Post(OrganizationForCreation model)
        {
            try
            {
                var result = await _orgRepository.CreateAsync(model);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Organization resource could not be retreived");
            }
        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<OrganizationCreationResponse>> Put(Guid Id , [FromBody] OrganizationForCreation model)
        {
            Single
            try
            {
                var result = await _orgRepository.UpdateAsync(Id, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Organization resource could not be retreived");
            }
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> Delete(Guid Id)
        {
            try
            {
                await _orgRepository.DeleteAsync(Id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Organization resource could not be retreived");
            }
        }
    }
}