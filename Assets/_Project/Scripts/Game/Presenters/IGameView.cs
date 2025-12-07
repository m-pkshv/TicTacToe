// Assets/_Project/Scripts/Game/Presenters/IGameView.cs

using TicTacToe.Game.Enums;

namespace TicTacToe.Game.Presenters
{
    /// <summary>
    /// Интерфейс View для паттерна MVP.
    /// Определяет контракт для отображения состояния игры.
    /// Реализуется UI-компонентами (например, GameScreen, BoardView).
    /// </summary>
    public interface IGameView
    {
        /// <summary>
        /// Обновить отображение ячейки.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки (0-8).</param>
        /// <param name="state">Новое состояние ячейки.</param>
        void UpdateCell(int cellIndex, CellState state);
        
        /// <summary>
        /// Показать, чей сейчас ход.
        /// </summary>
        /// <param name="currentPlayer">Текущий игрок (X или O).</param>
        void ShowCurrentTurn(CellState currentPlayer);
        
        /// <summary>
        /// Показать результат игры.
        /// </summary>
        /// <param name="result">Результат: XWins, OWins или Draw.</param>
        void ShowGameResult(GameResult result);
        
        /// <summary>
        /// Подсветить выигрышную линию.
        /// </summary>
        /// <param name="winningCells">Массив индексов ячеек выигрышной линии (3 элемента).</param>
        void HighlightWinningLine(int[] winningCells);
        
        /// <summary>
        /// Сбросить отображение доски в начальное состояние.
        /// </summary>
        void ResetBoard();
        
        /// <summary>
        /// Заблокировать/разблокировать ввод игрока.
        /// Используется при ходе AI или в сетевой игре.
        /// </summary>
        /// <param name="isLocked">true — заблокировать, false — разблокировать.</param>
        void SetInputLocked(bool isLocked);
        
        /// <summary>
        /// Показать индикатор "AI думает".
        /// </summary>
        /// <param name="isThinking">true — показать, false — скрыть.</param>
        void ShowAIThinking(bool isThinking);
        
        /// <summary>
        /// Показать индикатор ожидания сетевого игрока.
        /// </summary>
        /// <param name="isWaiting">true — показать, false — скрыть.</param>
        void ShowWaitingForPlayer(bool isWaiting);
    }
}
