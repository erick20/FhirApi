using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Common
{
    public static class Methods
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}
