namespace Appointments.Domain.Entities;

public class Patient
{
    public long PatientId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }
}