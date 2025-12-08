// Assets/_Project/Scripts/Utils/Constants.cs

namespace TicTacToe.Utils
{
    /// <summary>
    /// Глобальные константы проекта
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Константы игрового поля
        /// </summary>
        public static class Board
        {
            /// <summary>
            /// Размер поля (3x3)
            /// </summary>
            public const int SIZE = 3;
            
            /// <summary>
            /// Общее количество ячеек
            /// </summary>
            public const int CELL_COUNT = SIZE * SIZE;
            
            /// <summary>
            /// Индекс центральной ячейки
            /// </summary>
            public const int CENTER_INDEX = 4;
            
            /// <summary>
            /// Индексы угловых ячеек
            /// </summary>
            public static readonly int[] CORNER_INDICES = { 0, 2, 6, 8 };
            
            /// <summary>
            /// Индексы боковых ячеек
            /// </summary>
            public static readonly int[] EDGE_INDICES = { 1, 3, 5, 7 };
        }
        
        /// <summary>
        /// Идентификаторы экранов
        /// </summary>
        public static class Screens
        {
            public const string MAIN_MENU = "MainMenu";
            public const string DIFFICULTY = "Difficulty";
            public const string GAME = "Game";
            public const string SETTINGS = "Settings";
            public const string SHOP = "Shop";
            public const string LOBBY = "Lobby";
            public const string STATISTICS = "Statistics";
        }
        
        /// <summary>
        /// Идентификаторы popup-ов
        /// </summary>
        public static class Popups
        {
            public const string RESULT = "Result";
            public const string PAUSE = "Pause";
            public const string CONFIRM = "Confirm";
            public const string LOADING = "Loading";
            public const string ERROR = "Error";
            public const string PURCHASE = "Purchase";
            public const string REWARD = "Reward";
        }
        
        /// <summary>
        /// Настройки анимаций
        /// </summary>
        public static class Animation
        {
            /// <summary>
            /// Длительность fade анимации экранов (сек)
            /// </summary>
            public const float SCREEN_FADE_DURATION = 0.3f;
            
            /// <summary>
            /// Длительность анимации появления символа (сек)
            /// </summary>
            public const float SYMBOL_APPEAR_DURATION = 0.2f;
            
            /// <summary>
            /// Длительность анимации подсветки победной линии (сек)
            /// </summary>
            public const float WIN_LINE_DURATION = 0.5f;
            
            /// <summary>
            /// Длительность появления popup (сек)
            /// </summary>
            public const float POPUP_APPEAR_DURATION = 0.25f;
            
            /// <summary>
            /// Длительность анимации кнопки при нажатии (сек)
            /// </summary>
            public const float BUTTON_PRESS_DURATION = 0.1f;
        }
        
        /// <summary>
        /// Константы AI
        /// </summary>
        public static class AI
        {
            /// <summary>
            /// Задержка перед ходом AI (сек) — минимум
            /// </summary>
            public const float MOVE_DELAY_MIN = 0.3f;
            
            /// <summary>
            /// Задержка перед ходом AI (сек) — максимум
            /// </summary>
            public const float MOVE_DELAY_MAX = 0.8f;
            
            /// <summary>
            /// Вероятность умного хода для Medium AI (0-1)
            /// </summary>
            public const float MEDIUM_SMART_CHANCE = 0.7f;
            
            /// <summary>
            /// Максимальная глубина поиска Minimax
            /// </summary>
            public const int MINIMAX_MAX_DEPTH = 9;
            
            /// <summary>
            /// Количество уровней сложности
            /// </summary>
            public const int DIFFICULTY_COUNT = 3;
        }
        
        /// <summary>
        /// Константы сети
        /// </summary>
        public static class Network
        {
            /// <summary>
            /// Порт для UDP broadcast (LAN discovery)
            /// </summary>
            public const int DISCOVERY_PORT = 47777;
            
            /// <summary>
            /// Порт для игрового сервера
            /// </summary>
            public const int GAME_PORT = 47778;
            
            /// <summary>
            /// Интервал broadcast сообщений (сек)
            /// </summary>
            public const float BROADCAST_INTERVAL = 1.0f;
            
            /// <summary>
            /// Таймаут поиска серверов (сек)
            /// </summary>
            public const float DISCOVERY_TIMEOUT = 5.0f;
            
            /// <summary>
            /// Таймаут ожидания хода противника (сек)
            /// </summary>
            public const float MOVE_TIMEOUT = 30.0f;
            
            /// <summary>
            /// Время ожидания реконнекта (сек)
            /// </summary>
            public const float RECONNECT_TIMEOUT = 30.0f;
            
            /// <summary>
            /// Максимальное количество попыток реконнекта
            /// </summary>
            public const int MAX_RECONNECT_ATTEMPTS = 3;
        }
        
        /// <summary>
        /// Константы рекламы
        /// </summary>
        public static class Ads
        {
            /// <summary>
            /// Количество матчей между показами interstitial
            /// </summary>
            public const int MATCHES_BETWEEN_INTERSTITIAL = 3;
            
            /// <summary>
            /// Минимальный интервал между interstitial (сек)
            /// </summary>
            public const int MIN_INTERSTITIAL_INTERVAL = 60;
            
            /// <summary>
            /// Задержка перед первым показом рекламы (матчей)
            /// </summary>
            public const int FIRST_AD_DELAY_MATCHES = 2;
            
            /// <summary>
            /// Высота баннера по умолчанию (dp)
            /// </summary>
            public const float DEFAULT_BANNER_HEIGHT = 50f;
            
            /// <summary>
            /// Высота баннера для планшетов (dp)
            /// </summary>
            public const float TABLET_BANNER_HEIGHT = 90f;
        }
        
        /// <summary>
        /// Константы системы сохранений
        /// </summary>
        public static class Save
        {
            /// <summary>
            /// Имя файла сохранения
            /// </summary>
            public const string FILE_NAME = "tictactoe_save.json";
            
            /// <summary>
            /// Имя файла резервной копии
            /// </summary>
            public const string BACKUP_FILE_NAME = "tictactoe_save_backup.json";
            
            /// <summary>
            /// Ключ для PlayerPrefs (WebGL)
            /// </summary>
            public const string PLAYER_PREFS_KEY = "TicTacToe_SaveData";
            
            /// <summary>
            /// Ключ для облачного сохранения
            /// </summary>
            public const string CLOUD_KEY = "player_save";
            
            /// <summary>
            /// Интервал автосохранения (сек)
            /// </summary>
            public const float AUTO_SAVE_INTERVAL = 30f;
            
            /// <summary>
            /// Максимальное количество попыток облачного сохранения
            /// </summary>
            public const int MAX_CLOUD_RETRY_ATTEMPTS = 3;
            
            /// <summary>
            /// Задержка между попытками облачного сохранения (сек)
            /// </summary>
            public const float CLOUD_RETRY_DELAY = 1f;
        }
        
        /// <summary>
        /// Идентификаторы тем оформления
        /// </summary>
        public static class Themes
        {
            public const string CLASSIC = "theme_classic";
            public const string NEON = "theme_neon";
            public const string NATURE = "theme_nature";
            public const string MINIMAL = "theme_minimal";
            public const string SPACE = "theme_space";
            public const string CYBERPUNK = "theme_cyberpunk";
            public const string RETRO = "theme_retro";
            
            /// <summary>
            /// Тема по умолчанию (бесплатная)
            /// </summary>
            public const string DEFAULT = CLASSIC;
        }
        
        /// <summary>
        /// Идентификаторы IAP продуктов
        /// </summary>
        public static class Products
        {
            public const string REMOVE_ADS = "com.yourcompany.tictactoe.removeads";
            public const string THEME_NEON = "com.yourcompany.tictactoe.theme_neon";
            public const string THEME_NATURE = "com.yourcompany.tictactoe.theme_nature";
            public const string THEME_MINIMAL = "com.yourcompany.tictactoe.theme_minimal";
            public const string THEME_SPACE = "com.yourcompany.tictactoe.theme_space";
            public const string THEME_CYBERPUNK = "com.yourcompany.tictactoe.theme_cyberpunk";
            public const string THEME_RETRO = "com.yourcompany.tictactoe.theme_retro";
            public const string THEME_BUNDLE = "com.yourcompany.tictactoe.theme_bundle";
        }
        
        /// <summary>
        /// Константы аудио
        /// </summary>
        public static class Audio
        {
            /// <summary>
            /// Громкость музыки по умолчанию (0-1)
            /// </summary>
            public const float DEFAULT_MUSIC_VOLUME = 0.8f;
            
            /// <summary>
            /// Громкость звуков по умолчанию (0-1)
            /// </summary>
            public const float DEFAULT_SFX_VOLUME = 1.0f;
            
            /// <summary>
            /// Длительность fade музыки (сек)
            /// </summary>
            public const float MUSIC_FADE_DURATION = 0.5f;
        }
        
        /// <summary>
        /// Константы аналитики
        /// </summary>
        public static class Analytics
        {
            public const string EVENT_GAME_START = "game_start";
            public const string EVENT_GAME_END = "game_end";
            public const string EVENT_AD_SHOWN = "ad_shown";
            public const string EVENT_AD_CLICKED = "ad_clicked";
            public const string EVENT_PURCHASE = "purchase";
            public const string EVENT_THEME_SELECTED = "theme_selected";
            public const string EVENT_DIFFICULTY_SELECTED = "difficulty_selected";
            
            public const string PARAM_GAME_MODE = "game_mode";
            public const string PARAM_DIFFICULTY = "difficulty";
            public const string PARAM_RESULT = "result";
            public const string PARAM_DURATION = "duration";
            public const string PARAM_AD_TYPE = "ad_type";
            public const string PARAM_PRODUCT_ID = "product_id";
            public const string PARAM_THEME_ID = "theme_id";
        }
        
        /// <summary>
        /// Слои сортировки UI
        /// </summary>
        public static class SortingLayers
        {
            public const string BACKGROUND = "Background";
            public const string DEFAULT = "Default";
            public const string FOREGROUND = "Foreground";
            public const string UI = "UI";
            public const string POPUP = "Popup";
            public const string OVERLAY = "Overlay";
        }
        
        /// <summary>
        /// Теги игровых объектов
        /// </summary>
        public static class Tags
        {
            public const string GAME_MANAGER = "GameManager";
            public const string UI_MANAGER = "UIManager";
            public const string AUDIO_MANAGER = "AudioManager";
        }
    }
}
