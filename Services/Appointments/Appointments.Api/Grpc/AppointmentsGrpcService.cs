
using System.Globalization;
using Appointments.Api.Grpc.Contracts;
using Appointments.Bll.Interfaces;
using Appointments.Domain.Exceptions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Appointments.Api.Grpc;

public sealed class AppointmentsGrpcService(
    IAppointmentService appointmentService)
    : AppointmentsGrpc.AppointmentsGrpcBase
{
    public override async Task<AppointmentResponse> GetAppointmentById(
        GetAppointmentByIdRequest request,
        ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Appointment ID must be greater than zero."));
        }

        try
        {
            var appointment = await appointmentService.GetByIdAsync(
                request.Id,
                context.CancellationToken);

            return MapToResponse(appointment);
        }
        catch (NotFoundException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    exception.Message));
        }
        catch (DomainException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    exception.Message));
        }
        catch (ArgumentException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    exception.Message));
        }
    }

    private static AppointmentResponse MapToResponse(
        Appointments.Bll.DTOs.Appointments.AppointmentDto appointment)
    {
        var response = new AppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = ToTimestamp(appointment.AppointmentDate),
            Status = appointment.Status,
            TotalAmount = appointment.TotalAmount.ToString(
                CultureInfo.InvariantCulture),
            CreatedAt = ToTimestamp(appointment.CreatedAt)
        };

        response.Services.AddRange(
            appointment.Services.Select(service =>
                new AppointmentServiceResponse
                {
                    AppointmentServiceId =
                        service.AppointmentServiceId,

                    ServiceId = service.ServiceId,

                    ServiceNameSnapshot =
                        service.ServiceNameSnapshot,

                    PriceSnapshot = service.PriceSnapshot.ToString(
                        CultureInfo.InvariantCulture),

                    Quantity = service.Quantity
                }));

        return response;
    }

    private static Timestamp ToTimestamp(DateTime value)
    {
        // SQL Server DateTime values may have Unspecified Kind.
        // Project timestamps are treated as UTC.
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc)
        };

        return Timestamp.FromDateTime(utcValue);
    }
}
