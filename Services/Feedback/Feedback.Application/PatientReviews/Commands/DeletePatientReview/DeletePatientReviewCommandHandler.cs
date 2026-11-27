using Feedback.Domain.Interfaces;
using MediatR;

namespace Feedback.Application.PatientReviews.Commands.DeletePatientReview;

public sealed class DeletePatientReviewCommandHandler(
    IPatientReviewRepository repository)
    : IRequestHandler<DeletePatientReviewCommand, bool>
{
    public Task<bool> Handle(
        DeletePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(
            request.Id,
            request.ExpectedVersion,
            cancellationToken);
    }
}