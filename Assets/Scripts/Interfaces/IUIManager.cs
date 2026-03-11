/// <summary>
/// Interface for UI management operations.
/// Provides methods for updating UI elements like score, status, and hand type display.
/// </summary>
public interface IUIManager
{
    /// <summary>
    /// Sets a status message to display.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="restoreDiscardStatus">Whether to restore discard status after delay</param>
    void SetStatus(string message, bool restoreDiscardStatus = true);

    /// <summary>
    /// Updates the score display with chips and multiplier.
    /// </summary>
    /// <param name="chips">The chips value to display</param>
    /// <param name="multiplier">The multiplier value to display</param>
    void UpdateScore(int chips, int multiplier);

    /// <summary>
    /// Updates the total score display.
    /// </summary>
    /// <param name="score">The total score to display</param>
    void UpdateScore(int score);

    /// <summary>
    /// Updates the discard status display.
    /// </summary>
    /// <param name="remainingDiscards">The number of remaining discards</param>
    void UpdateDiscardStatus(int remainingDiscards);

    /// <summary>
    /// Shows the hand type display.
    /// </summary>
    /// <param name="handType">The poker hand type to display</param>
    void ShowHandType(PokerHandType handType);

    /// <summary>
    /// Hides the hand type display.
    /// </summary>
    void HideHandType();
}