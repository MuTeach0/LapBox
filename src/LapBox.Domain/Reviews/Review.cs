using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Reviews.Events;
using LapBox.Domain.Reviews.ValueObjects;

namespace LapBox.Domain.Reviews;

public sealed class Review : AggregateRoot
{
    public Guid LaptopId { get; private set; } // Slicing ID
    public Guid UserId { get; private set; }   // Slicing ID
    public Rating Rating { get; private set; } = null!;
    public string Comment { get; private set; } = string.Empty;

    private Review() { }

    private Review(Guid id, Guid laptopId, Guid userId, Rating rating, string comment) : base(id)
    {
        LaptopId = laptopId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
    }

    public static Result<Review> Create(Guid laptopId, Guid userId, Rating rating, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return ReviewErrors.CommentRequired;

        var review =new Review(Guid.NewGuid(), laptopId, userId, rating, comment);
        review.AddDomainEvent(new ReviewCreatedEvent(review.Id, laptopId, userId, rating.Value));
        return review;
    }
}