using System.ComponentModel.DataAnnotations;

namespace Appointments.Bll.DTOs.Appointments;

public class CreateAppointmentServiceDto
{
    [Range(1, long.MaxValue)]
    public long ServiceId { get; set; }

    [Required]
    [StringLength(150)]
    public string ServiceNameSnapshot { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal PriceSnapshot { get; set; }

    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;
}