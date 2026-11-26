using Feedback.Domain.Entities;
using Feedback.Domain.Exceptions;

namespace Feedback.Tests;

public class PatientReviewTests
{
    [Fact]
    public void Constructor_WhenRatingIsTen_ThrowsDomainException()
    {
        var action = () => new PatientReview(
            reviewId: 1,
            patientId: 1,
            doctorId: 1,
            rating: 10,
            comment: "Test review");

        Assert.Throws<DomainException>(action);
    }
}