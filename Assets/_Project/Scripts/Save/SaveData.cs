// Assets/_Project/Scripts/Save/SaveData.cs

using System;
using System.Collections.Generic;

namespace TicTacToe.Save
{
    /// <summary>
    /// Основная структура данных сохранения игры.
    /// Содержит всю персистентную информацию: покупки, настройки, статистику.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Текущая версия формата сохранения.
        /// Увеличивается при изменении структуры для поддержки миграции.
        /// </summary>
        public const int CURRENT_VERSION = 1;
        
        /// <summary>
        /// Версия данных сохранения (для миграции старых сохранений)
        /// </summary>
        public int Version = CURRENT_VERSION;
        
        /// <summary>
        /// Уникальный идентификатор игрока (GUID)
        /// </summary>
        public string PlayerId;
        
        /// <summary>
        /// Дата первого запуска игры (ISO 8601 формат)
        /// </summary>
        public string FirstLaunchDate;
        
        /// <summary>
        /// Данные о покупках (IAP)
        /// </summary>
        public PurchaseData Purchases = new PurchaseData();
        
        /// <summary>
        /// Пользовательские настройки
        /// </summary>
        public SettingsData Settings = new SettingsData();
        
        /// <summary>
        /// Игровая статистика
        /// </summary>
        public StatisticsData Statistics = new StatisticsData();
        
        /// <summary>
        /// Данные для управления рекламой
        /// </summary>
        public AdsData Ads = new AdsData();
        
        /// <summary>
        /// Создаёт новые данные сохранения со значениями по умолчанию
        /// </summary>
        public static SaveData CreateDefault()
        {
            return new SaveData
            {
                Version = CURRENT_VERSION,
                PlayerId = Guid.NewGuid().ToString(),
                FirstLaunchDate = DateTime.UtcNow.ToString("o"),
                Purchases = new PurchaseData(),
                Settings = new SettingsData(),
                Statistics = new StatisticsData(),
                Ads = new AdsData()
            };
        }
        
        /// <summary>
        /// Создаёт глубокую копию данных сохранения
        /// </summary>
        public SaveData Clone()
        {
            return new SaveData
            {
                Version = Version,
                PlayerId = PlayerId,
                FirstLaunchDate = FirstLaunchDate,
                Purchases = Purchases.Clone(),
                Settings = Settings.Clone(),
                Statistics = Statistics.Clone(),
                Ads = Ads.Clone()
            };
        }
    }
    
    /// <summary>
    /// Данные о покупках игрока (IAP)
    /// </summary>
    [Serializable]
    public class PurchaseData
    {
        /// <summary>
        /// Идентификатор темы по умолчанию
        /// </summary>
        public const string DEFAULT_THEME = "theme_classic";
        
        /// <summary>
        /// Список ID купленных тем. Classic включена по умолчанию.
        /// </summary>
        public List<string> OwnedThemes = new List<string> { DEFAULT_THEME };
        
        /// <summary>
        /// Флаг отключения рекламы (покупка "Remove Ads")
        /// </summary>
        public bool AdsRemoved = false;
        
        /// <summary>
        /// Проверяет, куплена ли указанная тема
        /// </summary>
        public bool HasTheme(string themeId)
        {
            return OwnedThemes != null && OwnedThemes.Contains(themeId);
        }
        
        /// <summary>
        /// Добавляет тему в список купленных
        /// </summary>
        public void AddTheme(string themeId)
        {
            if (OwnedThemes == null)
            {
                OwnedThemes = new List<string>();
            }
            
            if (!OwnedThemes.Contains(themeId))
            {
                OwnedThemes.Add(themeId);
            }
        }
        
        /// <summary>
        /// Создаёт копию данных о покупках
        /// </summary>
        public PurchaseData Clone()
        {
            return new PurchaseData
            {
                OwnedThemes = OwnedThemes != null 
                    ? new List<string>(OwnedThemes) 
                    : new List<string> { DEFAULT_THEME },
                AdsRemoved = AdsRemoved
            };
        }
    }
    
    /// <summary>
    /// Пользовательские настройки игры
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        /// <summary>
        /// ID выбранной темы оформления
        /// </summary>
        public string SelectedTheme = PurchaseData.DEFAULT_THEME;
        
        /// <summary>
        /// Громкость музыки (0.0 - 1.0)
        /// </summary>
        public float MusicVolume = 0.8f;
        
        /// <summary>
        /// Громкость звуковых эффектов (0.0 - 1.0)
        /// </summary>
        public float SfxVolume = 1.0f;
        
        /// <summary>
        /// Включена ли вибрация (только мобильные платформы)
        /// </summary>
        public bool VibrationEnabled = true;
        
        /// <summary>
        /// Код языка интерфейса (ISO 639-1)
        /// </summary>
        public string Language = "en";
        
        /// <summary>
        /// Включена ли музыка
        /// </summary>
        public bool MusicEnabled = true;
        
        /// <summary>
        /// Включены ли звуковые эффекты
        /// </summary>
        public bool SfxEnabled = true;
        
        /// <summary>
        /// Создаёт копию настроек
        /// </summary>
        public SettingsData Clone()
        {
            return new SettingsData
            {
                SelectedTheme = SelectedTheme,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                VibrationEnabled = VibrationEnabled,
                Language = Language,
                MusicEnabled = MusicEnabled,
                SfxEnabled = SfxEnabled
            };
        }
    }
    
    /// <summary>
    /// Игровая статистика
    /// </summary>
    [Serializable]
    public class StatisticsData
    {
        /// <summary>
        /// Количество уровней сложности AI
        /// </summary>
        private const int AI_DIFFICULTY_COUNT = 3;
        
        /// <summary>
        /// Общее количество сыгранных игр
        /// </summary>
        public int TotalGames = 0;
        
        /// <summary>
        /// Победы против AI по уровням сложности.
        /// Индексы: 0 = Easy, 1 = Medium, 2 = Hard
        /// </summary>
        public int[] WinsVsAI = new int[AI_DIFFICULTY_COUNT];
        
        /// <summary>
        /// Поражения против AI по уровням сложности
        /// </summary>
        public int[] LossesVsAI = new int[AI_DIFFICULTY_COUNT];
        
        /// <summary>
        /// Ничьи против AI по уровням сложности
        /// </summary>
        public int[] DrawsVsAI = new int[AI_DIFFICULTY_COUNT];
        
        /// <summary>
        /// Победы в локальном мультиплеере (Player X)
        /// </summary>
        public int WinsLocalMultiplayer = 0;
        
        /// <summary>
        /// Победы в сетевом мультиплеере
        /// </summary>
        public int WinsNetworkMultiplayer = 0;
        
        /// <summary>
        /// Общее количество поражений (legacy, для обратной совместимости)
        /// </summary>
        public int Losses = 0;
        
        /// <summary>
        /// Общее количество ничьих (legacy)
        /// </summary>
        public int Draws = 0;
        
        /// <summary>
        /// Текущая серия побед
        /// </summary>
        public int CurrentWinStreak = 0;
        
        /// <summary>
        /// Лучшая серия побед
        /// </summary>
        public int BestWinStreak = 0;
        
        /// <summary>
        /// Общее количество побед во всех режимах
        /// </summary>
        public int TotalWins
        {
            get
            {
                int aiWins = 0;
                if (WinsVsAI != null)
                {
                    for (int i = 0; i < WinsVsAI.Length; i++)
                    {
                        aiWins += WinsVsAI[i];
                    }
                }
                return aiWins + WinsLocalMultiplayer + WinsNetworkMultiplayer;
            }
        }
        
        /// <summary>
        /// Общее количество поражений во всех режимах
        /// </summary>
        public int TotalLosses
        {
            get
            {
                int aiLosses = 0;
                if (LossesVsAI != null)
                {
                    for (int i = 0; i < LossesVsAI.Length; i++)
                    {
                        aiLosses += LossesVsAI[i];
                    }
                }
                return aiLosses + Losses;
            }
        }
        
        /// <summary>
        /// Общее количество ничьих во всех режимах
        /// </summary>
        public int TotalDraws
        {
            get
            {
                int aiDraws = 0;
                if (DrawsVsAI != null)
                {
                    for (int i = 0; i < DrawsVsAI.Length; i++)
                    {
                        aiDraws += DrawsVsAI[i];
                    }
                }
                return aiDraws + Draws;
            }
        }
        
        /// <summary>
        /// Процент побед (0-100)
        /// </summary>
        public float WinRate
        {
            get
            {
                if (TotalGames == 0) return 0f;
                return (float)TotalWins / TotalGames * 100f;
            }
        }
        
        /// <summary>
        /// Записывает результат игры против AI
        /// </summary>
        /// <param name="difficultyIndex">Индекс сложности (0-2)</param>
        /// <param name="isWin">Победа игрока</param>
        /// <param name="isDraw">Ничья</param>
        public void RecordAIGame(int difficultyIndex, bool isWin, bool isDraw)
        {
            if (difficultyIndex < 0 || difficultyIndex >= AI_DIFFICULTY_COUNT)
            {
                UnityEngine.Debug.LogWarning($"Invalid AI difficulty index: {difficultyIndex}");
                return;
            }
            
            EnsureArraysInitialized();
            
            TotalGames++;
            
            if (isWin)
            {
                WinsVsAI[difficultyIndex]++;
                CurrentWinStreak++;
                if (CurrentWinStreak > BestWinStreak)
                {
                    BestWinStreak = CurrentWinStreak;
                }
            }
            else if (isDraw)
            {
                DrawsVsAI[difficultyIndex]++;
                // Серия побед продолжается при ничьей
            }
            else
            {
                LossesVsAI[difficultyIndex]++;
                CurrentWinStreak = 0;
            }
        }
        
        /// <summary>
        /// Записывает результат локальной мультиплеерной игры
        /// </summary>
        /// <param name="playerXWon">Победил ли Player X</param>
        /// <param name="isDraw">Ничья</param>
        public void RecordLocalMultiplayerGame(bool playerXWon, bool isDraw)
        {
            TotalGames++;
            
            if (isDraw)
            {
                Draws++;
            }
            else if (playerXWon)
            {
                WinsLocalMultiplayer++;
            }
            // Поражения Player O не отслеживаются отдельно
        }
        
        /// <summary>
        /// Записывает результат сетевой мультиплеерной игры
        /// </summary>
        public void RecordNetworkMultiplayerGame(bool isWin, bool isDraw)
        {
            TotalGames++;
            
            if (isWin)
            {
                WinsNetworkMultiplayer++;
                CurrentWinStreak++;
                if (CurrentWinStreak > BestWinStreak)
                {
                    BestWinStreak = CurrentWinStreak;
                }
            }
            else if (isDraw)
            {
                Draws++;
            }
            else
            {
                Losses++;
                CurrentWinStreak = 0;
            }
        }
        
        /// <summary>
        /// Гарантирует инициализацию массивов
        /// </summary>
        private void EnsureArraysInitialized()
        {
            if (WinsVsAI == null || WinsVsAI.Length != AI_DIFFICULTY_COUNT)
            {
                WinsVsAI = new int[AI_DIFFICULTY_COUNT];
            }
            if (LossesVsAI == null || LossesVsAI.Length != AI_DIFFICULTY_COUNT)
            {
                LossesVsAI = new int[AI_DIFFICULTY_COUNT];
            }
            if (DrawsVsAI == null || DrawsVsAI.Length != AI_DIFFICULTY_COUNT)
            {
                DrawsVsAI = new int[AI_DIFFICULTY_COUNT];
            }
        }
        
        /// <summary>
        /// Создаёт копию статистики
        /// </summary>
        public StatisticsData Clone()
        {
            var clone = new StatisticsData
            {
                TotalGames = TotalGames,
                WinsVsAI = WinsVsAI != null ? (int[])WinsVsAI.Clone() : new int[AI_DIFFICULTY_COUNT],
                LossesVsAI = LossesVsAI != null ? (int[])LossesVsAI.Clone() : new int[AI_DIFFICULTY_COUNT],
                DrawsVsAI = DrawsVsAI != null ? (int[])DrawsVsAI.Clone() : new int[AI_DIFFICULTY_COUNT],
                WinsLocalMultiplayer = WinsLocalMultiplayer,
                WinsNetworkMultiplayer = WinsNetworkMultiplayer,
                Losses = Losses,
                Draws = Draws,
                CurrentWinStreak = CurrentWinStreak,
                BestWinStreak = BestWinStreak
            };
            return clone;
        }
    }
    
    /// <summary>
    /// Данные для управления показом рекламы
    /// </summary>
    [Serializable]
    public class AdsData
    {
        /// <summary>
        /// Счётчик матчей для показа interstitial рекламы
        /// </summary>
        public int MatchCounter = 0;
        
        /// <summary>
        /// Флаг: первая сессия завершена (для отложенного показа рекламы)
        /// </summary>
        public bool FirstSessionCompleted = false;
        
        /// <summary>
        /// Время последнего показа interstitial (Unix timestamp)
        /// </summary>
        public long LastInterstitialTimestamp = 0;
        
        /// <summary>
        /// Общее количество показанных interstitial
        /// </summary>
        public int TotalInterstitialsShown = 0;
        
        /// <summary>
        /// Общее количество показанных rewarded видео
        /// </summary>
        public int TotalRewardedShown = 0;
        
        /// <summary>
        /// Инкрементирует счётчик матчей
        /// </summary>
        public void IncrementMatchCounter()
        {
            MatchCounter++;
        }
        
        /// <summary>
        /// Сбрасывает счётчик матчей
        /// </summary>
        public void ResetMatchCounter()
        {
            MatchCounter = 0;
        }
        
        /// <summary>
        /// Записывает показ interstitial
        /// </summary>
        public void RecordInterstitialShown()
        {
            TotalInterstitialsShown++;
            LastInterstitialTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ResetMatchCounter();
        }
        
        /// <summary>
        /// Записывает показ rewarded видео
        /// </summary>
        public void RecordRewardedShown()
        {
            TotalRewardedShown++;
        }
        
        /// <summary>
        /// Создаёт копию данных рекламы
        /// </summary>
        public AdsData Clone()
        {
            return new AdsData
            {
                MatchCounter = MatchCounter,
                FirstSessionCompleted = FirstSessionCompleted,
                LastInterstitialTimestamp = LastInterstitialTimestamp,
                TotalInterstitialsShown = TotalInterstitialsShown,
                TotalRewardedShown = TotalRewardedShown
            };
        }
    }
}
