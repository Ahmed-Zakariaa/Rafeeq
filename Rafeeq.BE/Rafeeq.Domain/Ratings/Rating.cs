using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Trips;

namespace Rafeeq.Domain.Ratings;

/// <summary>A 1–5 star rating one party leaves the other after a completed trip (one per direction per trip).</summary>
public class Rating : BaseEntity<int>
{
    public int TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public int FromUserId { get; private set; }
    public User? FromUser { get; private set; }
    public int ToUserId { get; private set; }
    public User? ToUser { get; private set; }

    public RatingRole RatedRole { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }

    private Rating() { }

    public Rating(int tripId, int fromUserId, int toUserId, RatingRole ratedRole, int stars, string? comment)
    {
        TripId = tripId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        RatedRole = ratedRole;
        Stars = stars;
        Comment = comment;
    }
}
