namespace Appointments.Domain.Entities;

public class Appointment
{
    public long AppointmentId { get; set; }

    public long PatientId { get; set; }

    public long DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public List<AppointmentService> Services { get; set; } = new();

    public Prescription? Prescription { get; set; }
}