namespace Appointments.Domain.Exceptions;

public class BusinessConflictException : DomainException
{
    public BusinessConflictException(string message) : base(message)
    {
    }
}   