using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Hl7Fhir.R4.Api.Utilities
{
    public class Access_Token
    {
        //Properties
        public string Token => GetAccessToken().Result;

        private string authority = "https://login.microsoftonline.com/894e47d6-5fd9-4351-b8a4-f3a178380c66";
        private string audience = "https://azurehealthcareapis.com";
        private string clientId = "d65826a4-a0d5-44d3-8f88-1e6660715fe9";
        private string clientSecret = "+^;>*+{;});Y:@}#;0/}D_//W$([*dkl?-#({;_^%";

        private AuthenticationContext authContext;
        private ClientCredential clientCredential;

        private IMemoryCache memoryCache;

        //Constructor
        public Access_Token(IMemoryCache memoryCache)
        {
            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, clientSecret);
            this.memoryCache = memoryCache;
        }

        private async Task<string> GetAccessToken()
        {
            string access_Token;

            if (!memoryCache.TryGetValue<string>("access_Token", out access_Token))
            {
                AuthenticationResult result = await authContext.AcquireTokenAsync(audience, clientCredential);
                access_Token = result.AccessToken;

                memoryCache.Set<string>("access_Token", access_Token, result.ExpiresOn);
            }
            return access_Token;
        }//ends GetAccessToken

    }
}
