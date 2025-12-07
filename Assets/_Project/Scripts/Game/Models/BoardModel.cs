// Assets/_Project/Scripts/Game/Models/BoardModel.cs

using System;
using System.Collections.Generic;
using TicTacToe.Game.Enums;

namespace TicTacToe.Game.Models
{
    /// <summary>
    /// Модель игрового поля 3x3.
    /// Отвечает за хранение состояния ячеек, проверку победы и ничьей.
    /// </summary>
    public class BoardModel
    {
        /// <summary>
        /// Размер стороны игрового поля.
        /// </summary>
        public const int BOARD_SIZE = 3;
        
        /// <summary>
        /// Общее количество ячеек на поле.
        /// </summary>
        public const int TOTAL_CELLS = 9;
        
        /// <summary>
        /// Все выигрышные комбинации (индексы ячеек).
        /// Порядок: 3 ряда, 3 столбца, 2 диагонали.
        /// </summary>
        public static readonly int[][] WIN_COMBINATIONS = new int[][]
        {
            new[] { 0, 1, 2 }, // Верхний ряд
            new[] { 3, 4, 5 }, // Средний ряд
            new[] { 6, 7, 8 }, // Нижний ряд
            new[] { 0, 3, 6 }, // Левый столбец
            new[] { 1, 4, 7 }, // Средний столбец
            new[] { 2, 5, 8 }, // Правый столбец
            new[] { 0, 4, 8 }, // Главная диагональ
            new[] { 2, 4, 6 }  // Побочная диагональ
        };
        
        private readonly CellState[] _cells;
        private int[] _lastWinningLine;
        
        /// <summary>
        /// Вызывается при изменении состояния ячейки.
        /// Параметры: индекс ячейки, новое состояние.
        /// </summary>
        public event Action<int, CellState> OnCellChanged;
        
        /// <summary>
        /// Вызывается при сбросе поля.
        /// </summary>
        public event Action OnBoardReset;
        
        /// <summary>
        /// Текущее состояние всех ячеек (только для чтения).
        /// </summary>
        public IReadOnlyList<CellState> Cells => _cells;
        
        /// <summary>
        /// Количество сделанных ходов.
        /// </summary>
        public int MoveCount { get; private set; }
        
        /// <summary>
        /// Создаёт новую модель поля с пустыми ячейками.
        /// </summary>
        public BoardModel()
        {
            _cells = new CellState[TOTAL_CELLS];
            Reset();
        }
        
        /// <summary>
        /// Приватный конструктор для клонирования.
        /// </summary>
        private BoardModel(CellState[] cells, int moveCount)
        {
            _cells = new CellState[TOTAL_CELLS];
            Array.Copy(cells, _cells, TOTAL_CELLS);
            MoveCount = moveCount;
        }
        
        /// <summary>
        /// Сбрасывает поле в начальное состояние.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < TOTAL_CELLS; i++)
            {
                _cells[i] = CellState.Empty;
            }
            
            MoveCount = 0;
            _lastWinningLine = null;
            OnBoardReset?.Invoke();
        }
        
        /// <summary>
        /// Выполняет ход в указанную ячейку.
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8).</param>
        /// <param name="player">Игрок (X или O).</param>
        /// <returns>true, если ход успешен; false, если ход невозможен.</returns>
        public bool MakeMove(int index, CellState player)
        {
            if (!IsValidMove(index, player))
            {
                return false;
            }
            
            _cells[index] = player;
            MoveCount++;
            OnCellChanged?.Invoke(index, player);
            
            return true;
        }
        
        /// <summary>
        /// Проверяет, является ли ход допустимым.
        /// </summary>
        /// <param name="index">Индекс ячейки.</param>
        /// <param name="player">Игрок.</param>
        /// <returns>true, если ход допустим.</returns>
        public bool IsValidMove(int index, CellState player)
        {
            if (index < 0 || index >= TOTAL_CELLS)
            {
                return false;
            }
            
            if (_cells[index] != CellState.Empty)
            {
                return false;
            }
            
            if (player == CellState.Empty)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Проверяет, пуста ли указанная ячейка.
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8).</param>
        /// <returns>true, если ячейка пуста.</returns>
        public bool IsCellEmpty(int index)
        {
            if (index < 0 || index >= TOTAL_CELLS)
            {
                return false;
            }
            
            return _cells[index] == CellState.Empty;
        }
        
        /// <summary>
        /// Возвращает состояние указанной ячейки.
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8).</param>
        /// <returns>Состояние ячейки или Empty при некорректном индексе.</returns>
        public CellState GetCell(int index)
        {
            if (index < 0 || index >= TOTAL_CELLS)
            {
                return CellState.Empty;
            }
            
            return _cells[index];
        }
        
        /// <summary>
        /// Проверяет наличие победителя.
        /// </summary>
        /// <returns>Результат игры (None, XWins, OWins).</returns>
        public GameResult CheckWin()
        {
            foreach (int[] combo in WIN_COMBINATIONS)
            {
                CellState first = _cells[combo[0]];
                
                if (first == CellState.Empty)
                {
                    continue;
                }
                
                if (_cells[combo[1]] == first && _cells[combo[2]] == first)
                {
                    _lastWinningLine = combo;
                    return first == CellState.X ? GameResult.XWins : GameResult.OWins;
                }
            }
            
            _lastWinningLine = null;
            return GameResult.None;
        }
        
        /// <summary>
        /// Проверяет, закончилась ли игра ничьей.
        /// </summary>
        /// <returns>true, если ничья (все ячейки заняты, победителя нет).</returns>
        public bool CheckDraw()
        {
            return MoveCount >= TOTAL_CELLS && CheckWin() == GameResult.None;
        }
        
        /// <summary>
        /// Возвращает полный результат игры (включая ничью).
        /// </summary>
        /// <returns>Результат: None (игра продолжается), XWins, OWins или Draw.</returns>
        public GameResult GetGameResult()
        {
            GameResult winResult = CheckWin();
            
            if (winResult != GameResult.None)
            {
                return winResult;
            }
            
            if (MoveCount >= TOTAL_CELLS)
            {
                return GameResult.Draw;
            }
            
            return GameResult.None;
        }
        
        /// <summary>
        /// Возвращает индексы ячеек выигрышной линии.
        /// </summary>
        /// <returns>Массив из 3 индексов или null, если победителя нет.</returns>
        public int[] GetWinningLine()
        {
            if (_lastWinningLine == null)
            {
                CheckWin();
            }
            
            return _lastWinningLine;
        }
        
        /// <summary>
        /// Возвращает список индексов пустых ячеек.
        /// </summary>
        /// <returns>Список индексов пустых ячеек.</returns>
        public List<int> GetEmptyCells()
        {
            List<int> emptyCells = new List<int>(TOTAL_CELLS - MoveCount);
            
            for (int i = 0; i < TOTAL_CELLS; i++)
            {
                if (_cells[i] == CellState.Empty)
                {
                    emptyCells.Add(i);
                }
            }
            
            return emptyCells;
        }
        
        /// <summary>
        /// Создаёт глубокую копию модели (для AI Minimax).
        /// События не копируются.
        /// </summary>
        /// <returns>Новый экземпляр BoardModel с тем же состоянием.</returns>
        public BoardModel Clone()
        {
            return new BoardModel(_cells, MoveCount);
        }
        
        /// <summary>
        /// Преобразует индекс ячейки в координаты (row, col).
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8).</param>
        /// <returns>Кортеж (row, col) где row и col от 0 до 2.</returns>
        public static (int row, int col) IndexToCoords(int index)
        {
            return (index / BOARD_SIZE, index % BOARD_SIZE);
        }
        
        /// <summary>
        /// Преобразует координаты (row, col) в индекс ячейки.
        /// </summary>
        /// <param name="row">Строка (0-2).</param>
        /// <param name="col">Столбец (0-2).</param>
        /// <returns>Индекс ячейки (0-8).</returns>
        public static int CoordsToIndex(int row, int col)
        {
            return row * BOARD_SIZE + col;
        }
    }
}
