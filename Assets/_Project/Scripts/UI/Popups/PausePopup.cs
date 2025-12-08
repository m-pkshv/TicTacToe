// Assets/_Project/Scripts/UI/Popups/PausePopup.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TicTacToe.Core;
using TicTacToe.Save;
using TicTacToe.Utils;

namespace TicTacToe.UI.Popups
{
    /// <summary>
    /// Popup меню паузы.
    /// Отображает кнопки управления игрой и настройки звука.
    /// </summary>
    public class PausePopup : BaseScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _backgroundButton;
        
        [Header("Sound Toggles")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _musicToggle;
        
        [Header("Sound Icons")]
        [SerializeField] private Image _soundIconOn;
        [SerializeField] private Image _soundIconOff;
        [SerializeField] private Image _musicIconOn;
        [SerializeField] private Image _musicIconOff;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private float _animationDuration = 0.25f;
        
        // События
        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnSettingsClicked;
        public event Action OnMainMenuClicked;
        
        private Coroutine _animationCoroutine;
        
        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            SetupToggles();
        }
        
        private void OnDestroy()
        {
            RemoveListeners();
        }
        
        private void SetupButtons()
        {
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
        }
        
        private void SetupToggles()
        {
            if (_soundToggle != null)
            {
                _soundToggle.onValueChanged.AddListener(HandleSoundToggle);
            }
            
            if (_musicToggle != null)
            {
                _musicToggle.onValueChanged.AddListener(HandleMusicToggle);
            }
        }
        
        private void RemoveListeners()
        {
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
        }
        
        /// <summary>
        /// Вызывается при показе popup-а
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Обновляем состояние toggles из SaveSystem
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
        
        /// <summary>
        /// Обновляет состояние toggles из SaveSystem
        /// </summary>
        private void UpdateSoundToggles()
        {
            // Получаем настройки из SaveSystem
            var (musicEnabled, sfxEnabled, _, _) = SaveSystem.Instance?.GetSoundSettings() 
                ?? (true, true, 0.8f, 1.0f);
            
            // Устанавливаем значения без вызова события
            if (_soundToggle != null)
            {
                _soundToggle.SetIsOnWithoutNotify(sfxEnabled);
            }
            
            if (_musicToggle != null)
            {
                _musicToggle.SetIsOnWithoutNotify(musicEnabled);
            }
            
            // Обновляем иконки
            UpdateSoundIcons(sfxEnabled);
            UpdateMusicIcons(musicEnabled);
        }
        
        /// <summary>
        /// Обновляет иконки звуковых эффектов
        /// </summary>
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
        
        /// <summary>
        /// Обновляет иконки музыки
        /// </summary>
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
        
        // ==================== Button Handlers ====================
        
        private void HandleResumeClick()
        {
            PlayButtonSound();
            OnResumeClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Продолжаем игру
            GameManager.Instance?.Resume();
        }
        
        private void HandleRestartClick()
        {
            PlayButtonSound();
            OnRestartClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Перезапускаем игру
            GameManager.Instance?.Restart();
        }
        
        private void HandleSettingsClick()
        {
            PlayButtonSound();
            OnSettingsClicked?.Invoke();
            
            // TODO: Открыть полноценные настройки
        }
        
        private void HandleMainMenuClick()
        {
            PlayButtonSound();
            OnMainMenuClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.PAUSE);
            
            // Возвращаемся в главное меню
            GameManager.Instance?.QuitToMenu();
            UIManager.Instance?.ShowScreen(Constants.Screens.MAIN_MENU);
        }
        
        // ==================== Toggle Handlers ====================
        
        /// <summary>
        /// Обработчик переключения звуковых эффектов
        /// </summary>
        private void HandleSoundToggle(bool enabled)
        {
            // Сохраняем в SaveSystem
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                var (musicEnabled, _, musicVol, sfxVol) = saveSystem.GetSoundSettings();
                saveSystem.SetSoundSettings(musicEnabled, enabled, musicVol, sfxVol);
            }
            
            // Обновляем иконки
            UpdateSoundIcons(enabled);
            
            // Применяем к AudioManager
            // TODO: Фаза 9
            // AudioManager.Instance?.SetSfxEnabled(enabled);
            
            Debug.Log($"[PausePopup] Sound effects: {(enabled ? "ON" : "OFF")}");
        }
        
        /// <summary>
        /// Обработчик переключения музыки
        /// </summary>
        private void HandleMusicToggle(bool enabled)
        {
            // Сохраняем в SaveSystem
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                var (_, sfxEnabled, musicVol, sfxVol) = saveSystem.GetSoundSettings();
                saveSystem.SetSoundSettings(enabled, sfxEnabled, musicVol, sfxVol);
            }
            
            // Обновляем иконки
            UpdateMusicIcons(enabled);
            
            // Применяем к AudioManager
            // TODO: Фаза 9
            // AudioManager.Instance?.SetMusicEnabled(enabled);
            
            Debug.Log($"[PausePopup] Music: {(enabled ? "ON" : "OFF")}");
        }
        
        // ==================== Animation ====================
        
        /// <summary>
        /// Анимация появления popup-а
        /// </summary>
        private IEnumerator AnimateAppear()
        {
            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 0f;
            }
            
            if (_panelTransform != null)
            {
                _panelTransform.localScale = Vector3.one * 0.8f;
            }
            
            float elapsed = 0f;
            
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Используем unscaled для работы на паузе
                float t = elapsed / _animationDuration;
                
                // Ease out back
                float overshoot = 1.5f;
                t = 1f + (overshoot + 1f) * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
                t = Mathf.Clamp01(t);
                
                if (_panelCanvasGroup != null)
                {
                    _panelCanvasGroup.alpha = t;
                }
                
                if (_panelTransform != null)
                {
                    _panelTransform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
                }
                
                yield return null;
            }
            
            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 1f;
            }
            
            if (_panelTransform != null)
            {
                _panelTransform.localScale = Vector3.one;
            }
            
            _animationCoroutine = null;
        }
        
        // ==================== Helpers ====================
        
        /// <summary>
        /// Воспроизводит звук нажатия кнопки
        /// </summary>
        private void PlayButtonSound()
        {
            // TODO: Фаза 9 — AudioManager
            // AudioManager.Instance?.PlaySfx(SoundType.ButtonClick);
        }
    }
}
