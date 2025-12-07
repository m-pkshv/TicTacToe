// Assets/_Project/Scripts/Game/Enums/GameResult.cs

namespace TicTacToe.Game.Enums
{
    /// <summary>
    /// Результат игры.
    /// </summary>
    public enum GameResult
    {
        /// <summary>
        /// Игра ещё не завершена.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Победа крестиков (X).
        /// </summary>
        XWins = 1,
        
        /// <summary>
        /// Победа ноликов (O).
        /// </summary>
        OWins = 2,
        
        /// <summary>
        /// Ничья (все ячейки заняты, победителя нет).
        /// </summary>
        Draw = 3
    }
}
