using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Models
{
    //ViewModel for Post Person API
    public class PersonViewModel
    {
        /// <summary>
        /// Id of the person to be created
        /// </summary>
        /// <example>testperson@testing.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// Id of the created person
        /// </summary>
        /// <example>f721113a-041d-4c49-89c8-7d9140660d9a</example>
        public string person_id { get; set; }
    }

    public class SuccessPersonResponse
    {
        public Data data { get; set; }
    }



    //Models related to GET Person API
    public class PersonProfileResponse
    {
        public HttpStatusCode statusCode { get; set; }

        public PersonProfile data { get; set; }
    }

    //Response model of GET Person API
    public class PersonProfileContainer
    {
        public PersonProfile data { get; set; }
    }

    public class PersonProfile
    {
        /// <summary>
        /// Id of the person
        /// </summary>
        /// <example>f721113a-041d-4c49-89c8-7d9140660d9j</example>
        public string personId { get; set; }

        /// <summary>
        /// Id of the associated patient
        /// </summary>
        /// <example>8f2b128d-05fc-4906-a058-76ae7bb05268</example>
        public string patientSelfId { get; set; }

        public List<ExternalId> externalIds { get; set; }

        public List<Related_Person> relatedPersons { get; set; }

        public List<FutureAppointment> appointments { get; set; }

        public List<_Provider> providers { get; set; }
    }

    public class FutureAppointment
    {
        /// <summary>
        /// Id of the future appointment
        /// </summary>
        /// <example>8f2b128d-05fc-4906-a058-76ae7bb05267</example>
        public string appointmentId { get; set; }

        /// <summary>
        /// start date of the future appointment (i.e When appointment is to take place)
        /// </summary>
        /// <example>2020-01-01T08:30:00-07:00</example>
        public string start { get; set; }

        /// <summary>
        /// Status of the future appointment
        /// </summary>
        /// <example>booked</example>
        public string status { get; set; }

        /// <summary>
        /// Id of the practitioner role involved in the future appointment
        /// </summary>
        /// <example>f272b604-80f7-44e7-a994-e96210fb7205</example>
        public string provider { get; set; }
    }

    public class Related_Person
    {
        /// <summary>
        /// Id of the related patient
        /// </summary>
        /// <example>8f2b128d-05fc-4906-a058-76ae7bb05260</example>
        public string patientId { get; set; }

        /// <summary>
        /// Name of the related patient
        /// </summary>
        /// <example>Tome Smith</example>
        public string nameText { get; set; }

        /// <summary>
        /// Relationship between person and related person
        /// </summary>
        /// <example>son</example>
        public string relationship { get; set; }
    }

    public class _Provider
    {
        /// <summary>
        /// Id of the practitioner associated with patient (through practitioner role)
        /// </summary>
        /// <example>db9c5f28-59c3-49cc-91af-0f62e9b4de8c</example>
        public string practitionerId { get; set; }

        /// <summary>
        /// Id of the practitioner role associated with patient
        /// </summary>
        /// <example>f272b604-80f7-44e7-a994-e96210fb7201</example>
        public string practitionerRoleId { get; set; }

        /// <summary>
        /// Name of the practitioner
        /// </summary>
        /// <example>Jones Henry</example>
        public string nameText { get; set; }

        /// <summary>
        /// Phone number associated with practitioner role
        /// </summary>
        /// <example>888- 888- 9999/example>
        public string phone { get; set; }
    }

    public class ExternalId
    {
        public string externalId { get; set; }
        public string assigner { get; set; }
    }


    public class PatientResponse
    {
        public HttpStatusCode statusCode { get; set; }
        public Patient patient { get; set; }
    }
}
