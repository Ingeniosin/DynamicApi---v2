using System.Net;

namespace DynamicApi.Exceptions;

public class UnauthorizedException : ApiException {
    
    public UnauthorizedException(string message) : base(message, HttpStatusCode.Unauthorized) {
    }

    

}