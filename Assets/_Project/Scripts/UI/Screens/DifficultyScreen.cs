// Assets/_Project/Scripts/UI/Screens/DifficultyScreen.cs

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Game.Enums;
using TicTacToe.AI;
using TicTacToe.Utils;

namespace TicTacToe.UI.Screens
{
    /// <summary>
    /// Экран выбора уровня сложности AI.
    /// Предоставляет три варианта: Лёгкий, Средний, Сложный.
    /// </summary>
    public class DifficultyScreen : BaseScreen
    {
        // Header
        [Header("Header")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        
        // Subtitle
        [Header("Subtitle")]
        [SerializeField] private TextMeshProUGUI _subtitleText;
        
        // Difficulty Buttons (Card Style)
        [Header("Difficulty Cards")]
        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _mediumButton;
        [SerializeField] private Button _hardButton;
        
        // Easy Card Elements
        [Header("Easy Card")]
        [SerializeField] private Image _easyIcon;
        [SerializeField] private TextMeshProUGUI _easyTitleText;
        [SerializeField] private TextMeshProUGUI _easyDescriptionText;
        
        // Medium Card Elements
        [Header("Medium Card")]
        [SerializeField] private Image _mediumIcon;
        [SerializeField] private TextMeshProUGUI _mediumTitleText;
        [SerializeField] private TextMeshProUGUI _mediumDescriptionText;
        
        // Hard Card Elements
        [Header("Hard Card")]
        [SerializeField] private Image _hardIcon;
        [SerializeField] private TextMeshProUGUI _hardTitleText;
        [SerializeField] private TextMeshProUGUI _hardDescriptionText;
        
        // Optional: Statistics per difficulty
        [Header("Statistics (Optional)")]
        [SerializeField] private TextMeshProUGUI _easyStatsText;
        [SerializeField] private TextMeshProUGUI _mediumStatsText;
        [SerializeField] private TextMeshProUGUI _hardStatsText;
        
        // Animation Settings
        [Header("Card Animation")]
        [SerializeField] private float _cardAnimationDelay = 0.1f;
        [SerializeField] private float _cardAnimationDuration = 0.3f;
        
        /// <summary>
        /// Событие при выборе сложности
        /// </summary>
        public event Action<AIDifficulty> OnDifficultySelected;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            SetupCardTexts();
        }

        /// <summary>
        /// Настройка обработчиков кнопок
        /// </summary>
        private void SetupButtons()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(HandleBackClick);
            }
            
            if (_easyButton != null)
            {
                _easyButton.onClick.AddListener(() => HandleDifficultySelect(AIDifficulty.Easy));
            }
            
            if (_mediumButton != null)
            {
                _mediumButton.onClick.AddListener(() => HandleDifficultySelect(AIDifficulty.Medium));
            }
            
            if (_hardButton != null)
            {
                _hardButton.onClick.AddListener(() => HandleDifficultySelect(AIDifficulty.Hard));
            }
        }

        /// <summary>
        /// Настройка текстов карточек по умолчанию
        /// </summary>
        private void SetupCardTexts()
        {
            // Используем AIFactory для получения названий и описаний
            SetCardText(_easyTitleText, AIFactory.GetDifficultyName(AIDifficulty.Easy));
            SetCardText(_easyDescriptionText, AIFactory.GetDifficultyDescription(AIDifficulty.Easy));
            
            SetCardText(_mediumTitleText, AIFactory.GetDifficultyName(AIDifficulty.Medium));
            SetCardText(_mediumDescriptionText, AIFactory.GetDifficultyDescription(AIDifficulty.Medium));
            
            SetCardText(_hardTitleText, AIFactory.GetDifficultyName(AIDifficulty.Hard));
            SetCardText(_hardDescriptionText, AIFactory.GetDifficultyDescription(AIDifficulty.Hard));
        }

        /// <summary>
        /// Безопасная установка текста
        /// </summary>
        private void SetCardText(TextMeshProUGUI textComponent, string text)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        /// <summary>
        /// Вызывается при показе экрана
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Обновляем статистику по каждой сложности
            UpdateDifficultyStatistics();
            
            // Запускаем анимацию появления карточек
            AnimateCardsIn();
        }

        /// <summary>
        /// Анимация появления карточек
        /// </summary>
        private void AnimateCardsIn()
        {
            Button[] cards = { _easyButton, _mediumButton, _hardButton };
            
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    AnimateCard(cards[i], i * _cardAnimationDelay);
                }
            }
        }

        /// <summary>
        /// Анимация отдельной карточки
        /// </summary>
        /// <param name="card">Карточка для анимации</param>
        /// <param name="delay">Задержка перед началом анимации</param>
        private void AnimateCard(Button card, float delay)
        {
            if (card == null) return;
            
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            }
            
            RectTransform rectTransform = card.GetComponent<RectTransform>();
            Vector3 originalScale = rectTransform.localScale;
            
            // Начальное состояние
            canvasGroup.alpha = 0f;
            rectTransform.localScale = originalScale * 0.8f;
            
            // Запускаем анимацию
            StartCoroutine(AnimateCardCoroutine(canvasGroup, rectTransform, originalScale, delay));
        }

        /// <summary>
        /// Корутина анимации карточки
        /// </summary>
        private System.Collections.IEnumerator AnimateCardCoroutine(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector3 targetScale,
            float delay)
        {
            yield return new WaitForSeconds(delay);
            
            float elapsed = 0f;
            Vector3 startScale = rectTransform.localScale;
            
            while (elapsed < _cardAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _cardAnimationDuration;
                
                // Ease out back для эффекта "пружины"
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                canvasGroup.alpha = easedT;
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            rectTransform.localScale = targetScale;
        }

        /// <summary>
        /// Обновление статистики по каждой сложности
        /// </summary>
        private void UpdateDifficultyStatistics()
        {
            // TODO: Фаза 5 - интеграция с SaveSystem
            // Пока скрываем или показываем заглушки
            
            /*
            // Пример реализации после Фазы 5:
            if (SaveSystem.Instance != null)
            {
                var easyStats = SaveSystem.Instance.GetAIStats(AIDifficulty.Easy);
                var mediumStats = SaveSystem.Instance.GetAIStats(AIDifficulty.Medium);
                var hardStats = SaveSystem.Instance.GetAIStats(AIDifficulty.Hard);
                
                SetStatsText(_easyStatsText, easyStats);
                SetStatsText(_mediumStatsText, mediumStats);
                SetStatsText(_hardStatsText, hardStats);
            }
            */
            
            // Пока скрываем статистику
            if (_easyStatsText != null) _easyStatsText.gameObject.SetActive(false);
            if (_mediumStatsText != null) _mediumStatsText.gameObject.SetActive(false);
            if (_hardStatsText != null) _hardStatsText.gameObject.SetActive(false);
        }

        #region Button Click Handlers

        /// <summary>
        /// Обработчик нажатия кнопки "Назад"
        /// </summary>
        private void HandleBackClick()
        {
            PlayButtonSound();
            GoBack();
        }

        /// <summary>
        /// Обработчик выбора сложности
        /// </summary>
        /// <param name="difficulty">Выбранная сложность</param>
        private void HandleDifficultySelect(AIDifficulty difficulty)
        {
            PlayButtonSound();
            
            // Визуальная обратная связь
            AnimateSelectedCard(difficulty);
            
            // Вызываем событие
            OnDifficultySelected?.Invoke(difficulty);
            
            // Запускаем игру через GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGameVsAI(difficulty);
            }
            
            // Переходим на игровой экран с небольшой задержкой для анимации
            StartCoroutine(TransitionToGameDelayed(0.2f));
        }

        /// <summary>
        /// Анимация выбранной карточки
        /// </summary>
        /// <param name="difficulty">Выбранная сложность</param>
        private void AnimateSelectedCard(AIDifficulty difficulty)
        {
            Button selectedButton = difficulty switch
            {
                AIDifficulty.Easy => _easyButton,
                AIDifficulty.Medium => _mediumButton,
                AIDifficulty.Hard => _hardButton,
                _ => null
            };
            
            if (selectedButton != null)
            {
                RectTransform rectTransform = selectedButton.GetComponent<RectTransform>();
                StartCoroutine(PulseAnimation(rectTransform));
            }
        }

        /// <summary>
        /// Корутина пульсирующей анимации
        /// </summary>
        private System.Collections.IEnumerator PulseAnimation(RectTransform rectTransform)
        {
            Vector3 originalScale = rectTransform.localScale;
            Vector3 targetScale = originalScale * 1.05f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            // Увеличиваем
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Возвращаем обратно
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }

        /// <summary>
        /// Отложенный переход на игровой экран
        /// </summary>
        private System.Collections.IEnumerator TransitionToGameDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowScreen(Constants.Screens.GAME);
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
        /// Устанавливает заголовок экрана
        /// </summary>
        /// <param name="title">Заголовок</param>
        public void SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        /// <summary>
        /// Устанавливает подзаголовок экрана
        /// </summary>
        /// <param name="subtitle">Подзаголовок</param>
        public void SetSubtitle(string subtitle)
        {
            if (_subtitleText != null)
            {
                _subtitleText.text = subtitle;
            }
        }

        /// <summary>
        /// Устанавливает иконку для карточки сложности
        /// </summary>
        /// <param name="difficulty">Сложность</param>
        /// <param name="icon">Иконка</param>
        public void SetDifficultyIcon(AIDifficulty difficulty, Sprite icon)
        {
            Image targetIcon = difficulty switch
            {
                AIDifficulty.Easy => _easyIcon,
                AIDifficulty.Medium => _mediumIcon,
                AIDifficulty.Hard => _hardIcon,
                _ => null
            };
            
            if (targetIcon != null)
            {
                targetIcon.sprite = icon;
            }
        }

        /// <summary>
        /// Включает/выключает кнопку сложности
        /// </summary>
        /// <param name="difficulty">Сложность</param>
        /// <param name="interactable">Доступность</param>
        public void SetDifficultyInteractable(AIDifficulty difficulty, bool interactable)
        {
            Button targetButton = difficulty switch
            {
                AIDifficulty.Easy => _easyButton,
                AIDifficulty.Medium => _mediumButton,
                AIDifficulty.Hard => _hardButton,
                _ => null
            };
            
            if (targetButton != null)
            {
                targetButton.interactable = interactable;
            }
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
            }
            
            if (_easyButton != null)
            {
                _easyButton.onClick.RemoveAllListeners();
            }
            
            if (_mediumButton != null)
            {
                _mediumButton.onClick.RemoveAllListeners();
            }
            
            if (_hardButton != null)
            {
                _hardButton.onClick.RemoveAllListeners();
            }
        }

        #endregion
    }
}
