using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Reviews.ValueObjects;

public record Rating
{
    public int Value { get; }

    private Rating(int value) => Value = value;

    public static Result<Rating> Create(int value)
    {
        if (value < 1 || value > 5)
            return Error.Validation("Rating.Value", "Rating must be between 1 and 5 stars.");

        return new Rating(value);
    }
}