using System.Net;

namespace DynamicApi.Exceptions;

public class BadRequestException : ApiException {
    
    public BadRequestException(string message) : base(message, HttpStatusCode.BadRequest) {
    }

    

}