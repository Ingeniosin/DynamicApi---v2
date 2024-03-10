using System.Net;

namespace DynamicApi.Exceptions;

public class InternalException : ApiException {
    
    public InternalException(string message) : base(message, HttpStatusCode.InternalServerError) {
    }

    

}