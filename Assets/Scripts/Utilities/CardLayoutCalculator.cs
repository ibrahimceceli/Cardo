using UnityEngine;

/// <summary>
/// Structure to hold card layout angles (rotation and roll) separately from the full quaternion.
/// </summary>
public struct CardLayoutAngles
{
    public float rotationAngle;
    public float rollAngle;

    public CardLayoutAngles(float rotationAngle, float rollAngle)
    {
        this.rotationAngle = rotationAngle;
        this.rollAngle = rollAngle;
    }
}

/// <summary>
/// Utility class for calculating arc/fan layout positions for cards.
/// Implements ICardLayoutCalculator to provide card positioning logic.
/// </summary>
public class CardLayoutCalculator : ICardLayoutCalculator
{
    // Layout configuration parameters
    private const float ArcRadius = 1f;
    private const float MaxAngle = 10f;
    private const float HorizontalSpacing = 1f;
    private const float ZOffsetPerCard = 0.01f;
    private const float MaxRollAngle = 8f;

    /// <summary>
    /// Calculates the layout information for a card in an arc/fan layout.
    /// </summary>
    /// <param name="cardIndex">The index of the card (0-based)</param>
    /// <param name="totalCards">The total number of cards in the layout</param>
    /// <param name="centerPosition">The center position for the layout</param>
    /// <param name="baseRotation">The base rotation to apply</param>
    /// <returns>CardLayoutInfo containing position, rotation, and sorting order</returns>
    public CardLayoutInfo CalculateCardLayout(int cardIndex, int totalCards, Vector3 centerPosition, Quaternion baseRotation)
    {
        if (totalCards <= 0)
        {
            return new CardLayoutInfo(centerPosition, baseRotation, 0);
        }

        // Calculate center index for symmetric layout
        float centerIndex = (totalCards - 1) / 2f;
        
        // Calculate angle step based on total cards
        float angleStep = totalCards > 1 ? MaxAngle / (totalCards - 1) : 0f;
        
        // Calculate angle for this card
        float angle = (cardIndex - centerIndex) * angleStep;
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate position using arc math
        float x = centerPosition.x + Mathf.Sin(angleRad) * ArcRadius + (cardIndex - centerIndex) * HorizontalSpacing;
        float y = centerPosition.y + Mathf.Cos(angleRad) * ArcRadius;
        float z = centerPosition.z + (cardIndex * ZOffsetPerCard);
        
        Vector3 position = new Vector3(x, y, z);
        
        // Calculate rotation angles
        float rotationAngle = angle;
        float rollAngle = 0f;
        
        // Calculate roll angle for fan effect
        if (centerIndex > 0)
        {
            rollAngle = -(cardIndex - centerIndex) / centerIndex * MaxRollAngle;
        }
        
        // Compose rotations: base * fan * roll
        Quaternion fanRotation = Quaternion.Euler(0, rotationAngle, 0);
        Quaternion rollRotation = Quaternion.Euler(0, 0, rollAngle);
        Quaternion finalRotation = baseRotation * fanRotation * rollRotation;
        
        // Sorting order is the card index
        int sortingOrder = cardIndex;
        
        return new CardLayoutInfo(position, finalRotation, sortingOrder);
    }

    /// <summary>
    /// Calculates the rotation and roll angles for a card in an arc/fan layout.
    /// This is a helper method for cases where angles are needed separately (e.g., for DrawCard method).
    /// </summary>
    /// <param name="cardIndex">The index of the card (0-based)</param>
    /// <param name="totalCards">The total number of cards in the layout</param>
    /// <returns>CardLayoutAngles containing rotationAngle and rollAngle</returns>
    public CardLayoutAngles CalculateCardAngles(int cardIndex, int totalCards)
    {
        if (totalCards <= 0)
        {
            return new CardLayoutAngles(0f, 0f);
        }

        float centerIndex = (totalCards - 1) / 2f;
        float angleStep = totalCards > 1 ? MaxAngle / (totalCards - 1) : 0f;
        
        float angle = (cardIndex - centerIndex) * angleStep;
        float rotationAngle = angle;
        
        float rollAngle = 0f;
        if (centerIndex > 0)
        {
            rollAngle = -(cardIndex - centerIndex) / centerIndex * MaxRollAngle;
        }
        
        return new CardLayoutAngles(rotationAngle, rollAngle);
    }
}
