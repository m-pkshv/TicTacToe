// Assets/_Project/Scripts/Game/Enums/CellState.cs

namespace TicTacToe.Game.Enums
{
    /// <summary>
    /// Состояние ячейки игрового поля.
    /// </summary>
    public enum CellState
    {
        /// <summary>
        /// Ячейка пуста (не занята).
        /// </summary>
        Empty = 0,
        
        /// <summary>
        /// Ячейка занята крестиком (X).
        /// </summary>
        X = 1,
        
        /// <summary>
        /// Ячейка занята ноликом (O).
        /// </summary>
        O = 2
    }
}
