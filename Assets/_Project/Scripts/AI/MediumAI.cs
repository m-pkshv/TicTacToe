// Assets/_Project/Scripts/AI/MediumAI.cs

using System.Collections.Generic;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;

namespace TicTacToe.AI
{
    /// <summary>
    /// Средний ИИ: эвристика с элементом случайности.
    /// Всегда блокирует победу противника и завершает свою победу.
    /// В 70% случаев делает стратегически выгодный ход (центр → углы → стороны).
    /// </summary>
    public class MediumAI : IAIPlayer
    {
        private const float SMART_MOVE_CHANCE = 0.7f;
        
        /// <summary>
        /// Приоритеты позиций: центр (4), углы (0,2,6,8), стороны (1,3,5,7).
        /// </summary>
        private static readonly int[] POSITION_PRIORITY = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };
        
        private readonly System.Random _random;
        
        /// <inheritdoc/>
        public AIDifficulty Difficulty => AIDifficulty.Medium;
        
        /// <summary>
        /// Создаёт экземпляр среднего ИИ.
        /// </summary>
        public MediumAI()
        {
            _random = new System.Random();
        }
        
        /// <summary>
        /// Создаёт экземпляр среднего ИИ с заданным seed для тестирования.
        /// </summary>
        /// <param name="seed">Seed для генератора случайных чисел</param>
        public MediumAI(int seed)
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
            
            CellState playerSymbol = aiSymbol == CellState.X ? CellState.O : CellState.X;
            
            // Приоритет 1: Победить (если можно завершить линию)
            int winMove = FindWinningMove(board, aiSymbol);
            if (winMove != -1)
            {
                return winMove;
            }
            
            // Приоритет 2: Блокировать победу противника
            int blockMove = FindWinningMove(board, playerSymbol);
            if (blockMove != -1)
            {
                return blockMove;
            }
            
            // Приоритет 3: Умный или случайный ход (70%/30%)
            if (_random.NextDouble() < SMART_MOVE_CHANCE)
            {
                return GetStrategicMove(board);
            }
            else
            {
                return GetRandomMove(board, emptyCells);
            }
        }
        
        /// <summary>
        /// Находит ход для завершения линии (победа или блокировка).
        /// </summary>
        /// <param name="board">Текущее состояние поля</param>
        /// <param name="symbol">Символ для поиска (X или O)</param>
        /// <returns>Индекс ячейки для завершения линии, или -1 если нет такой возможности</returns>
        private int FindWinningMove(BoardModel board, CellState symbol)
        {
            foreach (int[] combo in BoardModel.WIN_COMBINATIONS)
            {
                int emptyCount = 0;
                int symbolCount = 0;
                int emptyIndex = -1;
                
                foreach (int index in combo)
                {
                    CellState cell = board.GetCell(index);
                    
                    if (cell == CellState.Empty)
                    {
                        emptyCount++;
                        emptyIndex = index;
                    }
                    else if (cell == symbol)
                    {
                        symbolCount++;
                    }
                }
                
                // Если 2 символа и 1 пустая — можно завершить линию
                if (symbolCount == 2 && emptyCount == 1)
                {
                    return emptyIndex;
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Выбирает стратегически выгодную позицию по приоритету.
        /// </summary>
        /// <param name="board">Текущее состояние поля</param>
        /// <returns>Индекс лучшей доступной позиции</returns>
        private int GetStrategicMove(BoardModel board)
        {
            foreach (int index in POSITION_PRIORITY)
            {
                if (board.IsCellEmpty(index))
                {
                    return index;
                }
            }
            
            // Fallback (не должен произойти, если есть пустые клетки)
            return -1;
        }
        
        /// <summary>
        /// Выбирает случайную пустую ячейку.
        /// </summary>
        /// <param name="board">Текущее состояние поля</param>
        /// <param name="emptyCells">Список пустых ячеек</param>
        /// <returns>Случайный индекс пустой ячейки</returns>
        private int GetRandomMove(BoardModel board, List<int> emptyCells)
        {
            if (emptyCells.Count == 0)
            {
                return -1;
            }
            
            return emptyCells[_random.Next(emptyCells.Count)];
        }
    }
}
