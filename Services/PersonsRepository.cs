using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7Fhir.R4.Api.Models;
using Hl7Fhir.R4.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Hl7.Fhir.Model.Appointment;

namespace Hl7Fhir.R4.Api.Services
{
    public class PersonsRepository : BaseRepository, IPersonsRepository
    {
        public PersonsRepository(Access_Token accessToken) : base(accessToken) { }

     
        public async Task<PersonProfileResponse> GetPersonProfileAsync(string person_id)
        {
            PersonProfileResponse response = new PersonProfileResponse();

            //PHASE 1
            Person person = await GetPersonWithConfirmedIdentity(person_id);
            if (person == null)
            {
                response.statusCode = HttpStatusCode.BadRequest;
                return response;
            }

            response.statusCode = HttpStatusCode.Accepted; //The request has been accepted for processing, but the processing has not been completed

            //PHASE 2

            List<string> patientReferences = GetReferences(person, "Patient");

            if (patientReferences.Count == 0)
            {
                response.statusCode = HttpStatusCode.PartialContent;
                return response;
            }
            string patientReference = patientReferences[0];
            // response.statusCode = HttpStatusCode.Accepted; //The request has been accepted for processing, but the processing has not been completed

            //PHASE 3
            //Since there is always only one patient associated with a person, So in patientReferences array at index 0, we have that one patientReference

            List<Appointment> appointments = await GetInFutureAppointmentsForPatientAsync(patientReference);

            if (appointments == null)//appointments will be null only when ResponseCode is other than 200
            {
                response.statusCode = HttpStatusCode.InternalServerError;//500
                return response;
            }

            // response.statusCode = HttpStatusCode.Accepted; //The request has been accepted for processing, but the processing has not been completed

            List<FutureAppointment> futureAppointments = GetFutureAppointmentsFromAppointmentsWhereActorIsPractitionerRole(appointments);

            //PHASE 4

            List<Related_Person> related_People = await GetRelatedPeopleAsync(person);

            //PHASE 5
            PatientResponse getPatientResponse = await GetPatientAsync(patientReference);

            if (getPatientResponse.statusCode != HttpStatusCode.OK)
            {
                response.statusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            // response.statusCode = HttpStatusCode.Accepted; //The request has been accepted for processing, but the processing has not been completed

            List<string> practitionerRoleReferences = GetPractitionerRoleReferencesFromPatient(getPatientResponse.patient);

            List<_Provider> providers = null;

            if (practitionerRoleReferences.Count > 0)
            {
                providers = await GetPractitionerRolesAsync(practitionerRoleReferences);
            }

            //PHASE 5a
            //Extraction externalIds from the retrieved patient resource
            List<ExternalId> externalIds = GetExternalIdsFromPatient(getPatientResponse.patient);

            //PHASE 6
            //Collecting data into a single object
            response.data = new PersonProfile
            {
                personId = person_id,
                patientSelfId = getPatientResponse.patient.Id,
                externalIds = (externalIds?.Count > 0 ? externalIds : null),
                relatedPersons = (related_People?.Count > 0 ? related_People : null),
                appointments = (futureAppointments?.Count > 0 ? futureAppointments : null),
                providers = (providers?.Count > 0 ? providers : null)
            };

            //Setting Response Code
            response.statusCode = HttpStatusCode.OK;

            return response;
        }//ends GetPersonProfileAsync

       

        private async Task<Person> GetPersonWithConfirmedIdentity(string person_id)
        {
            _client = GetClient(baseUri);

            HttpStatusCode responseStatusCode = default(HttpStatusCode);

            _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
            {
                responseStatusCode = e.RawResponse.StatusCode;
            };

            Bundle bundle = await _client.SearchByIdAsync<Person>(person_id);

            if (responseStatusCode == HttpStatusCode.OK)
            {
                if (bundle?.Entry?.Count == 1)
                {
                    return (Person)bundle.Entry[0].Resource;
                }
            }

            return null;
        }

        private List<string> GetReferences(Person person, string referenceType)
        {
            List<string> referenceValues = new List<string>();

            foreach (var linkComponent in person.Link)
            {
                if (linkComponent?.Target.Type == referenceType)
                {
                    referenceValues.Add(linkComponent.Target.Reference);
                }
            }
            return referenceValues;
        }

        private async Task<List<Appointment>> GetInFutureAppointmentsForPatientAsync(string patientReference)
        {
            HttpStatusCode responseStatusCode = default(HttpStatusCode);

            List<Appointment> appointments = new List<Appointment>();

            _client = GetClient(baseUri);

            _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
            {
                responseStatusCode = e.RawResponse.StatusCode;

            };

            //https://panoramic-dev.azurehealthcareapis.com/Appointment?patient=Patient/patient_id&date=gtcurrent_date

            Bundle bundle = await _client.SearchAsync<Appointment>(new SearchParams().Where($"patient={patientReference}").Where($"date=gt{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz")}"));

            if (responseStatusCode == HttpStatusCode.OK)
            {
                if (bundle.Entry != null)
                {
                    foreach (var entryComponent in bundle.Entry)
                    {
                        appointments.Add((Appointment)entryComponent.Resource);
                    }
                }
                return appointments;
            }

            //will return null only if responseStatusCode is other than 200
            return null;
        }//ends GetInFutureAppointmentsForPatientAsync

        private List<FutureAppointment> GetFutureAppointmentsFromAppointmentsWhereActorIsPractitionerRole(List<Appointment> appointments)
        {
            //Local Function
            string GetStatusInStringWithFirstLetterSmall(AppointmentStatus? status)
            {
                if (status == null)
                {
                    return null;
                }//else
                string stringStatus = Enum.GetName(typeof(AppointmentStatus), status);
                stringStatus = char.ToLower(stringStatus[0]) + stringStatus.Substring(1);
                return stringStatus;
            }

            List<FutureAppointment> futureAppointments = new List<FutureAppointment>();

            foreach (Appointment appointment in appointments)
            {
                foreach (ParticipantComponent participantComponent in appointment.Participant)
                {
                    if (participantComponent.Actor.Type == "PractitionerRole")
                    {
                        futureAppointments.Add(new FutureAppointment
                        {
                            appointmentId = Enum.GetName(typeof(ResourceType), appointment.ResourceType) + "/" + appointment.Id,
                            status = GetStatusInStringWithFirstLetterSmall(appointment.Status),
                            start = appointment.Start.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                            provider = participantComponent.Actor.Reference
                        });
                    }//ends if-condition

                }//ends inner foreach

            }//ends outer foreach
            return futureAppointments;
        }//ends GetFutureAppointmentsFromAppointmentsWhereActorIsPractitionerRole

        private async Task<List<Related_Person>> GetRelatedPeopleAsync(Person person)
        {
            //Local Function
            string MapRelationship(AdministrativeGender? gender)
            {
                switch (gender)
                {
                    case AdministrativeGender.Male:
                        return "son";
                    case AdministrativeGender.Female:
                        return "daughter";
                    default://Unknown, Other, null will default to "child"
                        return "child";
                }
            }

            List<string> relatedPersonReferences = GetReferences(person, "RelatedPerson");

            List<Related_Person> related_People;

            if (relatedPersonReferences?.Count > 0)
            {
                related_People = new List<Related_Person>();
                int counter = 0;

                HttpStatusCode responseStatusCode = default(HttpStatusCode);

                _client = GetClient(baseUri);

                _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
                {
                    responseStatusCode = e.RawResponse.StatusCode;
                };

                RelatedPerson relatedPerson;
                Patient patient;

                do
                {
                    //GET call to fetch RelatedPerson
                    relatedPerson = await _client.ReadAsync<RelatedPerson>(relatedPersonReferences[counter]);

                    //We are using same responseStatusCode variable to check response codes f both requests
                    if (responseStatusCode == HttpStatusCode.OK)
                    {
                        //GET call to fetch patient
                        patient = await _client.ReadAsync<Patient>(relatedPerson.Patient.Reference);

                        if (responseStatusCode == HttpStatusCode.OK)
                        {
                            related_People.Add(new Related_Person
                            {//Object Mapping
                                patientId = relatedPerson.Patient.Reference,
                                nameText = patient.Name[0].Text,
                                relationship = MapRelationship(patient.Gender)//Relationship mapping
                            });
                        }//ends inner if-condition

                    }//ends outer if-condition

                    counter++;
                } while (counter < relatedPersonReferences.Count);

                return related_People;

            }//ends if-condition
            return null;
        }//ends GetRelatedPeopleAsync

        private async Task<PatientResponse> GetPatientAsync(string patientReference)
        {
            PatientResponse response = new PatientResponse();

            _client = GetClient(baseUri);

            //Response Processing Logic
            _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
            {
                response.statusCode = e.RawResponse.StatusCode;
            };

            try
            {
                response.patient = await _client.ReadAsync<Patient>(location: patientReference);
            }
            catch (Exception)
            {
            }
            return response;
        }//ends GetPatientAsync

        private List<string> GetPractitionerRoleReferencesFromPatient(Patient patient)
        {
            List<string> practitionerRoleReferences = new List<string>();
            if (patient.GeneralPractitioner != null)
            {
                foreach (var resourceReference in patient.GeneralPractitioner)
                {
                    if (resourceReference.Type == "PractitionerRole")
                    {
                        practitionerRoleReferences.Add(resourceReference.Reference);
                    }
                }
            }
            return practitionerRoleReferences;
        }//ends GetPractitionerRoleReferencesFromPatient

        private async Task<List<_Provider>> GetPractitionerRolesAsync(List<string> practitionerRoleReferences)
        {
            //Local Function
            string ExtractPhoneNumber(List<ContactPoint> telecom)
            {
                foreach (var contactPoint in telecom)
                {
                    if (contactPoint.System == ContactPoint.ContactPointSystem.Phone)
                    {
                        return contactPoint.Value;
                    }
                }
                return null;
            }

            HttpStatusCode responseStatusCode = default(HttpStatusCode);

            List<_Provider> providers = new List<_Provider>();

            //Getting client ready
            _client = GetClient(baseUri);

            _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
            {
                responseStatusCode = e.RawResponse.StatusCode;
            };

            int counter = 0;

            PractitionerRole practitionerRole;
            Practitioner practitioner;

            do
            {//counter-controlled loop

                //GET call to fetch PractitionerRole
                practitionerRole = await _client.ReadAsync<PractitionerRole>(practitionerRoleReferences[counter]);

                if (responseStatusCode == HttpStatusCode.OK)
                {
                    //GET call to fetch Practitioner
                    practitioner = await _client.ReadAsync<Practitioner>(practitionerRole.Practitioner.Reference);

                    if (responseStatusCode == HttpStatusCode.OK)
                    {//Mapping object
                        providers.Add(new _Provider
                        {
                            practitionerId = Enum.GetName(typeof(ResourceType), practitioner.ResourceType) + "/" + practitioner.Id,
                            practitionerRoleId = Enum.GetName(typeof(ResourceType), practitionerRole.ResourceType) + "/" + practitionerRole.Id,
                            nameText = practitioner.Name[0].Text,
                            phone = ExtractPhoneNumber(practitionerRole.Telecom)//extracting phone number
                        });
                    }//ends inner if-condition
                }//ends outer if-condition

                counter++;
            } while (counter < practitionerRoleReferences.Count);
            return providers;
        }//ends GetPractitionerRolesAsync

        private List<ExternalId> GetExternalIdsFromPatient(Patient patient)
        {
            if (patient.Identifier != null)
            {
                List<ExternalId> externalIds = new List<ExternalId>();

                foreach (Identifier identifier in patient.Identifier)
                {
                    if (identifier.Use == Identifier.IdentifierUse.Secondary && identifier.Type.Text == "MRN")
                    {
                        externalIds.Add(new ExternalId
                        {
                            externalId = identifier.Value,
                            assigner = identifier.Assigner.Reference
                        });
                    }

                }
                return externalIds;
            }
            return null;
        }//ends GetExternalIdsFromPatient

        public Task<IEnumerable<PersonProfileResponse>> GetPersonsAsync()
        {
            var personProfiles = new List<PersonProfileResponse>();

            return personProfiles;
        }

        private async Task<IEnumerable<Person>> GetPersonWithConfirmedIdentity()
        {
            _client = GetClient(baseUri);

            HttpStatusCode responseStatusCode = default(HttpStatusCode);

            _client.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
            {
                responseStatusCode = e.RawResponse.StatusCode;
            };

            Bundle bundle = await _client.SearchAsync<Person>();
            List<Person> persons = new List<Person>();

            if (responseStatusCode == HttpStatusCode.OK)
            {
                if (bundle?.Entry?.Count > 0)
                {
                    foreach (var e in bundle.Entry)
                    {
                        persons.Add((Person)e.Resource);
                    }
                    
                    return persons;
                }
            }

            return null;
        }
    }
}
