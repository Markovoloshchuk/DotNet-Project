using Feedback.Application.Common.Models;
using Feedback.Domain.Entities;

namespace Feedback.Application.Common.Mappings;

public static class DoctorDiscussionMapping
{
    public static DoctorDiscussionDto ToDto(
        this DoctorDiscussion discussion)
    {
        return new DoctorDiscussionDto(
            discussion.Id.ToString(),
            discussion.DiscussionId,
            discussion.Title,
            discussion.AuthorDoctorId,
            discussion.Content,
            discussion.Tags.ToArray(),
            discussion.Comments
                .Select(comment => new DiscussionCommentDto(
                    comment.CommentId,
                    comment.DoctorRefId,
                    comment.Text,
                    comment.CreatedAt))
                .ToArray(),
            discussion.CreatedAt,
            discussion.UpdatedAt,
            discussion.Version);
    }
}