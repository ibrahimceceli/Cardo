using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface for hand management operations.
/// Provides methods for managing cards in hand, selections, and playing/discarding hands.
/// </summary>
public interface IHandManager
{
    /// <summary>
    /// Gets the list of cards currently in hand.
    /// </summary>
    List<CardObject> currentHand { get; }

    /// <summary>
    /// Gets or sets the total chips accumulated.
    /// </summary>
    int totalChips { get; set; }

    /// <summary>
    /// Gets or sets the total multiplier.
    /// </summary>
    int totalMultiplier { get; set; }

    /// <summary>
    /// Adds a card to the current hand.
    /// </summary>
    /// <param name="newCard">The card to add</param>
    void AddCardToHand(CardObject newCard);

    /// <summary>
    /// Checks if a card can be selected.
    /// </summary>
    /// <param name="card">The card to check</param>
    /// <returns>True if the card can be selected, false otherwise</returns>
    bool CanSelectCard(CardObject card);

    /// <summary>
    /// Updates the selected hand type based on currently selected cards.
    /// </summary>
    void UpdateSelectedHandType();

    /// <summary>
    /// Plays the currently selected hand and draws replacement cards.
    /// </summary>
    /// <returns>True if hand was successfully played, false otherwise</returns>
    bool PlayHand();

    /// <summary>
    /// Discards the selected cards and draws new ones.
    /// </summary>
    void DiscardSelectedAndDraw();

    /// <summary>
    /// Gets the remaining number of discards available.
    /// </summary>
    /// <returns>The number of remaining discards</returns>
    int GetRemainingDiscards();
}