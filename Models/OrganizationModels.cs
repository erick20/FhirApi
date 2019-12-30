using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Models
{
    #region GET: api/organizations 
    public class OrganizationModel
    {
        public OrganizationData Data { get; set; }
    }

    public class OrganizationData
    {
        public bool? Active { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public List<Telecom> Telecom { get; set; }
        public List<OrgAddress> Address { get; set; }
    }

    
    #endregion

    #region POST: api/organizations 
    public class OrganizationForCreation
    {
        public string name { get; set; }
        public Type type { get; set; }
        public List<Telecom> Telecom { get; set; }
        public List<OrgAddress> Address { get; set; }
    }

    public class OrganizationCreationResponse
    {
        public NewOrganization Data { get; set; }
    }
    public class NewOrganization
    {
        public string OrganizationId { get; set; }
    }

    #endregion

    #region Entities
    public class Type
    {
        public string System { get; set; }
        public string Code { get; set; }
        public string Display { get; set; }
    }

    public class Telecom
    {
        public string System { get; set; }
        public string Value { get; set; }
    }

    public class OrgAddress
    {
        public string Use { get; set; }
        public string Type { get; set; }
        public IEnumerable<string> Line { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
    #endregion
}
