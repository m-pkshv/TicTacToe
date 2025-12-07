// Assets/_Project/Scripts/AI/EasyAI.cs

using System.Collections.Generic;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;

namespace TicTacToe.AI
{
    /// <summary>
    /// Лёгкий ИИ: случайный выбор из доступных ячеек.
    /// Подходит для начинающих игроков.
    /// </summary>
    public class EasyAI : IAIPlayer
    {
        private readonly System.Random _random;
        
        /// <inheritdoc/>
        public AIDifficulty Difficulty => AIDifficulty.Easy;
        
        /// <summary>
        /// Создаёт экземпляр лёгкого ИИ.
        /// </summary>
        public EasyAI()
        {
            _random = new System.Random();
        }
        
        /// <summary>
        /// Создаёт экземпляр лёгкого ИИ с заданным seed для тестирования.
        /// </summary>
        /// <param name="seed">Seed для генератора случайных чисел</param>
        public EasyAI(int seed)
        {
            _random = new System.Random(seed);
        }
        
        /// <inheritdoc/>
        public int GetMove(BoardModel board, CellState aiSymbol)
        {
            if (board == null)
            {
                return -1;
            }
            
            List<int> emptyCells = board.GetEmptyCells();
            
            if (emptyCells.Count == 0)
            {
                return -1;
            }
            
            int randomIndex = _random.Next(emptyCells.Count);
            return emptyCells[randomIndex];
        }
    }
}
