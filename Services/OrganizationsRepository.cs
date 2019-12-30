using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Memory;
using Hl7Fhir.R4.Api.Models;
using Hl7Fhir.R4.Api.Utilities;
using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Address;
using Hl7Fhir.R4.Api.Common;
using static Hl7.Fhir.Model.ContactPoint;
using Task = System.Threading.Tasks.Task;

namespace Hl7Fhir.R4.Api.Services
{
    public class OrganizationsRepository : IOrganizationsRepository
    {
        private readonly string _accessToken;
        private readonly FhirClient _client;

        public OrganizationsRepository(IMemoryCache memoryCache)
        {
            Access_Token accessToken = new Access_Token(memoryCache);
            _accessToken = accessToken.Token;
            _client = FHIRClientUtility.GetClientAsync(_accessToken);
        }

        public async Task<OrganizationCreationResponse> CreateAsync(OrganizationForCreation org)
        {
            var Result = new OrganizationCreationResponse { Data = new NewOrganization() };
            Organization organization = new Organization
            {
                Name = org.name,
                Address = GetFHIROrgAddresses(org),
                Telecom = GetFHIROrgTelecoms(org),
                Type = GetFHIROrgType(org)
            };

            Organization FHIROrganization = await _client.CreateAsync(organization).ConfigureAwait(false);
            Result.Data.OrganizationId = FHIROrganization.Id;//1ce02b5e-a9ae-4f4a-a2b9-cc0a8d4c987b

            return Result;
        }
        public async Task<IEnumerable<OrganizationModel>> GetOrganizationsAsync()
        {
            var organizations = new List<OrganizationModel>();
            var orgFHIRList = await GetOrganizationsFromFHIRAsync().ConfigureAwait(false);
            if (orgFHIRList != null && orgFHIRList.Count > 0)
            {
                foreach (var org in orgFHIRList)
                {
                    Coding coding = GetFhirCodingWithPROVcode(org);
                    OrganizationModel organizationModel = new OrganizationModel
                    {
                        Data = new OrganizationData
                        {
                            Active = org.Active,
                            Name = org.Name,
                            Type = new Models.Type
                            {
                                System = coding.System,
                                Code = coding.Code,
                                Display = coding.Display
                            },
                            Telecom = GetOrgTelecoms(org),
                            Address = GetOrgAddresses(org)
                        },
                    };
                    organizations.Add(organizationModel);
                }

            }

            return organizations;
        }

        public async Task<OrganizationModel> GetOrganizationAsync(Guid id)
        {
            var organizationModel = new OrganizationModel();
            var orgFHIR = await GetOrganizationFromFHIRAsync(id.ToString()).ConfigureAwait(false);
            if (orgFHIR != null)
            {
                Coding coding = GetFhirCodingWithPROVcode(orgFHIR);
                organizationModel = new OrganizationModel
                {
                    Data = new OrganizationData
                    {
                        Active = orgFHIR.Active,
                        Name = orgFHIR.Name,
                        Type = new Models.Type
                        {
                            System = coding.System,
                            Code = coding.Code,
                            Display = coding.Display
                        },
                        Telecom = GetOrgTelecoms(orgFHIR),
                        Address = GetOrgAddresses(orgFHIR)
                    },
                };
            }
            return organizationModel;
        }

        public async Task<OrganizationCreationResponse> UpdateAsync(Guid id, OrganizationForCreation org)
        {
            var Result = new OrganizationCreationResponse { Data = new NewOrganization() };

            Organization organization = await GetOrganizationFromFHIRAsync(id.ToString()).ConfigureAwait(false);

            organization.Name = org.name;
            organization.Address = GetFHIROrgAddresses(org);
            organization.Telecom = GetFHIROrgTelecoms(org);
            organization.Type = GetFHIROrgType(org);

            Organization FHIROrganization = await _client.UpdateAsync(organization).ConfigureAwait(false);

            Result.Data.OrganizationId = FHIROrganization.Id;//48fc2852-f0c3-4e94-af17-2bf57917eeb9

            return Result;
        }

        public async Task DeleteAsync(Guid id)
        {
            Organization organization = await GetOrganizationFromFHIRAsync(id.ToString()).ConfigureAwait(false);

            if (organization != null)
            {
                await _client.DeleteAsync(organization).ConfigureAwait(false);
            }
        }

        private static List<OrgAddress> GetOrgAddresses(Organization orgFHIR)
        {
            return (from address in orgFHIR.Address
                    let orgAddress = new OrgAddress
                    {
                        Use = address.Use.ToString(),
                        Type = address.Type.ToString(),
                        Line = address.Line,
                        City = address.City,
                        State = address.State,
                        PostalCode = address.PostalCode,
                        Country = address.Country
                    }
                    select orgAddress).ToList();
        }

        private static List<Address> GetFHIROrgAddresses(OrganizationForCreation org)
        {
            return (from OrgAddress in org.Address
                    let address = new Address
                    {
                        Use = OrgAddress.Use.ToEnum<AddressUse>(),
                        Type = OrgAddress.Type.ToEnum<AddressType>(),
                        Line = OrgAddress.Line,
                        City = OrgAddress.City,
                        State = OrgAddress.State,
                        PostalCode = OrgAddress.PostalCode,
                        Country = OrgAddress.Country
                    }
                    select address).ToList();
        }

        private static List<Telecom> GetOrgTelecoms(Organization orgFHIR)
        {
            return (from contactPoint in orgFHIR.Telecom
                    let telecom = new Telecom
                    {
                        System = contactPoint.System.ToString(),
                        Value = contactPoint.Value,
                    }
                    select telecom).ToList();
        }

        private static List<ContactPoint> GetFHIROrgTelecoms(OrganizationForCreation org)
        {
            if (org.Telecom != null)
            {
                return (from telecom in org.Telecom
                        let contactPoint = new ContactPoint
                        {
                            System = telecom.System.ToEnum<ContactPointSystem>(),
                            Value = telecom.Value,
                        }
                        select contactPoint).ToList();
            }
            else
            {
                return new List<ContactPoint>();
            }


        }

        private static Coding GetFhirCodingWithPROVcode(Organization orgFHIR)
        {
            List<Coding> codings = new List<Coding>();
            foreach (var type in orgFHIR.Type)
            {
                foreach (var coding in type.Coding)
                {
                    codings.Add(coding);
                }
            }
            return codings.Find(x => x.Code.Contains("prov"));
        }

        private static List<CodeableConcept> GetFHIROrgType(OrganizationForCreation org)
        {
            List<CodeableConcept> Result = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = org.type.System,
                            Code = org.type.Code,
                            Display = org.type.Display
                        },
                    },
                }
            };

            return Result;
        }

        #region FHIR Models
        private async Task<Organization> GetOrganizationFromFHIRAsync(string id)
        {
            var bundle = await _client.SearchByIdAsync<Organization>(id).ConfigureAwait(false);
            return ((bundle.Entry != null) && (bundle.Entry.Count > 0)) ? (Organization)bundle.Entry[0].Resource : null;

        }

        private async Task<List<Organization>> GetOrganizationsFromFHIRAsync()
        {
            List<Organization> organizations = new List<Organization>();
            var result = await _client.SearchAsync<Organization>().ConfigureAwait(false);
            while (result != null)
            {
                if (result.Entry != null)
                {

                    foreach (var e in result.Entry)
                    {
                        organizations.Add((Organization)e.Resource);
                    }
                }
                // get the next page of results
                result = _client.Continue(result);
            }
            return organizations;
        }

        #endregion

    }
}
