namespace NotifyMe.Domain.Events;

/// <summary>
/// The kinds of "something important" the brief calls out. Deliberately closed for now; adding
/// a category is a conscious domain change, unlike adding a channel (see
/// docs/architecture/notification-channels.md for why channels are open-ended instead).
/// </summary>
public enum EventCategory
{
    BreakingNews = 0,
    MarketMovement = 1,
    NaturalDisaster = 2,
}
