using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7Fhir.R4.Api.Models;

namespace Hl7Fhir.R4.Api.Services
{
    public interface IOrganizationsRepository
    {
        Task<OrganizationModel> GetOrganizationAsync(Guid id);
        Task<IEnumerable<OrganizationModel>> GetOrganizationsAsync();
        Task<OrganizationCreationResponse> CreateAsync(OrganizationForCreation organization);
        Task<OrganizationCreationResponse> UpdateAsync(Guid id, OrganizationForCreation org);
        Task DeleteAsync(Guid id);
    }
}
