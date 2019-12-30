using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hl7Fhir.R4.Api.Models
{
    public class Error
    {
        /// <summary>
        /// HTTP Status Code
        /// </summary>
        /// <example>400</example>
        public int code { get; set; }

        /// <summary>
        /// Error Describing Message
        /// </summary>
        /// <example>Person resource could not be created.</example>
        public string message { get; set; }
    }

    public class ErrorResponse
    {
        public Error error { get; set; }
        public static ErrorResponse ErrorResponse500 => new ErrorResponse { error = new Error { code = 500, message = "Internal Error" } };
        public static ErrorResponse ErrorResponseCustom(int code, string msg) => new ErrorResponse { error = new Error { code = code, message = msg } };

    }

}
