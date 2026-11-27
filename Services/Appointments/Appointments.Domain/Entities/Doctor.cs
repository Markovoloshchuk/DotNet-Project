namespace Appointments.Domain.Entities;

public class Doctor
{
    public long DoctorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string CabinetNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}