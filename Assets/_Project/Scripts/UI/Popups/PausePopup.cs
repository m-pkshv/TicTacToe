// Assets/_Project/Scripts/UI/Popups/PausePopup.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Utils;

namespace TicTacToe.UI.Popups
{
    /// <summary>
    /// Popup меню паузы
    /// </summary>
    public class PausePopup : BaseScreen
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        
        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _resumeButtonText;
        [SerializeField] private TextMeshProUGUI _restartButtonText;
        [SerializeField] private TextMeshProUGUI _settingsButtonText;
        [SerializeField] private TextMeshProUGUI _mainMenuButtonText;
        
        [Header("Sound Toggle")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Image _soundIconOn;
        [SerializeField] private Image _soundIconOff;
        [SerializeField] private Image _musicIconOn;
        [SerializeField] private Image _musicIconOff;
        
        [Header("Animation")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private float _appearAnimationDuration = 0.3f;
        
        [Header("Background")]
        [SerializeField] private Image _backgroundOverlay;
        [SerializeField] private Button _backgroundButton;
        
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup _popupCanvasGroup;
        
        private Coroutine _animationCoroutine;
        
        /// <summary>
        /// Событие продолжения игры
        /// </summary>
        public event Action OnResumeClicked;
        
        /// <summary>
        /// Событие перезапуска
        /// </summary>
        public event Action OnRestartClicked;
        
        /// <summary>
        /// Событие настроек
        /// </summary>
        public event Action OnSettingsClicked;
        
        /// <summary>
        /// Событие главного меню
        /// </summary>
        public event Action OnMainMenuClicked;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Подписываемся на кнопки
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(HandleResumeClick);
            }
            
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClick);
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(HandleSettingsClick);
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(HandleMainMenuClick);
            }
            
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.AddListener(HandleResumeClick);
            }
            
            // Подписываемся на toggles
            if (_soundToggle != null)
            {
                _soundToggle.onValueChanged.AddListener(HandleSoundToggle);
            }
            
            if (_musicToggle != null)
            {
                _musicToggle.onValueChanged.AddListener(HandleMusicToggle);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(HandleResumeClick);
            }
            
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClick);
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(HandleSettingsClick);
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(HandleMainMenuClick);
            }
            
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.RemoveListener(HandleResumeClick);
            }
            
            if (_soundToggle != null)
            {
                _soundToggle.onValueChanged.RemoveListener(HandleSoundToggle);
            }
            
            if (_musicToggle != null)
            {
                _musicToggle.onValueChanged.RemoveListener(HandleMusicToggle);
            }
            
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }
        
        /// <summary>
        /// Вызывается при показе popup-а
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Обновляем состояние toggles
            UpdateSoundToggles();
            
            // Запускаем анимацию появления
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            _animationCoroutine = StartCoroutine(AnimateAppear());
        }
        
        /// <summary>
        /// Вызывается при скрытии popup-а
        /// </summary>
        public override void OnScreenHide()
        {
            base.OnScreenHide();
            
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
        
        private void UpdateSoundToggles()
        {
            // TODO: Получить состояние из AudioManager (Фаза 9)
            // Пока используем PlayerPrefs
            
            bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
            bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            
            if (_soundToggle != null)
            {
                _soundToggle.SetIsOnWithoutNotify(soundEnabled);
            }
            
            if (_musicToggle != null)
            {
                _musicToggle.SetIsOnWithoutNotify(musicEnabled);
            }
            
            UpdateSoundIcons(soundEnabled);
            UpdateMusicIcons(musicEnabled);
        }
        
        private void UpdateSoundIcons(bool enabled)
        {
            if (_soundIconOn != null)
            {
                _soundIconOn.gameObject.SetActive(enabled);
            }
            
            if (_soundIconOff != null)
            {
                _soundIconOff.gameObject.SetActive(!enabled);
            }
        }
        
        private void UpdateMusicIcons(bool enabled)
        {
            if (_musicIconOn != null)
            {
                _musicIconOn.gameObject.SetActive(enabled);
            }
            
            if (_musicIconOff != null)
            {
                _musicIconOff.gameObject.SetActive(!enabled);
            }
        }
        
        private void HandleResumeClick()
        {
            OnResumeClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Продолжаем игру
            GameManager.Instance?.Resume();
        }
        
        private void HandleRestartClick()
        {
            OnRestartClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Перезапускаем игру
            GameManager.Instance?.Restart();
        }
        
        private void HandleSettingsClick()
        {
            OnSettingsClicked?.Invoke();
            
            // TODO: Открыть настройки (Фаза 9)
        }
        
        private void HandleMainMenuClick()
        {
            OnMainMenuClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Возвращаемся в главное меню
            GameManager.Instance?.QuitToMenu();
            UIManager.Instance?.ShowScreen(Constants.Screens.MAIN_MENU);
        }
        
        private void HandleSoundToggle(bool enabled)
        {
            PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            UpdateSoundIcons(enabled);
            
            // TODO: Применить к AudioManager (Фаза 9)
        }
        
        private void HandleMusicToggle(bool enabled)
        {
            PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            UpdateMusicIcons(enabled);
            
            // TODO: Применить к AudioManager (Фаза 9)
        }
        
        private IEnumerator AnimateAppear()
        {
            // Начальное состояние
            if (_contentContainer != null)
            {
                _contentContainer.localScale = Vector3.one * 0.8f;
            }
            
            if (_backgroundOverlay != null)
            {
                Color bgColor = _backgroundOverlay.color;
                bgColor.a = 0f;
                _backgroundOverlay.color = bgColor;
            }
            
            if (_popupCanvasGroup != null)
            {
                _popupCanvasGroup.alpha = 0f;
            }
            
            float elapsed = 0f;
            
            while (elapsed < _appearAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // unscaled для работы на паузе
                float t = elapsed / _appearAnimationDuration;
                float easedT = EaseOutQuad(t);
                
                if (_backgroundOverlay != null)
                {
                    Color bgColor = _backgroundOverlay.color;
                    bgColor.a = Mathf.Lerp(0f, 0.7f, easedT);
                    _backgroundOverlay.color = bgColor;
                }
                
                if (_contentContainer != null)
                {
                    float scale = Mathf.Lerp(0.8f, 1f, easedT);
                    _contentContainer.localScale = Vector3.one * scale;
                }
                
                if (_popupCanvasGroup != null)
                {
                    _popupCanvasGroup.alpha = easedT;
                }
                
                yield return null;
            }
            
            // Финальное состояние
            if (_contentContainer != null)
            {
                _contentContainer.localScale = Vector3.one;
            }
            
            if (_popupCanvasGroup != null)
            {
                _popupCanvasGroup.alpha = 1f;
            }
            
            _animationCoroutine = null;
        }
        
        /// <summary>
        /// Ease Out Quad функция
        /// </summary>
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }
}
