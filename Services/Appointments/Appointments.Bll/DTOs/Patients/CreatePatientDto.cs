using System.ComponentModel.DataAnnotations;

namespace Appointments.Bll.DTOs.Patients;

public class CreatePatientDto
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }
}