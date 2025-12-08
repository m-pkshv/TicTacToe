// Assets/_Project/Scripts/Core/Bootstrap.cs

using System;
using System.Threading.Tasks;
using UnityEngine;
using TicTacToe.Save;
using TicTacToe.UI;
using TicTacToe.Utils;

namespace TicTacToe.Core
{
    /// <summary>
    /// Точка входа приложения. Инициализирует все игровые системы в правильном порядке.
    /// Этот скрипт должен быть на объекте в первой загружаемой сцене (Bootstrap или Main).
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Initialization Settings")]
        [SerializeField] 
        [Tooltip("Использовать облачное сохранение (требует интернет)")]
        private bool _useCloudSave = true;
        
        [SerializeField]
        [Tooltip("Показывать загрузочный экран во время инициализации")]
        private bool _showLoadingScreen = true;
        
        [SerializeField]
        [Tooltip("Таймаут инициализации облака (секунды)")]
        private float _cloudInitTimeout = 10f;
        
        [Header("Debug")]
        [SerializeField]
        private bool _logInitialization = true;
        
        /// <summary>
        /// Вызывается когда инициализация завершена
        /// </summary>
        public static event Action OnInitializationComplete;
        
        /// <summary>
        /// Вызывается при ошибке инициализации
        /// </summary>
        public static event Action<string> OnInitializationError;
        
        /// <summary>
        /// Завершена ли инициализация
        /// </summary>
        public static bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Прогресс инициализации (0-1)
        /// </summary>
        public static float InitializationProgress { get; private set; }
        
        private async void Start()
        {
            await InitializeGameAsync();
        }
        
        /// <summary>
        /// Асинхронная инициализация всех систем
        /// </summary>
        private async Task InitializeGameAsync()
        {
            Log("=== Game Initialization Started ===");
            InitializationProgress = 0f;
            
            try
            {
                // Шаг 1: Базовые настройки Unity
                InitializeUnitySettings();
                InitializationProgress = 0.1f;
                Log("Unity settings initialized");
                
                // Шаг 2: SaveSystem (критически важно — первым!)
                await InitializeSaveSystemAsync();
                InitializationProgress = 0.4f;
                Log("SaveSystem initialized");
                
                // Шаг 3: GameManager
                InitializeGameManager();
                InitializationProgress = 0.6f;
                Log("GameManager initialized");
                
                // Шаг 4: Применяем сохранённые настройки
                ApplySavedSettings();
                InitializationProgress = 0.8f;
                Log("Saved settings applied");
                
                // Шаг 5: UI Manager (показываем главное меню)
                InitializeUI();
                InitializationProgress = 1f;
                Log("UI initialized");
                
                // Готово!
                IsInitialized = true;
                Log("=== Game Initialization Complete ===");
                
                OnInitializationComplete?.Invoke();
                
                // Отмечаем первую сессию как завершённую (для рекламы)
                MarkFirstSessionIfNeeded();
            }
            catch (Exception e)
            {
                string errorMessage = $"Initialization failed: {e.Message}";
                Debug.LogError($"[Bootstrap] {errorMessage}\n{e.StackTrace}");
                
                OnInitializationError?.Invoke(errorMessage);
                
                // Пытаемся показать хоть что-то
                TryShowFallbackUI();
            }
        }
        
        /// <summary>
        /// Настройки Unity
        /// </summary>
        private void InitializeUnitySettings()
        {
            // Целевой FPS
            Application.targetFrameRate = 60;
            
            // Не засыпать на мобильных
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Качество для мобильных
            #if UNITY_ANDROID || UNITY_IOS
            QualitySettings.vSyncCount = 0;
            #endif
            
            Log($"Target FPS: {Application.targetFrameRate}");
        }
        
        /// <summary>
        /// Инициализация системы сохранений
        /// </summary>
        private async Task InitializeSaveSystemAsync()
        {
            // Убеждаемся, что SaveSystem создан
            var saveSystem = SaveSystem.Instance;
            
            if (_useCloudSave)
            {
                // Асинхронная инициализация с облаком
                try
                {
                    var initTask = saveSystem.InitializeAsync();
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_cloudInitTimeout));
                    
                    var completedTask = await Task.WhenAny(initTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        Log("Cloud init timeout, using local save only");
                    }
                }
                catch (Exception e)
                {
                    Log($"Cloud init failed: {e.Message}, using local save");
                }
            }
            else
            {
                // Только локальная инициализация
                saveSystem.Initialize();
            }
            
            // Подписываемся на события
            saveSystem.OnDataSaved += OnDataSaved;
            saveSystem.OnError += OnSaveError;
            
            Log($"Player ID: {saveSystem.Data?.PlayerId}");
            Log($"Total games played: {saveSystem.Data?.Statistics?.TotalGames ?? 0}");
        }
        
        /// <summary>
        /// Инициализация GameManager
        /// </summary>
        private void InitializeGameManager()
        {
            // GameManager — Singleton, просто обращаемся к нему
            var gameManager = GameManager.Instance;
            
            if (gameManager == null)
            {
                Debug.LogError("[Bootstrap] GameManager not found! Make sure it exists in the scene or is created as Singleton.");
            }
        }
        
        /// <summary>
        /// Применяем сохранённые настройки
        /// </summary>
        private void ApplySavedSettings()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem?.Data?.Settings == null) return;
            
            var settings = saveSystem.Data.Settings;
            
            // Звук
            // TODO: Применить к AudioManager когда он будет создан (Фаза 9)
            Log($"Music: {(settings.MusicEnabled ? "ON" : "OFF")} ({settings.MusicVolume:P0})");
            Log($"SFX: {(settings.SfxEnabled ? "ON" : "OFF")} ({settings.SfxVolume:P0})");
            
            // Тема
            // TODO: Применить к ThemeManager когда он будет создан (Фаза 6)
            Log($"Theme: {settings.SelectedTheme}");
            
            // Вибрация
            Log($"Vibration: {(settings.VibrationEnabled ? "ON" : "OFF")}");
        }
        
        /// <summary>
        /// Инициализация UI
        /// </summary>
        private void InitializeUI()
        {
            // UIManager должен показать главное меню
            var uiManager = UIManager.Instance;
            
            if (uiManager != null)
            {
                uiManager.ShowScreen(Constants.Screens.MAIN_MENU);
            }
            else
            {
                Debug.LogWarning("[Bootstrap] UIManager not found!");
            }
        }
        
        /// <summary>
        /// Отмечаем первую сессию завершённой (для отложенного показа рекламы)
        /// </summary>
        private void MarkFirstSessionIfNeeded()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem?.Data?.Ads == null) return;
            
            // Отмечаем через несколько секунд после старта
            // (чтобы игрок успел хоть немного поиграть)
            Invoke(nameof(MarkFirstSessionComplete), 30f);
        }
        
        private void MarkFirstSessionComplete()
        {
            var adsData = SaveSystem.Instance?.Data?.Ads;
            if (adsData != null && !adsData.FirstSessionCompleted)
            {
                adsData.FirstSessionCompleted = true;
                SaveSystem.Instance.MarkDirty();
                Log("First session marked as complete");
            }
        }
        
        /// <summary>
        /// Попытка показать UI при ошибке инициализации
        /// </summary>
        private void TryShowFallbackUI()
        {
            try
            {
                // Пробуем показать главное меню даже при ошибке
                UIManager.Instance?.ShowScreen(Constants.Screens.MAIN_MENU);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrap] Fallback UI failed: {e.Message}");
            }
        }
        
        // ==================== Event Handlers ====================
        
        private void OnDataSaved()
        {
            Log("Game data saved");
        }
        
        private void OnSaveError(string error)
        {
            Debug.LogWarning($"[Bootstrap] Save error: {error}");
            // TODO: Показать уведомление пользователю
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Приложение свёрнуто — сохраняем
                SaveSystem.Instance?.ForceSave();
                Log("App paused - data saved");
            }
        }
        
        private void OnApplicationQuit()
        {
            // Финальное сохранение
            SaveSystem.Instance?.ForceSave();
            Log("App quit - data saved");
        }
        
        // ==================== Helpers ====================
        
        private void Log(string message)
        {
            if (_logInitialization)
            {
                Debug.Log($"[Bootstrap] {message}");
            }
        }
    }
}
