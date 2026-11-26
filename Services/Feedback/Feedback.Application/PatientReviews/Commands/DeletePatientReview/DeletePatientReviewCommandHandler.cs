using Feedback.Application.Common.Interfaces;
using Feedback.Domain.Interfaces;
using MongoDB.Bson;
using MediatR;

namespace Feedback.Application.PatientReviews.Commands.DeletePatientReview;

public sealed class DeletePatientReviewCommandHandler(
    IPatientReviewRepository repository)
    : IRequestHandler<DeletePatientReviewCommand, bool>
{
    public async Task<bool> Handle(
        DeletePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.Id);

        return await repository.DeleteAsync(
            id,
            request.ExpectedVersion,
            cancellationToken);
    }
}