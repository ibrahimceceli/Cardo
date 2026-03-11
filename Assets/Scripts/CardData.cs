using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cardo/Card Data")]
public class CardData : ScriptableObject
{
    public enum Suit
    {
        Glitch,
        Core,
        Neural,
        Protocol
    }

    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    [Header("Card Properties")]
    public Suit cardSuit;
    public Rank cardRank;

    public int NumericValue
    {
        get { return (int)cardRank; }
    }

    [Header("Joker Properties")]
    [SerializeField]
    private bool isJoker = false;
    
    [SerializeField]
    private int jokerBonusScore = 0;
    
    [SerializeField]
    private string jokerDescription = "";
    
    [SerializeField]
    private int jokerMultiplier = 1;
    
    [SerializeField]
    private int jokerHandSizeBonus = 0;

    public bool IsJoker
    {
        get { return isJoker; }
    }

    public int JokerBonusScore
    {
        get { return isJoker ? jokerBonusScore : 0; }
    }

    public string JokerDescription
    {
        get { return isJoker ? jokerDescription : ""; }
    }

    public int JokerMultiplier
    {
        get { return isJoker ? jokerMultiplier : 1; }
    }

    public int JokerHandSizeBonus
    {
        get { return isJoker ? jokerHandSizeBonus : 0; }
    }

    [Header("Visuals")]
    public Sprite cardFrontGraphic;
}
