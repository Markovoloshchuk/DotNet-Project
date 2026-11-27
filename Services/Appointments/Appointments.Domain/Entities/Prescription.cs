namespace Appointments.Domain.Entities;

public class Prescription
{
    public long PrescriptionId { get; set; }

    public long AppointmentId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string? MedicinesRecommendations { get; set; }

    public DateTime IssuedAt { get; set; }
}