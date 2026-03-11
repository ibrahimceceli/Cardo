using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Structure to hold collected card transform data (position, rotation, and sorting order).
/// </summary>
public struct CardTransformData
{
    public List<Vector3> positions;
    public List<Quaternion> rotations;
    public List<int> sortingOrders;

    public CardTransformData(List<Vector3> positions, List<Quaternion> rotations, List<int> sortingOrders)
    {
        this.positions = positions;
        this.rotations = rotations;
        this.sortingOrders = sortingOrders;
    }
}

/// <summary>
/// Utility class for collecting card transform information (position, rotation, sorting order).
/// Centralizes the pattern of collecting transform data from a list of cards.
/// </summary>
public static class CardTransformHelper
{
    /// <summary>
    /// Collects position, rotation, and sorting order data from a list of cards.
    /// </summary>
    /// <param name="cards">The list of CardObject instances to collect transform data from</param>
    /// <returns>CardTransformData containing positions, rotations, and sorting orders lists</returns>
    public static CardTransformData CollectCardTransforms(List<CardObject> cards)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        List<int> sortingOrders = new List<int>();

        foreach (CardObject card in cards)
        {
            if (card == null)
            {
                continue;
            }

            positions.Add(card.transform.position);
            rotations.Add(card.transform.rotation);

            // Use cached SpriteRenderer property instead of GetComponent
            SpriteRenderer spriteRenderer = card.SpriteRenderer;
            int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
            sortingOrders.Add(sortingOrder);
        }

        return new CardTransformData(positions, rotations, sortingOrders);
    }
}
