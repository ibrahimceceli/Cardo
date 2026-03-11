using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface for deck management operations.
/// Provides methods for initializing, shuffling, and drawing cards.
/// </summary>
public interface IDeckManager
{
    /// <summary>
    /// Initializes the deck with all card data.
    /// </summary>
    void InitializeDeck();

    /// <summary>
    /// Shuffles the draw deck.
    /// </summary>
    void ShuffleDrawDeck();

    /// <summary>
    /// Draws a card at the specified position with rotation and sorting order.
    /// </summary>
    /// <param name="spawnPosition">Position where the card should be spawned</param>
    /// <param name="rotationAngle">Rotation angle in degrees</param>
    /// <param name="rollAngle">Roll angle in degrees</param>
    /// <param name="sortingOrder">Sorting order for sprite renderer</param>
    /// <returns>The drawn CardObject, or null if deck is empty</returns>
    CardObject DrawCard(Vector3 spawnPosition, float rotationAngle = 0f, float rollAngle = 0f, int sortingOrder = 0);

    /// <summary>
    /// Draws a card at the specified position with quaternion rotation and sorting order.
    /// </summary>
    /// <param name="spawnPosition">Position where the card should be spawned</param>
    /// <param name="spawnRotation">Rotation as Quaternion</param>
    /// <param name="sortingOrder">Sorting order for sprite renderer</param>
    /// <returns>The drawn CardObject, or null if deck is empty</returns>
    CardObject DrawCardAtPosition(Vector3 spawnPosition, Quaternion spawnRotation, int sortingOrder);

    /// <summary>
    /// Rearranges the current hand by recalculating positions for all cards.
    /// </summary>
    void RearrangeHand();
}