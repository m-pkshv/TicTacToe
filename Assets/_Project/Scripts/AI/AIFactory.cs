// Assets/_Project/Scripts/AI/AIFactory.cs

using System;
using TicTacToe.Game.Enums;

namespace TicTacToe.AI
{
    /// <summary>
    /// Фабрика для создания ИИ игроков.
    /// Использует паттерн Factory для инкапсуляции логики создания.
    /// </summary>
    public static class AIFactory
    {
        /// <summary>
        /// Создаёт экземпляр ИИ по заданному уровню сложности.
        /// </summary>
        /// <param name="difficulty">Уровень сложности ИИ</param>
        /// <returns>Экземпляр ИИ игрока</returns>
        /// <exception cref="ArgumentException">Если указан неизвестный уровень сложности</exception>
        public static IAIPlayer Create(AIDifficulty difficulty)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => new EasyAI(),
                AIDifficulty.Medium => new MediumAI(),
                AIDifficulty.Hard => new HardAI(),
                _ => throw new ArgumentException($"Unknown AI difficulty: {difficulty}", nameof(difficulty))
            };
        }
        
        /// <summary>
        /// Создаёт экземпляр ИИ с заданным seed для генератора случайных чисел.
        /// Полезно для тестирования и воспроизведения результатов.
        /// </summary>
        /// <param name="difficulty">Уровень сложности ИИ</param>
        /// <param name="seed">Seed для генератора случайных чисел</param>
        /// <returns>Экземпляр ИИ игрока</returns>
        /// <exception cref="ArgumentException">Если указан неизвестный уровень сложности</exception>
        public static IAIPlayer Create(AIDifficulty difficulty, int seed)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => new EasyAI(seed),
                AIDifficulty.Medium => new MediumAI(seed),
                AIDifficulty.Hard => new HardAI(), // HardAI детерминированный, seed не нужен
                _ => throw new ArgumentException($"Unknown AI difficulty: {difficulty}", nameof(difficulty))
            };
        }
        
        /// <summary>
        /// Получает отображаемое имя уровня сложности.
        /// </summary>
        /// <param name="difficulty">Уровень сложности</param>
        /// <returns>Локализованное имя (пока на английском)</returns>
        public static string GetDifficultyName(AIDifficulty difficulty)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => "Easy",
                AIDifficulty.Medium => "Medium",
                AIDifficulty.Hard => "Hard",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// Получает описание уровня сложности.
        /// </summary>
        /// <param name="difficulty">Уровень сложности</param>
        /// <returns>Описание поведения ИИ</returns>
        public static string GetDifficultyDescription(AIDifficulty difficulty)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => "Random moves. Perfect for beginners.",
                AIDifficulty.Medium => "Smart moves with some randomness. A fair challenge.",
                AIDifficulty.Hard => "Unbeatable. Best possible moves every time.",
                _ => "Unknown difficulty level."
            };
        }
    }
}
