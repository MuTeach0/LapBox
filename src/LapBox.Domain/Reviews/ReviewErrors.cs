using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Reviews;

public static class ReviewErrors
{
    public static readonly Error CommentRequired =
        Error.Validation("Review.Comment", "Comment cannot be empty.");
}