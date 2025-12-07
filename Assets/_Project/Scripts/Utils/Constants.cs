// Assets/_Project/Scripts/Utils/Constants.cs

namespace TicTacToe.Utils
{
    /// <summary>
    /// Глобальные константы проекта.
    /// Содержит все магические числа, строки и настройки.
    /// </summary>
    public static class Constants
    {
        // ==================== ИГРОВОЕ ПОЛЕ ====================
        
        /// <summary>Размер игрового поля (3x3)</summary>
        public const int BOARD_SIZE = 3;
        
        /// <summary>Общее количество клеток</summary>
        public const int TOTAL_CELLS = BOARD_SIZE * BOARD_SIZE;
        
        // ==================== ИГРОВЫЕ НАСТРОЙКИ ====================
        
        /// <summary>Минимальная задержка хода AI (секунды)</summary>
        public const float AI_MIN_DELAY = 0.3f;
        
        /// <summary>Максимальная задержка хода AI (секунды)</summary>
        public const float AI_MAX_DELAY = 0.8f;
        
        /// <summary>Таймаут хода в сетевой игре (секунды)</summary>
        public const float NETWORK_TURN_TIMEOUT = 30f;
        
        /// <summary>Таймаут переподключения (секунды)</summary>
        public const float NETWORK_RECONNECT_TIMEOUT = 30f;
        
        // ==================== СЕТЬ ====================
        
        /// <summary>Порт для LAN Discovery (UDP Broadcast)</summary>
        public const int LAN_DISCOVERY_PORT = 47777;
        
        /// <summary>Интервал поиска игр в LAN (секунды)</summary>
        public const float LAN_DISCOVERY_INTERVAL = 2f;
        
        /// <summary>Максимальное количество игроков</summary>
        public const int MAX_PLAYERS = 2;
        
        // ==================== UI / АНИМАЦИИ ====================
        
        /// <summary>Длительность анимации перехода между экранами</summary>
        public const float SCREEN_TRANSITION_DURATION = 0.3f;
        
        /// <summary>Длительность анимации появления символа</summary>
        public const float SYMBOL_APPEAR_DURATION = 0.2f;
        
        /// <summary>Длительность анимации линии победы</summary>
        public const float WIN_LINE_DURATION = 0.5f;
        
        /// <summary>Задержка перед показом результата</summary>
        public const float RESULT_DELAY = 0.8f;
        
        // ==================== РЕКЛАМА ====================
        
        /// <summary>Частота показа Interstitial (каждые N матчей)</summary>
        public const int INTERSTITIAL_FREQUENCY = 3;
        
        /// <summary>Минимальный интервал между Interstitial (секунды)</summary>
        public const float INTERSTITIAL_MIN_INTERVAL = 60f;
        
        /// <summary>Максимум Interstitial за сессию</summary>
        public const int MAX_INTERSTITIALS_PER_SESSION = 5;
        
        /// <summary>Высота баннера по умолчанию (dp)</summary>
        public const float DEFAULT_BANNER_HEIGHT = 50f;
        
        // ==================== СОХРАНЕНИЯ ====================
        
        /// <summary>Ключ для PlayerPrefs - данные сохранения</summary>
        public const string SAVE_KEY = "TicTacToe_SaveData";
        
        /// <summary>Ключ для PlayerPrefs - настройки</summary>
        public const string SETTINGS_KEY = "TicTacToe_Settings";
        
        /// <summary>Версия формата сохранения</summary>
        public const int SAVE_VERSION = 1;
        
        // ==================== SCREEN IDS ====================
        
        /// <summary>
        /// Идентификаторы экранов для навигации в UIManager.
        /// Использовать вместо строковых литералов.
        /// </summary>
        public static class Screens
        {
            /// <summary>Главное меню</summary>
            public const string MAIN_MENU = "MainMenu";
            
            /// <summary>Выбор сложности AI</summary>
            public const string DIFFICULTY = "Difficulty";
            
            /// <summary>Игровой экран</summary>
            public const string GAME = "Game";
            
            /// <summary>Экран результатов (может быть popup)</summary>
            public const string RESULT = "Result";
            
            /// <summary>Лобби сетевой игры</summary>
            public const string LOBBY = "Lobby";
            
            /// <summary>Создание сетевой игры (Host)</summary>
            public const string HOST = "Host";
            
            /// <summary>Присоединение к игре (Join)</summary>
            public const string JOIN = "Join";
            
            /// <summary>Магазин тем</summary>
            public const string SHOP = "Shop";
            
            /// <summary>Настройки</summary>
            public const string SETTINGS = "Settings";
            
            /// <summary>Статистика игрока</summary>
            public const string STATISTICS = "Statistics";
        }
        
        // ==================== POPUP IDS ====================
        
        /// <summary>
        /// Идентификаторы popup-ов для UIManager.
        /// </summary>
        public static class Popups
        {
            /// <summary>Пауза</summary>
            public const string PAUSE = "PausePopup";
            
            /// <summary>Результат игры</summary>
            public const string RESULT = "ResultPopup";
            
            /// <summary>Подтверждение покупки</summary>
            public const string PURCHASE = "PurchasePopup";
            
            /// <summary>Ошибка</summary>
            public const string ERROR = "ErrorPopup";
            
            /// <summary>Подтверждение действия</summary>
            public const string CONFIRM = "ConfirmPopup";
            
            /// <summary>Загрузка</summary>
            public const string LOADING = "LoadingPopup";
            
            /// <summary>Переподключение к сети</summary>
            public const string RECONNECT = "ReconnectPopup";
        }
        
        // ==================== ANIMATION PARAMETERS ====================
        
        /// <summary>
        /// Имена параметров для Animator.
        /// Использовать вместо строковых литералов.
        /// </summary>
        public static class AnimParams
        {
            /// <summary>Trigger для появления</summary>
            public const string SHOW = "Show";
            
            /// <summary>Trigger для скрытия</summary>
            public const string HIDE = "Hide";
            
            /// <summary>Trigger для выигрыша</summary>
            public const string WIN = "Win";
            
            /// <summary>Bool для выделения</summary>
            public const string HIGHLIGHTED = "Highlighted";
            
            /// <summary>Bool для интерактивности</summary>
            public const string INTERACTABLE = "Interactable";
        }
        
        // ==================== AUDIO ====================
        
        /// <summary>
        /// Идентификаторы звуков для AudioManager.
        /// </summary>
        public static class Sounds
        {
            /// <summary>Звук постановки X</summary>
            public const string PLACE_X = "place_x";
            
            /// <summary>Звук постановки O</summary>
            public const string PLACE_O = "place_o";
            
            /// <summary>Звук победы</summary>
            public const string WIN = "win";
            
            /// <summary>Звук поражения</summary>
            public const string LOSE = "lose";
            
            /// <summary>Звук ничьей</summary>
            public const string DRAW = "draw";
            
            /// <summary>Звук нажатия кнопки</summary>
            public const string BUTTON_CLICK = "button_click";
            
            /// <summary>Звук ошибки</summary>
            public const string ERROR = "error";
        }
        
        // ==================== THEMES ====================
        
        /// <summary>
        /// Идентификаторы тем оформления.
        /// </summary>
        public static class Themes
        {
            /// <summary>Классическая тема (бесплатная)</summary>
            public const string CLASSIC = "theme_classic";
            
            /// <summary>Неоновая тема</summary>
            public const string NEON = "theme_neon";
            
            /// <summary>Природа</summary>
            public const string NATURE = "theme_nature";
            
            /// <summary>Минимализм</summary>
            public const string MINIMAL = "theme_minimal";
            
            /// <summary>Космос</summary>
            public const string SPACE = "theme_space";
            
            /// <summary>Киберпанк</summary>
            public const string CYBERPUNK = "theme_cyberpunk";
            
            /// <summary>Ретро</summary>
            public const string RETRO = "theme_retro";
        }
        
        // ==================== IAP PRODUCT IDS ====================
        
        /// <summary>
        /// Идентификаторы IAP продуктов.
        /// Должны совпадать с ID в Google Play Console / App Store Connect.
        /// </summary>
        public static class IAPProducts
        {
            /// <summary>Отключение рекламы</summary>
            public const string REMOVE_ADS = "com.tictactoe.remove_ads";
            
            /// <summary>Неоновая тема</summary>
            public const string THEME_NEON = "com.tictactoe.theme_neon";
            
            /// <summary>Тема Nature</summary>
            public const string THEME_NATURE = "com.tictactoe.theme_nature";
            
            /// <summary>Тема Minimal</summary>
            public const string THEME_MINIMAL = "com.tictactoe.theme_minimal";
            
            /// <summary>Тема Space</summary>
            public const string THEME_SPACE = "com.tictactoe.theme_space";
            
            /// <summary>Тема Cyberpunk</summary>
            public const string THEME_CYBERPUNK = "com.tictactoe.theme_cyberpunk";
            
            /// <summary>Тема Retro</summary>
            public const string THEME_RETRO = "com.tictactoe.theme_retro";
            
            /// <summary>Набор всех тем со скидкой</summary>
            public const string ALL_THEMES_BUNDLE = "com.tictactoe.all_themes";
        }
        
        // ==================== ANALYTICS EVENTS ====================
        
        /// <summary>
        /// Названия событий аналитики.
        /// </summary>
        public static class AnalyticsEvents
        {
            /// <summary>Игра начата</summary>
            public const string GAME_START = "game_start";
            
            /// <summary>Игра завершена</summary>
            public const string GAME_END = "game_end";
            
            /// <summary>Покупка совершена</summary>
            public const string PURCHASE = "purchase";
            
            /// <summary>Реклама показана</summary>
            public const string AD_SHOWN = "ad_shown";
            
            /// <summary>Тема изменена</summary>
            public const string THEME_CHANGED = "theme_changed";
            
            /// <summary>Экран открыт</summary>
            public const string SCREEN_VIEW = "screen_view";
        }
        
        // ==================== LAYER MASKS ====================
        
        /// <summary>
        /// Имена слоёв для LayerMask.
        /// </summary>
        public static class Layers
        {
            /// <summary>Слой UI</summary>
            public const string UI = "UI";
            
            /// <summary>Слой игровых объектов</summary>
            public const string GAME = "Game";
        }
        
        // ==================== TAGS ====================
        
        /// <summary>
        /// Теги объектов.
        /// </summary>
        public static class Tags
        {
            /// <summary>Тег игрового поля</summary>
            public const string BOARD = "Board";
            
            /// <summary>Тег клетки</summary>
            public const string CELL = "Cell";
        }
    }
}
