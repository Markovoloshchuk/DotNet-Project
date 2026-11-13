namespace Appointments.Bll.DTOs.Appointments;

public class AppointmentServiceDto
{
    public long AppointmentServiceId { get; set; }

    public long ServiceId { get; set; }

    public string ServiceNameSnapshot { get; set; } = string.Empty;

    public decimal PriceSnapshot { get; set; }

    public int Quantity { get; set; }
}