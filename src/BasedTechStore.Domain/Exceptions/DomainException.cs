using System.Net;

namespace BasedTechStore.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public abstract HttpStatusCode StatusCode { get; }
        protected DomainException(string message) : base(message) { }
    }

    public class UnauthorizedException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
        public UnauthorizedException(string message = "Unauthorized") : base(message) { }
    }

    public class NotFoundException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
        public NotFoundException(string entity, object id) : base($"Entity: {entity} with id: '{id}' not found") { }
        public NotFoundException(string message) : base(message) { } 
    }

    public class ValidationException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
        public IDictionary<string, string[]>? Errors { get; }
        public ValidationException(IDictionary<string,  string[]>? errors) : base("Validation failed") 
            { Errors = errors; }
        public ValidationException(string field, string error) : base("Validation failed")
        {
            Errors = new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            };
        }
    }

    public class ForbiddenException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
        public ForbiddenException(string message = "Access forbidden") : base(message) { }
    }

    public class ConflictException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
        public ConflictException(string message) : base(message) { }
    }

    public class BadRequestException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
        public BadRequestException(string message) : base(message) { }
    }

    public class ServiceUnavailableException : DomainException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.ServiceUnavailable;
        public ServiceUnavailableException(string message = "Service temporarily unavailable") : base(message) { }
    }
}
