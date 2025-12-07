// Assets/_Project/Scripts/UI/Screens/MainMenuScreen.cs

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Utils;

namespace TicTacToe.UI.Screens
{
    /// <summary>
    /// Экран главного меню игры.
    /// Содержит кнопки для выбора режима игры и навигации.
    /// </summary>
    public class MainMenuScreen : BaseScreen
    {
        // Header Buttons
        [Header("Header Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;
        
        // Logo & Title
        [Header("Logo & Title")]
        [SerializeField] private Image _logoImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        
        // Game Mode Buttons
        [Header("Game Mode Buttons")]
        [SerializeField] private Button _vsAIButton;
        [SerializeField] private Button _localMultiplayerButton;
        [SerializeField] private Button _networkMultiplayerButton;
        
        // Button Texts (for localization support)
        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _vsAIButtonText;
        [SerializeField] private TextMeshProUGUI _localMultiplayerButtonText;
        [SerializeField] private TextMeshProUGUI _networkMultiplayerButtonText;
        
        // Button Icons
        [Header("Button Icons")]
        [SerializeField] private Image _vsAIIcon;
        [SerializeField] private Image _localMultiplayerIcon;
        [SerializeField] private Image _networkMultiplayerIcon;
        
        // Optional: Statistics Display
        [Header("Statistics (Optional)")]
        [SerializeField] private GameObject _statsContainer;
        [SerializeField] private TextMeshProUGUI _totalGamesText;
        [SerializeField] private TextMeshProUGUI _winsText;
        
        // Animation Settings
        [Header("Button Animation")]
        [SerializeField] private float _buttonAnimationDelay = 0.1f;
        [SerializeField] private float _buttonAnimationDuration = 0.3f;
        
        /// <summary>
        /// Событие при нажатии кнопки "Против ИИ"
        /// </summary>
        public event Action OnVsAIClicked;
        
        /// <summary>
        /// Событие при нажатии кнопки "Локальный мультиплеер"
        /// </summary>
        public event Action OnLocalMultiplayerClicked;
        
        /// <summary>
        /// Событие при нажатии кнопки "Сетевая игра"
        /// </summary>
        public event Action OnNetworkMultiplayerClicked;
        
        /// <summary>
        /// Событие при нажатии кнопки настроек
        /// </summary>
        public event Action OnSettingsClicked;
        
        /// <summary>
        /// Событие при нажатии кнопки магазина
        /// </summary>
        public event Action OnShopClicked;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
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
        /// Вызывается при показе экрана
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Обновляем статистику при показе
            UpdateStatistics();
            
            // Запускаем анимацию появления кнопок
            AnimateButtonsIn();
            
            // Примечание: GameManager автоматически переходит в MainMenu state
            // при вызове QuitToMenu() или при старте приложения
        }

        /// <summary>
        /// Вызывается при скрытии экрана
        /// </summary>
        public override void OnScreenHide()
        {
            base.OnScreenHide();
        }

        /// <summary>
        /// Анимация появления кнопок
        /// </summary>
        private void AnimateButtonsIn()
        {
            // Анимация кнопок с задержкой
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
        /// Анимация отдельной кнопки
        /// </summary>
        /// <param name="button">Кнопка для анимации</param>
        /// <param name="delay">Задержка перед началом анимации</param>
        private void AnimateButton(Button button, float delay)
        {
            if (button == null) return;
            
            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            }
            
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            
            // Начальное состояние
            canvasGroup.alpha = 0f;
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + new Vector2(0, -30f);
            
            // Запускаем анимацию с задержкой
            StartCoroutine(AnimateButtonCoroutine(canvasGroup, rectTransform, originalPosition, delay));
        }

        /// <summary>
        /// Корутина анимации кнопки
        /// </summary>
        private System.Collections.IEnumerator AnimateButtonCoroutine(
            CanvasGroup canvasGroup, 
            RectTransform rectTransform, 
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
                
                // Ease out quad
                float easedT = 1f - (1f - t) * (1f - t);
                
                canvasGroup.alpha = easedT;
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = targetPosition;
        }

        /// <summary>
        /// Обновление статистики игрока
        /// </summary>
        private void UpdateStatistics()
        {
            // TODO: Фаза 5 - интеграция с SaveSystem
            // Пока показываем заглушку или скрываем контейнер
            
            if (_statsContainer != null)
            {
                // Скрываем до реализации SaveSystem
                _statsContainer.SetActive(false);
            }
            
            /*
            // Пример реализации после Фазы 5:
            if (SaveSystem.Instance != null)
            {
                var stats = SaveSystem.Instance.GetStatistics();
                
                if (_totalGamesText != null)
                {
                    _totalGamesText.text = $"Игр: {stats.TotalGames}";
                }
                
                if (_winsText != null)
                {
                    _winsText.text = $"Побед: {stats.Wins}";
                }
                
                _statsContainer?.SetActive(true);
            }
            */
        }

        #region Button Click Handlers

        /// <summary>
        /// Обработчик нажатия кнопки "Против ИИ"
        /// </summary>
        private void HandleVsAIClick()
        {
            PlayButtonSound();
            OnVsAIClicked?.Invoke();
            
            // Переход на экран выбора сложности
            ShowScreen(Constants.Screens.DIFFICULTY);
        }

        /// <summary>
        /// Обработчик нажатия кнопки "На одном устройстве"
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки "По Wi-Fi"
        /// </summary>
        private void HandleNetworkMultiplayerClick()
        {
            PlayButtonSound();
            OnNetworkMultiplayerClicked?.Invoke();
            
            // Переход на экран сетевого лобби
            ShowScreen(Constants.Screens.LOBBY);
        }

        /// <summary>
        /// Обработчик нажатия кнопки настроек
        /// </summary>
        private void HandleSettingsClick()
        {
            PlayButtonSound();
            OnSettingsClicked?.Invoke();
            
            // Переход на экран настроек
            ShowScreen(Constants.Screens.SETTINGS);
        }

        /// <summary>
        /// Обработчик нажатия кнопки магазина
        /// </summary>
        private void HandleShopClick()
        {
            PlayButtonSound();
            OnShopClicked?.Invoke();
            
            // Переход на экран магазина
            ShowScreen(Constants.Screens.SHOP);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Воспроизведение звука нажатия кнопки
        /// </summary>
        private void PlayButtonSound()
        {
            // TODO: Фаза 9 - интеграция с AudioManager
            /*
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(Constants.Sounds.BUTTON_CLICK);
            }
            */
        }

        /// <summary>
        /// Устанавливает текст кнопки "Против ИИ"
        /// </summary>
        /// <param name="text">Текст кнопки</param>
        public void SetVsAIButtonText(string text)
        {
            if (_vsAIButtonText != null)
            {
                _vsAIButtonText.text = text;
            }
        }

        /// <summary>
        /// Устанавливает текст кнопки "Локальный мультиплеер"
        /// </summary>
        /// <param name="text">Текст кнопки</param>
        public void SetLocalMultiplayerButtonText(string text)
        {
            if (_localMultiplayerButtonText != null)
            {
                _localMultiplayerButtonText.text = text;
            }
        }

        /// <summary>
        /// Устанавливает текст кнопки "Сетевая игра"
        /// </summary>
        /// <param name="text">Текст кнопки</param>
        public void SetNetworkMultiplayerButtonText(string text)
        {
            if (_networkMultiplayerButtonText != null)
            {
                _networkMultiplayerButtonText.text = text;
            }
        }

        /// <summary>
        /// Включает/выключает кнопку сетевой игры
        /// (например, если нет Wi-Fi подключения)
        /// </summary>
        /// <param name="interactable">Доступность кнопки</param>
        public void SetNetworkButtonInteractable(bool interactable)
        {
            if (_networkMultiplayerButton != null)
            {
                _networkMultiplayerButton.interactable = interactable;
            }
        }

        /// <summary>
        /// Проверяет доступность сетевого подключения и обновляет UI
        /// </summary>
        public void UpdateNetworkAvailability()
        {
            bool isNetworkAvailable = Application.internetReachability != NetworkReachability.NotReachable;
            
            // На WebGL сетевая игра по Wi-Fi недоступна (только LAN)
            #if UNITY_WEBGL
            isNetworkAvailable = false;
            #endif
            
            SetNetworkButtonInteractable(isNetworkAvailable);
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            // Отписываемся от событий кнопок
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

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Автоматическое присвоение ID экрана в редакторе
            if (string.IsNullOrEmpty(ScreenId))
            {
                // Используем рефлексию для доступа к protected полю или просто устанавливаем через SerializedObject
                // Это поле должно быть установлено в инспекторе как "main_menu"
            }
        }
        #endif

        #endregion
    }
}
