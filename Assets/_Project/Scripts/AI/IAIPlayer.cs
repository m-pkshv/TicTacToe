// Assets/_Project/Scripts/AI/IAIPlayer.cs

using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;

namespace TicTacToe.AI
{
    /// <summary>
    /// Интерфейс для ИИ игрока.
    /// Определяет контракт для всех уровней сложности ИИ.
    /// </summary>
    public interface IAIPlayer
    {
        /// <summary>
        /// Уровень сложности ИИ.
        /// </summary>
        AIDifficulty Difficulty { get; }
        
        /// <summary>
        /// Вычислить следующий ход.
        /// </summary>
        /// <param name="board">Текущее состояние поля</param>
        /// <param name="aiSymbol">Символ ИИ (X или O)</param>
        /// <returns>Индекс ячейки для хода (0-8), или -1 если ход невозможен</returns>
        int GetMove(BoardModel board, CellState aiSymbol);
    }
}
