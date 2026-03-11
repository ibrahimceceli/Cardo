public class HandScoringService
{
    /// <summary>
    /// Gets the base chip score for a given poker hand type.
    /// </summary>
    /// <param name="handType">The type of poker hand</param>
    /// <returns>The base chip score</returns>
    public int GetBaseScore(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return 160;
            case PokerHandType.FlushHouse: return 140;
            case PokerHandType.FiveOfAKind: return 120;
            case PokerHandType.RoyalFlush: return 100;
            case PokerHandType.StraightFlush: return 100;
            case PokerHandType.FourOfAKind: return 60;
            case PokerHandType.FullHouse: return 40;
            case PokerHandType.Flush: return 35;
            case PokerHandType.Straight: return 30;
            case PokerHandType.ThreeOfAKind: return 30;
            case PokerHandType.TwoPair: return 20;
            case PokerHandType.Pair: return 10;
            case PokerHandType.HighCard: return 5;
            case PokerHandType.None: return 1;
            default: return 0;
        }
    }

    /// <summary>
    /// Gets the multiplier for a given poker hand type.
    /// </summary>
    /// <param name="handType">The type of poker hand</param>
    /// <returns>The multiplier value</returns>
    public float GetMultiplier(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return 16f;
            case PokerHandType.FlushHouse: return 14f;
            case PokerHandType.FiveOfAKind: return 12f;
            case PokerHandType.RoyalFlush: return 8f;
            case PokerHandType.StraightFlush: return 8f;
            case PokerHandType.FourOfAKind: return 7f;
            case PokerHandType.FullHouse: return 4f;
            case PokerHandType.Flush: return 4f;
            case PokerHandType.Straight: return 4f;
            case PokerHandType.ThreeOfAKind: return 3f;
            case PokerHandType.TwoPair: return 2f;
            case PokerHandType.Pair: return 2f;
            case PokerHandType.HighCard: return 1f;
            case PokerHandType.None: return 1f;
            default: return 1f;
        }
    }
}
