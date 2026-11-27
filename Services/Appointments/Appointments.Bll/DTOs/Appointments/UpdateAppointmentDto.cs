using System.ComponentModel.DataAnnotations;

namespace Appointments.Bll.DTOs.Appointments;

public class UpdateAppointmentDto
{
    [Range(1, long.MaxValue)]
    public long PatientId { get; set; }

    [Range(1, long.MaxValue)]
    public long DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }
}