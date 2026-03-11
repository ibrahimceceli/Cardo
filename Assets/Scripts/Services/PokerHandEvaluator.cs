using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerHandEvaluator
{
    /// <summary>
    /// Determines the poker hand type from a list of selected cards.
    /// </summary>
    /// <param name="selectedCards">List of CardObject instances to evaluate</param>
    /// <returns>The PokerHandType that best matches the cards</returns>
    public PokerHandType DetermineHand(List<CardObject> selectedCards)
    {
        if (selectedCards.Count < 1 || selectedCards.Count > 5)
        {
            return PokerHandType.None;
        }

        HandAnalysis analysis = AnalyzeHand(selectedCards);
        
        return EvaluateHandType(analysis, selectedCards.Count);
    }
    
    private HandAnalysis AnalyzeHand(List<CardObject> selectedCards)
    {
        Dictionary<int, int> rankCounts = new Dictionary<int, int>();
        Dictionary<string, int> suitCounts = new Dictionary<string, int>();

        foreach (CardObject cardObj in selectedCards)
        {
            int rankValue = cardObj.cardInfo.NumericValue;
            string suitValue = cardObj.cardInfo.cardSuit.ToString();

            if (rankCounts.ContainsKey(rankValue)) rankCounts[rankValue]++;
            else rankCounts.Add(rankValue, 1);

            if (suitCounts.ContainsKey(suitValue)) suitCounts[suitValue]++;
            else suitCounts.Add(suitValue, 1);
        }

        List<int> sortedRanks = rankCounts.Keys.OrderBy(r => r).ToList();
        
        RankCombinationCounts rankCombinations = CountRankCombinations(rankCounts);
        bool isFlush = CheckFlush(selectedCards.Count, suitCounts);
        bool isStraight = CheckStraight(sortedRanks, selectedCards.Count);

        return new HandAnalysis
        {
            RankCounts = rankCounts,
            SortedRanks = sortedRanks,
            RankCombinations = rankCombinations,
            IsFlush = isFlush,
            IsStraight = isStraight
        };
    }
    
    private RankCombinationCounts CountRankCombinations(Dictionary<int, int> rankCounts)
    {
        return new RankCombinationCounts
        {
            Pairs = rankCounts.Values.Count(c => c == 2),
            ThreeOfAKinds = rankCounts.Values.Count(c => c == 3),
            FourOfAKinds = rankCounts.Values.Count(c => c == 4)
        };
    }
    
    private bool CheckFlush(int cardCount, Dictionary<string, int> suitCounts)
    {
        return cardCount == 5 && suitCounts.Count == 1;
    }
    
    private PokerHandType EvaluateHandType(HandAnalysis analysis, int cardCount)
    {
        bool isFiveOfAKind = cardCount == 5 && analysis.RankCounts.Count == 1;
        bool isFullHouse = analysis.RankCombinations.ThreeOfAKinds == 1 && analysis.RankCombinations.Pairs >= 1;
        
        // Check special combinations first (highest priority)
        if (isFiveOfAKind && analysis.IsFlush)
        {
            return PokerHandType.FlushFive;
        }
        
        if (isFullHouse && analysis.IsFlush)
        {
            return PokerHandType.FlushHouse;
        }
        
        if (isFiveOfAKind)
        {
            return PokerHandType.FiveOfAKind;
        }
        
        if (analysis.IsStraight && analysis.IsFlush && CheckRoyalFlush(analysis.SortedRanks))
        {
            return PokerHandType.RoyalFlush;
        }
        
        if (analysis.IsStraight && analysis.IsFlush)
        {
            return PokerHandType.StraightFlush;
        }
        
        // Check rank-based combinations
        return EvaluateRankCombinations(analysis);
    }
    
    private PokerHandType EvaluateRankCombinations(HandAnalysis analysis)
    {
        if (analysis.RankCombinations.FourOfAKinds == 1)
        {
            return PokerHandType.FourOfAKind;
        }
        
        bool isFullHouse = analysis.RankCombinations.ThreeOfAKinds == 1 && analysis.RankCombinations.Pairs >= 1;
        if (isFullHouse)
        {
            return PokerHandType.FullHouse;
        }
        
        if (analysis.IsFlush)
        {
            return PokerHandType.Flush;
        }
        
        if (analysis.IsStraight)
        {
            return PokerHandType.Straight;
        }
        
        if (analysis.RankCombinations.ThreeOfAKinds == 1)
        {
            return PokerHandType.ThreeOfAKind;
        }
        
        if (analysis.RankCombinations.Pairs == 2)
        {
            return PokerHandType.TwoPair;
        }
        
        if (analysis.RankCombinations.Pairs == 1)
        {
            return PokerHandType.Pair;
        }
        
        return PokerHandType.HighCard;
    }

    /// <summary>
    /// Checks if the sorted ranks form a straight.
    /// </summary>
    private bool CheckStraight(List<int> sortedRanks, int cardCount)
    {
        if (cardCount != 5 || sortedRanks.Count < 5)
        {
            return false;
        }

        if (sortedRanks.Count == 5)
        {
            // Normal straight: consecutive ranks
            if (sortedRanks[4] - sortedRanks[0] == 4)
            {
                return true;
            }
            // Ace-low straight: A-2-3-4-5
            else if (sortedRanks.Contains(14) && sortedRanks.Contains(2) && 
                     sortedRanks.Contains(3) && sortedRanks.Contains(4) && sortedRanks.Contains(5))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the sorted ranks form a royal flush (10-J-Q-K-A).
    /// </summary>
    private bool CheckRoyalFlush(List<int> sortedRanks)
    {
        return sortedRanks.Contains(10) && sortedRanks.Contains(11) && 
               sortedRanks.Contains(12) && sortedRanks.Contains(13) && sortedRanks.Contains(14);
    }
}

/// <summary>
/// Helper struct to hold rank combination counts (pairs, three of a kind, etc.)
/// </summary>
internal struct RankCombinationCounts
{
    public int Pairs;
    public int ThreeOfAKinds;
    public int FourOfAKinds;
}

/// <summary>
/// Helper struct to hold analyzed hand data
/// </summary>
internal struct HandAnalysis
{
    public Dictionary<int, int> RankCounts;
    public List<int> SortedRanks;
    public RankCombinationCounts RankCombinations;
    public bool IsFlush;
    public bool IsStraight;
}
