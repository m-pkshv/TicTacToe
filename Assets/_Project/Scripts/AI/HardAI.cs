// Assets/_Project/Scripts/AI/HardAI.cs

using System;
using System.Collections.Generic;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;

namespace TicTacToe.AI
{
    /// <summary>
    /// Сложный ИИ: алгоритм Minimax с альфа-бета отсечением.
    /// Непобедимый — всегда играет оптимально.
    /// Максимум — ничья, если игрок тоже играет идеально.
    /// </summary>
    public class HardAI : IAIPlayer
    {
        private const int SCORE_WIN = 10;
        private const int SCORE_LOSE = -10;
        private const int SCORE_DRAW = 0;
        
        private CellState _aiSymbol;
        private CellState _playerSymbol;
        
        /// <inheritdoc/>
        public AIDifficulty Difficulty => AIDifficulty.Hard;
        
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
            
            _aiSymbol = aiSymbol;
            _playerSymbol = aiSymbol == CellState.X ? CellState.O : CellState.X;
            
            // Оптимизация первого хода: центр или угол (для ускорения)
            if (emptyCells.Count == BoardModel.TOTAL_CELLS)
            {
                // Первый ход — центр оптимален
                return 4;
            }
            
            if (emptyCells.Count == BoardModel.TOTAL_CELLS - 1)
            {
                // Второй ход — если центр занят, выбираем угол; иначе — центр
                return board.IsCellEmpty(4) ? 4 : 0;
            }
            
            int bestScore = int.MinValue;
            int bestMove = -1;
            
            foreach (int index in emptyCells)
            {
                // Клонируем доску и делаем ход
                BoardModel clone = board.Clone();
                clone.MakeMove(index, _aiSymbol);
                
                // Оцениваем этот ход с помощью Minimax
                int score = Minimax(clone, 0, false, int.MinValue, int.MaxValue);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = index;
                }
            }
            
            return bestMove;
        }
        
        /// <summary>
        /// Алгоритм Minimax с альфа-бета отсечением.
        /// </summary>
        /// <param name="board">Текущее состояние поля</param>
        /// <param name="depth">Глубина рекурсии (для оценки быстрых побед)</param>
        /// <param name="isMaximizing">true если ход ИИ, false если ход игрока</param>
        /// <param name="alpha">Лучший результат для максимизирующего игрока</param>
        /// <param name="beta">Лучший результат для минимизирующего игрока</param>
        /// <returns>Оценка позиции</returns>
        private int Minimax(BoardModel board, int depth, bool isMaximizing, int alpha, int beta)
        {
            // Проверка терминальных состояний
            GameResult result = board.CheckWin();
            
            if (result == GameResult.XWins)
            {
                // X победил: хорошо для ИИ если он X, плохо если O
                return _aiSymbol == CellState.X ? SCORE_WIN - depth : SCORE_LOSE + depth;
            }
            
            if (result == GameResult.OWins)
            {
                // O победил: хорошо для ИИ если он O, плохо если X
                return _aiSymbol == CellState.O ? SCORE_WIN - depth : SCORE_LOSE + depth;
            }
            
            if (board.CheckDraw())
            {
                return SCORE_DRAW;
            }
            
            List<int> emptyCells = board.GetEmptyCells();
            
            if (isMaximizing)
            {
                // Ход ИИ — максимизируем счёт
                int maxScore = int.MinValue;
                
                foreach (int index in emptyCells)
                {
                    BoardModel clone = board.Clone();
                    clone.MakeMove(index, _aiSymbol);
                    
                    int score = Minimax(clone, depth + 1, false, alpha, beta);
                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    
                    // Бета-отсечение
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                
                return maxScore;
            }
            else
            {
                // Ход игрока — минимизируем счёт (для ИИ)
                int minScore = int.MaxValue;
                
                foreach (int index in emptyCells)
                {
                    BoardModel clone = board.Clone();
                    clone.MakeMove(index, _playerSymbol);
                    
                    int score = Minimax(clone, depth + 1, true, alpha, beta);
                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    
                    // Альфа-отсечение
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                
                return minScore;
            }
        }
    }
}
