namespace Appointments.Bll.DTOs.Appointments;

public class AppointmentDto
{
    public long AppointmentId { get; set; }

    public long PatientId { get; set; }

    public long DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<AppointmentServiceDto> Services { get; set; } = new();
}