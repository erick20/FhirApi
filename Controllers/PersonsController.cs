using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hl7Fhir.R4.Api.Models;
using Hl7Fhir.R4.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hl7Fhir.R4.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        //Properties
        private readonly IPersonsRepository _personsRepository;

        //Constuctor
        public PersonsController(IPersonsRepository personsRepository)
        {
            _personsRepository = personsRepository;
        }

        //Actions

        public async Task<ActionResult<IEnumerable<PersonProfileResponse>>> Get()
        {
            IEnumerable<PersonProfileResponse> persons = new List<PersonProfileResponse>();
            try
            {

            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }


        /// <summary>
        /// Get Person
        /// </summary>
        /// <remarks>
        /// Get person profile
        /// </remarks>
        /// <param name="id">Id of the person</param>
        /// <response code="200"></response>
        /// <response code="206">No target with type value of Patient</response>
        /// <response code="400">Person profile could not be retrieved</response>
        /// <response code="500">Internal server error</response>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonProfileContainer), 200)]
        [ProducesResponseType(typeof(string), 206)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<ActionResult> Get([Required][FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    PersonProfileResponse response = await _personsRepository.GetPersonProfileAsync(id);
                    if(response.statusCode== HttpStatusCode.OK)
                    {
                        IEnumerable<PersonProfileContainer> personProfileContainers = new List<PersonProfileContainer> { new PersonProfileContainer { data = response.data } };
                        return Ok(personProfileContainers.First());
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.PartialContent, "No target with type value of Patient");
                    }
                }
                catch (Exception)
                {

                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
                }
                

                

            }//ends If ModelState.IsValid

            return BadRequest();
        }//ends Get


        

    }//ends PersonsController
}