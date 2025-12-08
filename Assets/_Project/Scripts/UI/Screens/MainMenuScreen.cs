// Assets/_Project/Scripts/UI/Screens/MainMenuScreen.cs

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Save;
using TicTacToe.Utils;

namespace TicTacToe.UI.Screens
{
    /// <summary>
    /// Экран главного меню.
    /// Отображает кнопки выбора режима игры и статистику.
    /// </summary>
    public class MainMenuScreen : BaseScreen
    {
        [Header("Header Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;
        
        [Header("Game Mode Buttons")]
        [SerializeField] private Button _vsAIButton;
        [SerializeField] private Button _localMultiplayerButton;
        [SerializeField] private Button _networkMultiplayerButton;
        
        [Header("Statistics Display")]
        [SerializeField] private GameObject _statsContainer;
        [SerializeField] private TMP_Text _totalGamesText;
        [SerializeField] private TMP_Text _winsText;
        [SerializeField] private TMP_Text _winRateText;
        [SerializeField] private TMP_Text _bestStreakText;
        
        [Header("Animation Settings")]
        [SerializeField] private float _buttonAnimationDelay = 0.1f;
        [SerializeField] private float _buttonAnimationDuration = 0.3f;
        
        // События для внешних систем
        public event Action OnVsAIClicked;
        public event Action OnLocalMultiplayerClicked;
        public event Action OnNetworkMultiplayerClicked;
        public event Action OnSettingsClicked;
        public event Action OnShopClicked;
        
        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            RemoveButtonListeners();
        }
        
        /// <summary>
        /// Настройка обработчиков кнопок
        /// </summary>
        private void SetupButtons()
        {
            // Header buttons
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(HandleSettingsClick);
            }
            
            if (_shopButton != null)
            {
                _shopButton.onClick.AddListener(HandleShopClick);
            }
            
            // Game mode buttons
            if (_vsAIButton != null)
            {
                _vsAIButton.onClick.AddListener(HandleVsAIClick);
            }
            
            if (_localMultiplayerButton != null)
            {
                _localMultiplayerButton.onClick.AddListener(HandleLocalMultiplayerClick);
            }
            
            if (_networkMultiplayerButton != null)
            {
                _networkMultiplayerButton.onClick.AddListener(HandleNetworkMultiplayerClick);
            }
        }
        
        /// <summary>
        /// Удаление обработчиков при уничтожении
        /// </summary>
        private void RemoveButtonListeners()
        {
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(HandleSettingsClick);
            }
            
            if (_shopButton != null)
            {
                _shopButton.onClick.RemoveListener(HandleShopClick);
            }
            
            if (_vsAIButton != null)
            {
                _vsAIButton.onClick.RemoveListener(HandleVsAIClick);
            }
            
            if (_localMultiplayerButton != null)
            {
                _localMultiplayerButton.onClick.RemoveListener(HandleLocalMultiplayerClick);
            }
            
            if (_networkMultiplayerButton != null)
            {
                _networkMultiplayerButton.onClick.RemoveListener(HandleNetworkMultiplayerClick);
            }
        }
        
        /// <summary>
        /// Вызывается при показе экрана
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Обновляем статистику при показе
            UpdateStatistics();
            
            // Запускаем анимацию появления кнопок
            AnimateButtonsIn();
            
            // Убеждаемся, что GameManager в правильном состоянии
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameManager.State.MainMenu);
            }
        }
        
        /// <summary>
        /// Вызывается при скрытии экрана
        /// </summary>
        public override void OnScreenHide()
        {
            base.OnScreenHide();
        }
        
        /// <summary>
        /// Обновляет отображение статистики из SaveSystem
        /// </summary>
        private void UpdateStatistics()
        {
            var saveSystem = SaveSystem.Instance;
            
            // Проверяем, инициализирована ли система сохранений
            if (saveSystem == null || saveSystem.Data == null || saveSystem.Data.Statistics == null)
            {
                // Скрываем контейнер статистики если данных нет
                if (_statsContainer != null)
                {
                    _statsContainer.SetActive(false);
                }
                return;
            }
            
            // Показываем контейнер статистики
            if (_statsContainer != null)
            {
                _statsContainer.SetActive(true);
            }
            
            var stats = saveSystem.Data.Statistics;
            
            // Общее количество игр
            if (_totalGamesText != null)
            {
                _totalGamesText.text = $"Games: {stats.TotalGames}";
            }
            
            // Победы
            if (_winsText != null)
            {
                _winsText.text = $"Wins: {stats.TotalWins}";
            }
            
            // Процент побед
            if (_winRateText != null)
            {
                if (stats.TotalGames > 0)
                {
                    _winRateText.text = $"Win Rate: {stats.WinRate:F0}%";
                }
                else
                {
                    _winRateText.text = "Win Rate: -";
                }
            }
            
            // Лучшая серия побед
            if (_bestStreakText != null)
            {
                _bestStreakText.text = $"Best Streak: {stats.BestWinStreak}";
            }
        }
        
        /// <summary>
        /// Анимация появления кнопок
        /// </summary>
        private void AnimateButtonsIn()
        {
            Button[] buttons = { _vsAIButton, _localMultiplayerButton, _networkMultiplayerButton };
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    AnimateButton(buttons[i], i * _buttonAnimationDelay);
                }
            }
        }
        
        /// <summary>
        /// Анимирует появление одной кнопки
        /// </summary>
        private void AnimateButton(Button button, float delay)
        {
            if (button == null) return;
            
            var rectTransform = button.GetComponent<RectTransform>();
            var canvasGroup = button.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Начальное состояние
            canvasGroup.alpha = 0f;
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + new Vector2(0, -30f);
            
            // Анимация через корутину
            StartCoroutine(AnimateButtonCoroutine(rectTransform, canvasGroup, originalPosition, delay));
        }
        
        private System.Collections.IEnumerator AnimateButtonCoroutine(
            RectTransform rectTransform, 
            CanvasGroup canvasGroup, 
            Vector2 targetPosition, 
            float delay)
        {
            yield return new WaitForSeconds(delay);
            
            float elapsed = 0f;
            Vector2 startPosition = rectTransform.anchoredPosition;
            
            while (elapsed < _buttonAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _buttonAnimationDuration;
                
                // Ease out
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                canvasGroup.alpha = t;
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = targetPosition;
        }
        
        // ==================== Button Click Handlers ====================
        
        private void HandleVsAIClick()
        {
            PlayButtonSound();
            OnVsAIClicked?.Invoke();
            
            // Переход на экран выбора сложности
            ShowScreen(Constants.Screens.DIFFICULTY);
        }
        
        private void HandleLocalMultiplayerClick()
        {
            PlayButtonSound();
            OnLocalMultiplayerClicked?.Invoke();
            
            // Сразу начинаем игру в локальном мультиплеере
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartLocalMultiplayer();
            }
            
            // Переход на игровой экран
            ShowScreen(Constants.Screens.GAME);
        }
        
        private void HandleNetworkMultiplayerClick()
        {
            PlayButtonSound();
            OnNetworkMultiplayerClicked?.Invoke();
            
            // Переход на экран сетевого лобби
            ShowScreen(Constants.Screens.LOBBY);
        }
        
        private void HandleSettingsClick()
        {
            PlayButtonSound();
            OnSettingsClicked?.Invoke();
            
            // Переход на экран настроек
            ShowScreen(Constants.Screens.SETTINGS);
        }
        
        private void HandleShopClick()
        {
            PlayButtonSound();
            OnShopClicked?.Invoke();
            
            // Переход в магазин
            ShowScreen(Constants.Screens.SHOP);
        }
        
        // ==================== Helper Methods ====================
        
        /// <summary>
        /// Воспроизводит звук нажатия кнопки
        /// </summary>
        private void PlayButtonSound()
        {
            // TODO: Фаза 9 — AudioManager
            // AudioManager.Instance?.PlaySfx(SoundType.ButtonClick);
        }
        
        /// <summary>
        /// Переход на другой экран через UIManager
        /// </summary>
        private void ShowScreen(string screenId)
        {
            UIManager.Instance?.ShowScreen(screenId);
        }
        
        /// <summary>
        /// Принудительное обновление статистики (для вызова извне)
        /// </summary>
        public void RefreshStatistics()
        {
            UpdateStatistics();
        }
    }
}
