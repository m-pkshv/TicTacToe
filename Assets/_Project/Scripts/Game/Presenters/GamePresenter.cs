// Assets/_Project/Scripts/Game/Presenters/GamePresenter.cs

using System;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;

namespace TicTacToe.Game.Presenters
{
    /// <summary>
    /// Presenter в паттерне MVP.
    /// Связывает BoardModel (данные) с IGameView (отображение).
    /// Обрабатывает игровую логику и управляет потоком игры.
    /// </summary>
    public class GamePresenter : IDisposable
    {
        private readonly BoardModel _board;
        private IGameView _view;
        
        private CellState _currentPlayer;
        private bool _isGameActive;
        private bool _isInputLocked;
        
        /// <summary>
        /// Текущий игрок (X или O).
        /// </summary>
        public CellState CurrentPlayer => _currentPlayer;
        
        /// <summary>
        /// Активна ли игра (не завершена).
        /// </summary>
        public bool IsGameActive => _isGameActive;
        
        /// <summary>
        /// Модель игрового поля.
        /// </summary>
        public BoardModel Board => _board;
        
        /// <summary>
        /// Вызывается при смене хода.
        /// </summary>
        public event Action<CellState> OnTurnChanged;
        
        /// <summary>
        /// Вызывается при завершении игры.
        /// </summary>
        public event Action<GameResult> OnGameEnded;
        
        /// <summary>
        /// Вызывается при успешном ходе.
        /// Параметры: индекс ячейки, игрок.
        /// </summary>
        public event Action<int, CellState> OnMoveMade;
        
        /// <summary>
        /// Создаёт новый GamePresenter.
        /// </summary>
        public GamePresenter()
        {
            _board = new BoardModel();
            _currentPlayer = CellState.X;
            _isGameActive = false;
            _isInputLocked = false;
        }
        
        /// <summary>
        /// Создаёт GamePresenter с существующей моделью (для тестов).
        /// </summary>
        /// <param name="board">Модель поля.</param>
        public GamePresenter(BoardModel board)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _currentPlayer = CellState.X;
            _isGameActive = false;
            _isInputLocked = false;
        }
        
        /// <summary>
        /// Привязать View к Presenter.
        /// </summary>
        /// <param name="view">Реализация IGameView.</param>
        public void SetView(IGameView view)
        {
            _view = view;
        }
        
        /// <summary>
        /// Начать новую игру.
        /// </summary>
        /// <param name="startingPlayer">Кто ходит первым (по умолчанию X).</param>
        public void StartGame(CellState startingPlayer = CellState.X)
        {
            if (startingPlayer == CellState.Empty)
            {
                startingPlayer = CellState.X;
            }
            
            _board.Reset();
            _currentPlayer = startingPlayer;
            _isGameActive = true;
            _isInputLocked = false;
            
            _view?.ResetBoard();
            _view?.SetInputLocked(false);
            _view?.ShowCurrentTurn(_currentPlayer);
        }
        
        /// <summary>
        /// Обработать попытку хода игрока.
        /// Вызывается из View при клике на ячейку.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки (0-8).</param>
        /// <returns>true, если ход выполнен успешно.</returns>
        public bool TryMakeMove(int cellIndex)
        {
            if (!CanMakeMove(cellIndex))
            {
                return false;
            }
            
            return ExecuteMove(cellIndex, _currentPlayer);
        }
        
        /// <summary>
        /// Выполнить ход (для AI или сетевого игрока).
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки.</param>
        /// <param name="player">Игрок, делающий ход.</param>
        /// <returns>true, если ход выполнен успешно.</returns>
        public bool MakeMove(int cellIndex, CellState player)
        {
            if (!_isGameActive)
            {
                return false;
            }
            
            if (player == CellState.Empty)
            {
                return false;
            }
            
            if (!_board.IsValidMove(cellIndex, player))
            {
                return false;
            }
            
            return ExecuteMove(cellIndex, player);
        }
        
        /// <summary>
        /// Заблокировать ввод игрока.
        /// </summary>
        public void LockInput()
        {
            _isInputLocked = true;
            _view?.SetInputLocked(true);
        }
        
        /// <summary>
        /// Разблокировать ввод игрока.
        /// </summary>
        public void UnlockInput()
        {
            _isInputLocked = false;
            _view?.SetInputLocked(false);
        }
        
        /// <summary>
        /// Перезапустить игру с теми же настройками.
        /// </summary>
        public void RestartGame()
        {
            StartGame(CellState.X);
        }
        
        /// <summary>
        /// Проверить, может ли текущий игрок сделать ход.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки.</param>
        /// <returns>true, если ход возможен.</returns>
        public bool CanMakeMove(int cellIndex)
        {
            if (!_isGameActive)
            {
                return false;
            }
            
            if (_isInputLocked)
            {
                return false;
            }
            
            return _board.IsCellEmpty(cellIndex);
        }
        
        /// <summary>
        /// Освободить ресурсы.
        /// </summary>
        public void Dispose()
        {
            _view = null;
        }
        
        private bool ExecuteMove(int cellIndex, CellState player)
        {
            if (!_board.MakeMove(cellIndex, player))
            {
                return false;
            }
            
            // Обновить View
            _view?.UpdateCell(cellIndex, player);
            
            // Уведомить о ходе
            OnMoveMade?.Invoke(cellIndex, player);
            
            // Проверить результат
            GameResult result = _board.GetGameResult();
            
            if (result != GameResult.None)
            {
                EndGame(result);
                return true;
            }
            
            // Переключить ход
            SwitchTurn();
            
            return true;
        }
        
        private void SwitchTurn()
        {
            _currentPlayer = _currentPlayer == CellState.X ? CellState.O : CellState.X;
            
            _view?.ShowCurrentTurn(_currentPlayer);
            OnTurnChanged?.Invoke(_currentPlayer);
        }
        
        private void EndGame(GameResult result)
        {
            _isGameActive = false;
            _isInputLocked = true;
            
            // Подсветить выигрышную линию при победе
            if (result == GameResult.XWins || result == GameResult.OWins)
            {
                int[] winningLine = _board.GetWinningLine();
                if (winningLine != null)
                {
                    _view?.HighlightWinningLine(winningLine);
                }
            }
            
            _view?.SetInputLocked(true);
            _view?.ShowGameResult(result);
            
            OnGameEnded?.Invoke(result);
        }
    }
}
