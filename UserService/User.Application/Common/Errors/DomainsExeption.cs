using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.Common.Errors
{
   

    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }

    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string message = "Resource not found.") : base(message) { }
    }

    public sealed class ForbiddenException : DomainException
    {
        public ForbiddenException(string message = "Forbidden.") : base(message) { }
    }

    public sealed class ConflictException : DomainException
    {
        public ConflictException(string message = "Conflict.") : base(message) { }
    }

    public sealed class ValidationException : DomainException
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationException(string message, IDictionary<string, string[]> errors) : base(message) => Errors = errors;
        public ValidationException(IDictionary<string, string[]> errors) : this("Validation failed.", errors) { }
    }

}
