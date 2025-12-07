// Assets/_Project/Scripts/Utils/Constants.cs

namespace TicTacToe.Utils
{
    /// <summary>
    /// Глобальные константы проекта
    /// </summary>
    public static class Constants
    {
        // ===== ИГРОВОЕ ПОЛЕ =====
        
        /// <summary>
        /// Размер игрового поля (3x3)
        /// </summary>
        public const int BOARD_SIZE = 3;
        
        /// <summary>
        /// Общее количество клеток на поле
        /// </summary>
        public const int TOTAL_CELLS = BOARD_SIZE * BOARD_SIZE;

        // ===== СЕТЬ =====
        
        /// <summary>
        /// Порт для UDP Broadcast при поиске игр в локальной сети
        /// </summary>
        public const int LAN_DISCOVERY_PORT = 47777;
        
        /// <summary>
        /// Таймаут ожидания переподключения (секунды)
        /// </summary>
        public const float RECONNECT_TIMEOUT = 30.0f;
        
        /// <summary>
        /// Интервал пинга для проверки соединения (секунды)
        /// </summary>
        public const float PING_INTERVAL = 2.0f;
        
        /// <summary>
        /// Таймаут пинга (секунды)
        /// </summary>
        public const float PING_TIMEOUT = 5.0f;

        // ===== РЕКЛАМА =====
        
        /// <summary>
        /// Частота показа interstitial рекламы (каждые N матчей)
        /// </summary>
        public const int DEFAULT_INTERSTITIAL_FREQUENCY = 3;
        
        /// <summary>
        /// Минимальный интервал между показами рекламы (секунды)
        /// </summary>
        public const float MIN_INTERSTITIAL_INTERVAL = 60f;
        
        /// <summary>
        /// Максимум показов interstitial за сессию
        /// </summary>
        public const int MAX_INTERSTITIALS_PER_SESSION = 5;
        
        /// <summary>
        /// Высота баннера по умолчанию (dp)
        /// </summary>
        public const float DEFAULT_BANNER_HEIGHT = 50f;

        // ===== АНИМАЦИИ =====
        
        /// <summary>
        /// Длительность анимации появления символа (секунды)
        /// </summary>
        public const float SYMBOL_APPEAR_DURATION = 0.2f;
        
        /// <summary>
        /// Длительность анимации линии победы (секунды)
        /// </summary>
        public const float WIN_LINE_DURATION = 0.3f;
        
        /// <summary>
        /// Задержка перед показом результата игры (секунды)
        /// </summary>
        public const float GAME_OVER_DELAY = 0.5f;

        // ===== AI =====
        
        /// <summary>
        /// Минимальная задержка хода AI (секунды)
        /// </summary>
        public const float AI_MIN_DELAY = 0.3f;
        
        /// <summary>
        /// Максимальная задержка хода AI (секунды)
        /// </summary>
        public const float AI_MAX_DELAY = 1.0f;
        
        /// <summary>
        /// Вероятность умного хода для Medium AI (0-1)
        /// </summary>
        public const float MEDIUM_AI_SMART_MOVE_CHANCE = 0.7f;

        // ===== СОХРАНЕНИЯ =====
        
        /// <summary>
        /// Имя файла сохранения
        /// </summary>
        public const string SAVE_FILE_NAME = "tictactoe_save.json";
        
        /// <summary>
        /// Текущая версия формата сохранения
        /// </summary>
        public const int SAVE_VERSION = 1;

        // ===== ТЕМЫ =====
        
        /// <summary>
        /// ID темы по умолчанию (бесплатная)
        /// </summary>
        public const string DEFAULT_THEME_ID = "theme_classic";

        // ===== СЦЕНЫ =====
        
        /// <summary>
        /// Название сцены Bootstrap
        /// </summary>
        public const string SCENE_BOOTSTRAP = "Bootstrap";
        
        /// <summary>
        /// Название сцены главного меню
        /// </summary>
        public const string SCENE_MAIN_MENU = "MainMenu";
        
        /// <summary>
        /// Название игровой сцены
        /// </summary>
        public const string SCENE_GAME = "Game";

        // ===== UI =====
        
        /// <summary>
        /// Референсное разрешение для UI (ширина)
        /// </summary>
        public const int UI_REFERENCE_WIDTH = 1080;
        
        /// <summary>
        /// Референсное разрешение для UI (высота)
        /// </summary>
        public const int UI_REFERENCE_HEIGHT = 1920;

        // ===== АУДИО =====
        
        /// <summary>
        /// Громкость музыки по умолчанию
        /// </summary>
        public const float DEFAULT_MUSIC_VOLUME = 0.8f;
        
        /// <summary>
        /// Громкость звуков по умолчанию
        /// </summary>
        public const float DEFAULT_SFX_VOLUME = 1.0f;

        // ===== PLAYERPREFS КЛЮЧИ =====
        
        public static class PlayerPrefsKeys
        {
            public const string MUSIC_VOLUME = "MusicVolume";
            public const string SFX_VOLUME = "SfxVolume";
            public const string VIBRATION_ENABLED = "VibrationEnabled";
            public const string SELECTED_THEME = "SelectedTheme";
            public const string LANGUAGE = "Language";
            public const string FIRST_LAUNCH = "FirstLaunch";
            public const string ADS_REMOVED = "AdsRemoved";
        }

        // ===== АНАЛИТИКА =====
        
        public static class AnalyticsEvents
        {
            public const string GAME_STARTED = "game_started";
            public const string GAME_ENDED = "game_ended";
            public const string THEME_PURCHASED = "theme_purchased";
            public const string ADS_REMOVED = "ads_removed";
            public const string AD_WATCHED = "ad_watched";
            public const string LEVEL_SELECTED = "level_selected";
        }
    }
}
