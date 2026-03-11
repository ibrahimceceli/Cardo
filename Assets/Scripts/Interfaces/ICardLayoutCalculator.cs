using UnityEngine;

/// <summary>
/// Structure to hold card layout information including position, rotation, and sorting order.
/// </summary>
public struct CardLayoutInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public int sortingOrder;

    public CardLayoutInfo(Vector3 position, Quaternion rotation, int sortingOrder)
    {
        this.position = position;
        this.rotation = rotation;
        this.sortingOrder = sortingOrder;
    }
}

/// <summary>
/// Interface for card layout calculation operations.
/// Provides methods for calculating card positions in arc/fan layouts.
/// </summary>
public interface ICardLayoutCalculator
{
    /// <summary>
    /// Calculates the layout information for a card in an arc/fan layout.
    /// </summary>
    /// <param name="cardIndex">The index of the card (0-based)</param>
    /// <param name="totalCards">The total number of cards in the layout</param>
    /// <param name="centerPosition">The center position for the layout</param>
    /// <param name="baseRotation">The base rotation to apply</param>
    /// <returns>CardLayoutInfo containing position, rotation, and sorting order</returns>
    CardLayoutInfo CalculateCardLayout(int cardIndex, int totalCards, Vector3 centerPosition, Quaternion baseRotation);
}