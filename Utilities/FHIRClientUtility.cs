using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Utilities
{
    public class FHIRClientUtility
    {
        private static Uri _fhirServerUrl = new Uri("https://panoramic-dev.azurehealthcareapis.com/");
        public static FhirClient GetClientAsync(string accessToken)
        {
            var client = new Hl7.Fhir.Rest.FhirClient(_fhirServerUrl);
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            };
            client.PreferredFormat = ResourceFormat.Json;
            return client;
        }
    }
}
