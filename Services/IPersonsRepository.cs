using Hl7Fhir.R4.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Services
{
    public interface IPersonsRepository
    {
        Task<PersonProfileResponse> GetPersonProfileAsync(string person_id);

        Task<IEnumerable<PersonProfileResponse>> GetPersonsAsync();
    }
}
