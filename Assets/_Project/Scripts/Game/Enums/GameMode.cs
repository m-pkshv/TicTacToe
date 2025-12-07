// Assets/_Project/Scripts/Game/Enums/GameMode.cs

namespace TicTacToe.Game.Enums
{
    /// <summary>
    /// Режим игры.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Игра против компьютера (AI).
        /// </summary>
        VsAI,
        
        /// <summary>
        /// Локальный мультиплеер (два игрока на одном устройстве).
        /// </summary>
        LocalMultiplayer,
        
        /// <summary>
        /// Сетевой мультиплеер (LAN).
        /// </summary>
        NetworkMultiplayer
    }
}
