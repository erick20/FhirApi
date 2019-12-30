using Hl7.Fhir.Rest;
using Hl7Fhir.R4.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Services
{
    public abstract class BaseRepository
    {
        //Properties
        protected Access_Token _accessToken;
        protected FhirClient _client;
        protected Uri baseUri = new Uri("https://panoramic-dev.azurehealthcareapis.com/");

        //Contstructor
        public BaseRepository(Access_Token accessToken)
        {
            _accessToken = accessToken;
        }

        //Methods
        protected FhirClient GetClient(Uri uri)
        {
            _client = null;//Clearing the _client object
            var client = new FhirClient(uri);
            client.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
            {
                e.RawRequest.Headers.Add("Authorization", $"Bearer {_accessToken.Token}");
            };
            client.PreferredFormat = ResourceFormat.Json;
            return client;
        }

    }
}
