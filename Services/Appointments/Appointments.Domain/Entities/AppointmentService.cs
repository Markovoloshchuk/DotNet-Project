namespace Appointments.Domain.Entities;

public class AppointmentService
{
    public long AppointmentServiceId { get; set; }

    public long AppointmentId { get; set; }

    public long ServiceId { get; set; }

    public string ServiceNameSnapshot { get; set; } = string.Empty;

    public decimal PriceSnapshot { get; set; }

    public int Quantity { get; set; }
}