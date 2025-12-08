// Assets/_Project/Scripts/Save/SaveSystem.cs

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using TicTacToe.Utils;

namespace TicTacToe.Save
{
    /// <summary>
    /// Система сохранения и загрузки данных игры.
    /// Использует JSON сериализацию с платформо-зависимым хранилищем:
    /// - WebGL: PlayerPrefs
    /// - Остальные платформы: файловая система
    /// </summary>
    public class SaveSystem : Singleton<SaveSystem>
    {
        private const string SAVE_FILE_NAME = "tictactoe_save.json";
        private const string BACKUP_FILE_NAME = "tictactoe_save_backup.json";
        private const string PLAYER_PREFS_KEY = "TicTacToe_SaveData";
        
        private CloudSaveService _cloudSaveService;
        private bool _isInitialized = false;
        private bool _isDirty = false;
        private float _autoSaveTimer = 0f;
        
        /// <summary>
        /// Интервал автосохранения в секундах
        /// </summary>
        private const float AUTO_SAVE_INTERVAL = 30f;
        
        /// <summary>
        /// Текущие данные сохранения
        /// </summary>
        public SaveData Data { get; private set; }
        
        /// <summary>
        /// Сервис облачного сохранения
        /// </summary>
        public CloudSaveService CloudService => _cloudSaveService;
        
        /// <summary>
        /// Инициализирована ли система
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Есть ли несохранённые изменения
        /// </summary>
        public bool HasUnsavedChanges => _isDirty;
        
        /// <summary>
        /// Событие: данные загружены
        /// </summary>
        public event Action OnDataLoaded;
        
        /// <summary>
        /// Событие: данные сохранены
        /// </summary>
        public event Action OnDataSaved;
        
        /// <summary>
        /// Событие: данные изменены
        /// </summary>
        public event Action OnDataChanged;
        
        /// <summary>
        /// Событие: ошибка сохранения/загрузки
        /// </summary>
        public event Action<string> OnError;
        
        protected override void Awake()
        {
            base.Awake();
            _cloudSaveService = new CloudSaveService();
        }
        
        private void Update()
        {
            // Автосохранение при наличии несохранённых изменений
            if (_isDirty && _isInitialized)
            {
                _autoSaveTimer += Time.unscaledDeltaTime;
                if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
                {
                    Save();
                    _autoSaveTimer = 0f;
                }
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Сохраняем при сворачивании приложения
            if (pauseStatus && _isDirty)
            {
                Save();
            }
        }
        
        private void OnApplicationQuit()
        {
            // Сохраняем перед выходом
            if (_isDirty)
            {
                Save();
            }
        }
        
        /// <summary>
        /// Инициализирует систему сохранений
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[SaveSystem] Already initialized");
                return;
            }
            
            Load();
            _isInitialized = true;
            
            Debug.Log($"[SaveSystem] Initialized. Player ID: {Data?.PlayerId}");
        }
        
        /// <summary>
        /// Асинхронная инициализация с облачной синхронизацией
        /// </summary>
        public async Task InitializeAsync()
        {
            Initialize();
            
            // Пытаемся синхронизировать с облаком
            try
            {
                await SyncWithCloud();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Cloud sync failed during init: {e.Message}");
                // Продолжаем работу с локальными данными
            }
        }
        
        /// <summary>
        /// Сохраняет данные в локальное хранилище
        /// </summary>
        public void Save()
        {
            if (Data == null)
            {
                Debug.LogError("[SaveSystem] Cannot save: Data is null");
                return;
            }
            
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                SaveToPlayerPrefs(json);
                #else
                SaveToFile(json);
                #endif
                
                _isDirty = false;
                _autoSaveTimer = 0f;
                
                OnDataSaved?.Invoke();
                Debug.Log("[SaveSystem] Data saved successfully");
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to save: {e.Message}";
                Debug.LogError($"[SaveSystem] {errorMessage}");
                OnError?.Invoke(errorMessage);
            }
        }
        
        /// <summary>
        /// Загружает данные из локального хранилища
        /// </summary>
        public void Load()
        {
            try
            {
                string json = null;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                json = LoadFromPlayerPrefs();
                #else
                json = LoadFromFile();
                #endif
                
                if (string.IsNullOrEmpty(json))
                {
                    CreateDefaultSave();
                    Debug.Log("[SaveSystem] Created new save data");
                }
                else
                {
                    Data = JsonUtility.FromJson<SaveData>(json);
                    
                    if (Data == null)
                    {
                        Debug.LogWarning("[SaveSystem] Failed to deserialize save data, creating default");
                        CreateDefaultSave();
                    }
                    else
                    {
                        // Миграция если нужно
                        if (Data.Version < SaveData.CURRENT_VERSION)
                        {
                            MigrateSaveData(Data.Version);
                            Save(); // Сохраняем мигрированные данные
                        }
                        
                        Debug.Log($"[SaveSystem] Loaded save data v{Data.Version}");
                    }
                }
                
                _isDirty = false;
                OnDataLoaded?.Invoke();
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to load: {e.Message}";
                Debug.LogError($"[SaveSystem] {errorMessage}");
                
                // Пытаемся восстановить из резервной копии
                if (TryRestoreFromBackup())
                {
                    Debug.Log("[SaveSystem] Restored from backup");
                }
                else
                {
                    CreateDefaultSave();
                    Debug.Log("[SaveSystem] Created default save after load failure");
                }
                
                OnError?.Invoke(errorMessage);
            }
        }
        
        /// <summary>
        /// Сбрасывает данные до значений по умолчанию
        /// </summary>
        public void Reset()
        {
            CreateDefaultSave();
            Save();
            Debug.Log("[SaveSystem] Save data reset to default");
        }
        
        /// <summary>
        /// Помечает данные как изменённые (требуют сохранения)
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
            OnDataChanged?.Invoke();
        }
        
        /// <summary>
        /// Принудительно сохраняет данные немедленно
        /// </summary>
        public void ForceSave()
        {
            Save();
        }
        
        /// <summary>
        /// Синхронизирует данные с облаком
        /// </summary>
        public async Task SyncWithCloud()
        {
            if (!_cloudSaveService.IsAvailable)
            {
                Debug.Log("[SaveSystem] Cloud save not available");
                return;
            }
            
            try
            {
                // Инициализируем облачный сервис если нужно
                if (!_cloudSaveService.IsAuthenticated)
                {
                    await _cloudSaveService.Initialize();
                }
                
                // Загружаем облачные данные
                SaveData cloudData = await _cloudSaveService.LoadFromCloud();
                
                if (cloudData != null)
                {
                    // Объединяем данные
                    Data = _cloudSaveService.MergeData(Data, cloudData);
                    Save();
                    Debug.Log("[SaveSystem] Synced with cloud");
                }
                else
                {
                    // В облаке нет данных - загружаем локальные
                    await UploadToCloud();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Cloud sync failed: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Загружает данные в облако
        /// </summary>
        public async Task UploadToCloud()
        {
            if (!_cloudSaveService.IsAvailable || Data == null)
            {
                return;
            }
            
            try
            {
                await _cloudSaveService.SaveToCloud(Data);
                Debug.Log("[SaveSystem] Uploaded to cloud");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Cloud upload failed: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Скачивает данные из облака (перезаписывает локальные)
        /// </summary>
        public async Task DownloadFromCloud()
        {
            if (!_cloudSaveService.IsAvailable)
            {
                return;
            }
            
            try
            {
                SaveData cloudData = await _cloudSaveService.LoadFromCloud();
                
                if (cloudData != null)
                {
                    Data = cloudData;
                    Save();
                    Debug.Log("[SaveSystem] Downloaded from cloud");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Cloud download failed: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Возвращает путь к файлу сохранения
        /// </summary>
        public string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }
        
        /// <summary>
        /// Возвращает путь к файлу резервной копии
        /// </summary>
        public string GetBackupPath()
        {
            return Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);
        }
        
        /// <summary>
        /// Создаёт резервную копию сохранения
        /// </summary>
        public void CreateBackup()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                string savePath = GetSavePath();
                string backupPath = GetBackupPath();
                
                if (File.Exists(savePath))
                {
                    File.Copy(savePath, backupPath, true);
                    Debug.Log("[SaveSystem] Backup created");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Failed to create backup: {e.Message}");
            }
            #endif
        }
        
        // ==================== Приватные методы ====================
        
        private void CreateDefaultSave()
        {
            Data = SaveData.CreateDefault();
            _isDirty = true;
        }
        
        private void SaveToPlayerPrefs(string json)
        {
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, json);
            PlayerPrefs.Save();
        }
        
        private string LoadFromPlayerPrefs()
        {
            return PlayerPrefs.GetString(PLAYER_PREFS_KEY, null);
        }
        
        private void SaveToFile(string json)
        {
            string path = GetSavePath();
            
            // Создаём резервную копию перед сохранением
            if (File.Exists(path))
            {
                CreateBackup();
            }
            
            // Сохраняем во временный файл, затем переименовываем (атомарная операция)
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            File.Move(tempPath, path);
        }
        
        private string LoadFromFile()
        {
            string path = GetSavePath();
            
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            
            return null;
        }
        
        private bool TryRestoreFromBackup()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                string backupPath = GetBackupPath();
                
                if (File.Exists(backupPath))
                {
                    string json = File.ReadAllText(backupPath);
                    Data = JsonUtility.FromJson<SaveData>(json);
                    
                    if (Data != null)
                    {
                        _isDirty = true;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Failed to restore from backup: {e.Message}");
            }
            #endif
            
            return false;
        }
        
        /// <summary>
        /// Мигрирует данные со старых версий
        /// </summary>
        private void MigrateSaveData(int fromVersion)
        {
            Debug.Log($"[SaveSystem] Migrating save data from v{fromVersion} to v{SaveData.CURRENT_VERSION}");
            
            // Пример миграции с версии 1 на 2:
            // if (fromVersion < 2)
            // {
            //     // Добавляем новые поля или конвертируем данные
            //     Data.NewField = DefaultValue;
            // }
            
            // После всех миграций обновляем версию
            Data.Version = SaveData.CURRENT_VERSION;
        }
        
        // ==================== Удобные методы для работы с данными ====================
        
        /// <summary>
        /// Проверяет, отключена ли реклама
        /// </summary>
        public bool IsAdsRemoved()
        {
            return Data?.Purchases?.AdsRemoved ?? false;
        }
        
        /// <summary>
        /// Проверяет, куплена ли тема
        /// </summary>
        public bool HasTheme(string themeId)
        {
            return Data?.Purchases?.HasTheme(themeId) ?? themeId == PurchaseData.DEFAULT_THEME;
        }
        
        /// <summary>
        /// Возвращает текущую выбранную тему
        /// </summary>
        public string GetSelectedTheme()
        {
            return Data?.Settings?.SelectedTheme ?? PurchaseData.DEFAULT_THEME;
        }
        
        /// <summary>
        /// Устанавливает выбранную тему
        /// </summary>
        public void SetSelectedTheme(string themeId)
        {
            if (Data?.Settings != null && HasTheme(themeId))
            {
                Data.Settings.SelectedTheme = themeId;
                MarkDirty();
            }
        }
        
        /// <summary>
        /// Добавляет купленную тему
        /// </summary>
        public void AddPurchasedTheme(string themeId)
        {
            Data?.Purchases?.AddTheme(themeId);
            MarkDirty();
        }
        
        /// <summary>
        /// Отключает рекламу (покупка)
        /// </summary>
        public void RemoveAds()
        {
            if (Data?.Purchases != null)
            {
                Data.Purchases.AdsRemoved = true;
                MarkDirty();
            }
        }
        
        /// <summary>
        /// Записывает результат игры против AI
        /// </summary>
        public void RecordAIGameResult(int difficultyIndex, bool isWin, bool isDraw)
        {
            Data?.Statistics?.RecordAIGame(difficultyIndex, isWin, isDraw);
            Data?.Ads?.IncrementMatchCounter();
            MarkDirty();
        }
        
        /// <summary>
        /// Записывает результат локальной мультиплеерной игры
        /// </summary>
        public void RecordLocalMultiplayerResult(bool playerXWon, bool isDraw)
        {
            Data?.Statistics?.RecordLocalMultiplayerGame(playerXWon, isDraw);
            Data?.Ads?.IncrementMatchCounter();
            MarkDirty();
        }
        
        /// <summary>
        /// Записывает результат сетевой мультиплеерной игры
        /// </summary>
        public void RecordNetworkMultiplayerResult(bool isWin, bool isDraw)
        {
            Data?.Statistics?.RecordNetworkMultiplayerGame(isWin, isDraw);
            Data?.Ads?.IncrementMatchCounter();
            MarkDirty();
        }
        
        /// <summary>
        /// Возвращает общую статистику для отображения
        /// </summary>
        public (int wins, int losses, int draws, int total) GetStatsSummary()
        {
            if (Data?.Statistics == null)
            {
                return (0, 0, 0, 0);
            }
            
            var stats = Data.Statistics;
            return (stats.TotalWins, stats.TotalLosses, stats.TotalDraws, stats.TotalGames);
        }
        
        /// <summary>
        /// Возвращает настройки звука
        /// </summary>
        public (bool musicEnabled, bool sfxEnabled, float musicVolume, float sfxVolume) GetSoundSettings()
        {
            if (Data?.Settings == null)
            {
                return (true, true, 0.8f, 1.0f);
            }
            
            var settings = Data.Settings;
            return (settings.MusicEnabled, settings.SfxEnabled, settings.MusicVolume, settings.SfxVolume);
        }
        
        /// <summary>
        /// Сохраняет настройки звука
        /// </summary>
        public void SetSoundSettings(bool musicEnabled, bool sfxEnabled, 
            float musicVolume = -1f, float sfxVolume = -1f)
        {
            if (Data?.Settings == null) return;
            
            Data.Settings.MusicEnabled = musicEnabled;
            Data.Settings.SfxEnabled = sfxEnabled;
            
            if (musicVolume >= 0f)
            {
                Data.Settings.MusicVolume = Mathf.Clamp01(musicVolume);
            }
            if (sfxVolume >= 0f)
            {
                Data.Settings.SfxVolume = Mathf.Clamp01(sfxVolume);
            }
            
            MarkDirty();
        }
        
        /// <summary>
        /// Возвращает, включена ли вибрация
        /// </summary>
        public bool IsVibrationEnabled()
        {
            return Data?.Settings?.VibrationEnabled ?? true;
        }
        
        /// <summary>
        /// Устанавливает состояние вибрации
        /// </summary>
        public void SetVibrationEnabled(bool enabled)
        {
            if (Data?.Settings != null)
            {
                Data.Settings.VibrationEnabled = enabled;
                MarkDirty();
            }
        }
    }
}
