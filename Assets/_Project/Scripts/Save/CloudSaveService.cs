// Assets/_Project/Scripts/Save/CloudSaveService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_SERVICES_CORE
using Unity.Services.Core;
#endif

#if UNITY_SERVICES_AUTHENTICATION
using Unity.Services.Authentication;
#endif

#if UNITY_SERVICES_CLOUDSAVE
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
#endif

namespace TicTacToe.Save
{
    /// <summary>
    /// Сервис облачного сохранения через Unity Cloud Save.
    /// Обеспечивает синхронизацию данных между устройствами.
    /// </summary>
    public class CloudSaveService
    {
        private const string CLOUD_KEY = "player_save";
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const float RETRY_DELAY_SECONDS = 1f;
        
        private bool _isInitialized = false;
        
        /// <summary>
        /// Доступен ли облачный сервис на текущей платформе
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                #if UNITY_SERVICES_CLOUDSAVE && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE)
                return true;
                #else
                return false;
                #endif
            }
        }
        
        /// <summary>
        /// Авторизован ли пользователь
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                #if UNITY_SERVICES_AUTHENTICATION
                return AuthenticationService.Instance?.IsSignedIn ?? false;
                #else
                return false;
                #endif
            }
        }
        
        /// <summary>
        /// Инициализирован ли сервис
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// ID текущего игрока (из Authentication)
        /// </summary>
        public string PlayerId
        {
            get
            {
                #if UNITY_SERVICES_AUTHENTICATION
                return AuthenticationService.Instance?.PlayerId;
                #else
                return null;
                #endif
            }
        }
        
        /// <summary>
        /// Событие: успешная авторизация
        /// </summary>
        public event Action OnAuthenticated;
        
        /// <summary>
        /// Событие: ошибка авторизации
        /// </summary>
        public event Action<string> OnAuthenticationFailed;
        
        /// <summary>
        /// Событие: данные синхронизированы
        /// </summary>
        public event Action OnSynced;
        
        /// <summary>
        /// Событие: ошибка синхронизации
        /// </summary>
        public event Action<string> OnSyncFailed;
        
        /// <summary>
        /// Инициализирует Unity Services и выполняет анонимную авторизацию
        /// </summary>
        public async Task Initialize()
        {
            if (!IsAvailable)
            {
                Debug.Log("[CloudSave] Not available on this platform");
                return;
            }
            
            if (_isInitialized && IsAuthenticated)
            {
                Debug.Log("[CloudSave] Already initialized");
                return;
            }
            
            #if UNITY_SERVICES_CORE && UNITY_SERVICES_AUTHENTICATION
            try
            {
                // Инициализируем Unity Services
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("[CloudSave] Unity Services initialized");
                }
                
                // Подписываемся на события авторизации
                SetupAuthEvents();
                
                // Выполняем анонимную авторизацию
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[CloudSave] Signed in anonymously. Player ID: {PlayerId}");
                }
                
                _isInitialized = true;
                OnAuthenticated?.Invoke();
            }
            catch (AuthenticationException e)
            {
                string errorMessage = $"Authentication failed: {e.Message}";
                Debug.LogWarning($"[CloudSave] {errorMessage}");
                OnAuthenticationFailed?.Invoke(errorMessage);
                throw;
            }
            catch (RequestFailedException e)
            {
                string errorMessage = $"Request failed: {e.Message}";
                Debug.LogWarning($"[CloudSave] {errorMessage}");
                OnAuthenticationFailed?.Invoke(errorMessage);
                throw;
            }
            catch (Exception e)
            {
                string errorMessage = $"Initialization failed: {e.Message}";
                Debug.LogError($"[CloudSave] {errorMessage}");
                OnAuthenticationFailed?.Invoke(errorMessage);
                throw;
            }
            #endif
        }
        
        /// <summary>
        /// Загружает данные из облака
        /// </summary>
        public async Task<SaveData> LoadFromCloud()
        {
            if (!IsAvailable || !IsAuthenticated)
            {
                Debug.LogWarning("[CloudSave] Cannot load: not available or not authenticated");
                return null;
            }
            
            #if UNITY_SERVICES_CLOUDSAVE
            try
            {
                var keys = new HashSet<string> { CLOUD_KEY };
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
                
                if (savedData != null && savedData.TryGetValue(CLOUD_KEY, out var item))
                {
                    string json = item.Value.GetAsString();
                    
                    if (!string.IsNullOrEmpty(json))
                    {
                        SaveData data = JsonUtility.FromJson<SaveData>(json);
                        Debug.Log($"[CloudSave] Loaded data from cloud. Version: {data?.Version}");
                        return data;
                    }
                }
                
                Debug.Log("[CloudSave] No data found in cloud");
                return null;
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogWarning($"[CloudSave] Validation error: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                return null;
            }
            catch (CloudSaveRateLimitedException e)
            {
                Debug.LogWarning($"[CloudSave] Rate limited. Retry after: {e.RetryAfter}");
                OnSyncFailed?.Invoke("Rate limited, please try again later");
                return null;
            }
            catch (CloudSaveException e)
            {
                Debug.LogWarning($"[CloudSave] Cloud save error: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Load failed: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                throw;
            }
            #else
            return null;
            #endif
        }
        
        /// <summary>
        /// Сохраняет данные в облако
        /// </summary>
        public async Task SaveToCloud(SaveData saveData)
        {
            if (!IsAvailable || !IsAuthenticated)
            {
                Debug.LogWarning("[CloudSave] Cannot save: not available or not authenticated");
                return;
            }
            
            if (saveData == null)
            {
                Debug.LogError("[CloudSave] Cannot save null data");
                return;
            }
            
            #if UNITY_SERVICES_CLOUDSAVE
            try
            {
                string json = JsonUtility.ToJson(saveData);
                
                var data = new Dictionary<string, object>
                {
                    { CLOUD_KEY, json }
                };
                
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                
                Debug.Log("[CloudSave] Data saved to cloud");
                OnSynced?.Invoke();
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogWarning($"[CloudSave] Validation error: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                throw;
            }
            catch (CloudSaveRateLimitedException e)
            {
                Debug.LogWarning($"[CloudSave] Rate limited. Retry after: {e.RetryAfter}");
                OnSyncFailed?.Invoke("Rate limited, please try again later");
                throw;
            }
            catch (CloudSaveException e)
            {
                Debug.LogWarning($"[CloudSave] Cloud save error: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Save failed: {e.Message}");
                OnSyncFailed?.Invoke(e.Message);
                throw;
            }
            #endif
        }
        
        /// <summary>
        /// Сохраняет данные с повторными попытками
        /// </summary>
        public async Task SaveToCloudWithRetry(SaveData saveData, int maxAttempts = MAX_RETRY_ATTEMPTS)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await SaveToCloud(saveData);
                    return;
                }
                catch (Exception e)
                {
                    if (attempt == maxAttempts)
                    {
                        Debug.LogError($"[CloudSave] All {maxAttempts} save attempts failed");
                        throw;
                    }
                    
                    Debug.LogWarning($"[CloudSave] Save attempt {attempt} failed: {e.Message}. Retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(RETRY_DELAY_SECONDS * attempt));
                }
            }
        }
        
        /// <summary>
        /// Удаляет данные из облака
        /// </summary>
        public async Task DeleteFromCloud()
        {
            if (!IsAvailable || !IsAuthenticated)
            {
                return;
            }
            
            #if UNITY_SERVICES_CLOUDSAVE
            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(CLOUD_KEY);
                Debug.Log("[CloudSave] Data deleted from cloud");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Delete failed: {e.Message}");
                throw;
            }
            #endif
        }
        
        /// <summary>
        /// Объединяет локальные и облачные данные.
        /// Стратегия: покупки объединяются, статистика — берём максимум.
        /// </summary>
        public SaveData MergeData(SaveData local, SaveData cloud)
        {
            if (local == null && cloud == null)
            {
                return SaveData.CreateDefault();
            }
            
            if (local == null)
            {
                return cloud.Clone();
            }
            
            if (cloud == null)
            {
                return local.Clone();
            }
            
            // Создаём результат на основе локальных данных
            SaveData merged = local.Clone();
            
            // === Покупки: объединяем (если куплено где-то — считаем купленным) ===
            MergePurchases(merged.Purchases, cloud.Purchases);
            
            // === Настройки: берём локальные (более актуальные) ===
            // merged.Settings уже содержит локальные настройки
            
            // === Статистика: берём максимальные значения ===
            MergeStatistics(merged.Statistics, cloud.Statistics);
            
            // === Данные рекламы: берём большие значения ===
            MergeAdsData(merged.Ads, cloud.Ads);
            
            Debug.Log("[CloudSave] Data merged successfully");
            return merged;
        }
        
        /// <summary>
        /// Выход из аккаунта
        /// </summary>
        public void SignOut()
        {
            #if UNITY_SERVICES_AUTHENTICATION
            if (IsAuthenticated)
            {
                AuthenticationService.Instance.SignOut();
                _isInitialized = false;
                Debug.Log("[CloudSave] Signed out");
            }
            #endif
        }
        
        // ==================== Приватные методы ====================
        
        private void SetupAuthEvents()
        {
            #if UNITY_SERVICES_AUTHENTICATION
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"[CloudSave] Signed in event. Player ID: {PlayerId}");
            };
            
            AuthenticationService.Instance.SignedOut += () =>
            {
                Debug.Log("[CloudSave] Signed out event");
                _isInitialized = false;
            };
            
            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("[CloudSave] Session expired");
                // Можно попробовать переавторизоваться
            };
            #endif
        }
        
        private void MergePurchases(PurchaseData local, PurchaseData cloud)
        {
            if (cloud == null) return;
            
            // Объединяем темы
            if (cloud.OwnedThemes != null)
            {
                var mergedThemes = new HashSet<string>(local.OwnedThemes ?? new List<string>());
                mergedThemes.UnionWith(cloud.OwnedThemes);
                local.OwnedThemes = mergedThemes.ToList();
            }
            
            // Отключение рекламы — если куплено хоть где-то
            local.AdsRemoved = local.AdsRemoved || cloud.AdsRemoved;
        }
        
        private void MergeStatistics(StatisticsData local, StatisticsData cloud)
        {
            if (cloud == null) return;
            
            // Общие игры — максимум (не суммируем, чтобы избежать дублирования)
            local.TotalGames = Math.Max(local.TotalGames, cloud.TotalGames);
            
            // Победы против AI — поэлементный максимум
            MergeIntArray(ref local.WinsVsAI, cloud.WinsVsAI);
            MergeIntArray(ref local.LossesVsAI, cloud.LossesVsAI);
            MergeIntArray(ref local.DrawsVsAI, cloud.DrawsVsAI);
            
            // Остальная статистика — максимум
            local.WinsLocalMultiplayer = Math.Max(local.WinsLocalMultiplayer, cloud.WinsLocalMultiplayer);
            local.WinsNetworkMultiplayer = Math.Max(local.WinsNetworkMultiplayer, cloud.WinsNetworkMultiplayer);
            local.Losses = Math.Max(local.Losses, cloud.Losses);
            local.Draws = Math.Max(local.Draws, cloud.Draws);
            
            // Серия побед — берём лучшую
            local.BestWinStreak = Math.Max(local.BestWinStreak, cloud.BestWinStreak);
            // Текущая серия — берём локальную (более актуальная)
        }
        
        private void MergeIntArray(ref int[] local, int[] cloud)
        {
            if (cloud == null) return;
            
            if (local == null)
            {
                local = (int[])cloud.Clone();
                return;
            }
            
            int maxLength = Math.Max(local.Length, cloud.Length);
            int[] merged = new int[maxLength];
            
            for (int i = 0; i < maxLength; i++)
            {
                int localValue = i < local.Length ? local[i] : 0;
                int cloudValue = i < cloud.Length ? cloud[i] : 0;
                merged[i] = Math.Max(localValue, cloudValue);
            }
            
            local = merged;
        }
        
        private void MergeAdsData(AdsData local, AdsData cloud)
        {
            if (cloud == null) return;
            
            // Берём большие значения (предполагаем, что больше = более свежие данные)
            local.TotalInterstitialsShown = Math.Max(local.TotalInterstitialsShown, cloud.TotalInterstitialsShown);
            local.TotalRewardedShown = Math.Max(local.TotalRewardedShown, cloud.TotalRewardedShown);
            
            // Первая сессия завершена — если хоть где-то true
            local.FirstSessionCompleted = local.FirstSessionCompleted || cloud.FirstSessionCompleted;
            
            // Счётчик матчей и timestamp — берём локальные (более актуальные)
        }
    }
}
