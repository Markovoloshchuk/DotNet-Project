using Feedback.Domain.Entities;
using Feedback.Domain.Exceptions;

namespace Feedback.Tests;

public sealed class DoctorDiscussionTests
{
    [Fact]
    public void AddComment_WhenCommentIdAlreadyExists_ThrowsDomainException()
    {
        var discussion = new DoctorDiscussion(
            discussionId: 101,
            title: "Test discussion",
            authorDoctorId: 10,
            content: "Discussion content");

        discussion.AddComment(1, 10, "First comment");

        var action = () => discussion.AddComment(
            1,
            11,
            "Duplicate comment");

        var exception = Assert.Throws<DomainException>(action);

        Assert.Equal(
            "Comment with this ID already exists.",
            exception.Message);
    }

    [Fact]
    public void AddTag_WhenTagAlreadyExistsIgnoringCase_DoesNotAddDuplicate()
    {
        var discussion = new DoctorDiscussion(
            discussionId: 102,
            title: "Test discussion",
            authorDoctorId: 10,
            content: "Discussion content",
            tags: ["Cardiology"]);

        discussion.AddTag("cardiology");

        Assert.Single(discussion.Tags);
        Assert.Equal("Cardiology", Assert.Single(discussion.Tags));
    }

    [Fact]
    public void UpdateContent_TrimsTitleAndContent()
    {
        var discussion = new DoctorDiscussion(
            discussionId: 103,
            title: "Old title",
            authorDoctorId: 10,
            content: "Old content");

        discussion.UpdateContent(
            "  New title  ",
            "  New content  ");

        Assert.Equal("New title", discussion.Title);
        Assert.Equal("New content", discussion.Content);
        Assert.NotNull(discussion.UpdatedAt);
    }
}