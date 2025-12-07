// Assets/_Project/Scripts/Game/Enums/AIDifficulty.cs

namespace TicTacToe.Game.Enums
{
    /// <summary>
    /// Уровень сложности компьютерного противника (AI).
    /// </summary>
    public enum AIDifficulty
    {
        /// <summary>
        /// Лёгкий уровень: случайный выбор хода.
        /// </summary>
        Easy = 0,
        
        /// <summary>
        /// Средний уровень: эвристика с элементом случайности (70% умных ходов).
        /// </summary>
        Medium = 1,
        
        /// <summary>
        /// Сложный уровень: алгоритм Minimax с альфа-бета отсечением (непобедим).
        /// </summary>
        Hard = 2
    }
}
