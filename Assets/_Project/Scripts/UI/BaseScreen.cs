// Assets/_Project/Scripts/UI/BaseScreen.cs

using System;
using System.Collections;
using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// Базовый абстрактный класс для всех UI экранов.
    /// Предоставляет общую функциональность для управления жизненным циклом экрана,
    /// анимациями переходов и навигацией.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        // ==================== КОНСТАНТЫ ====================
        
        /// <summary>Длительность анимации появления/скрытия по умолчанию</summary>
        protected const float DEFAULT_ANIMATION_DURATION = 0.3f;
        
        // ==================== SERIALIZED FIELDS ====================
        
        [Header("Screen Settings")]
        [SerializeField] 
        [Tooltip("Уникальный идентификатор экрана")]
        protected string _screenId;
        
        [SerializeField] 
        [Tooltip("Показывать ли баннер на этом экране")]
        protected bool _showBanner = true;
        
        [SerializeField] 
        [Tooltip("Можно ли вернуться с этого экрана кнопкой Назад")]
        protected bool _allowBackNavigation = true;
        
        [Header("Animation Settings")]
        [SerializeField] 
        [Tooltip("Длительность анимации перехода")]
        protected float _animationDuration = DEFAULT_ANIMATION_DURATION;
        
        [SerializeField] 
        [Tooltip("Кривая анимации (опционально)")]
        protected AnimationCurve _animationCurve;
        
        // ==================== PRIVATE FIELDS ====================
        
        private CanvasGroup _canvasGroup;
        private bool _isAnimating;
        
        // ==================== СОБЫТИЯ ====================
        
        /// <summary>Вызывается перед показом экрана</summary>
        public event Action OnBeforeShow;
        
        /// <summary>Вызывается после показа экрана</summary>
        public event Action OnAfterShow;
        
        /// <summary>Вызывается перед скрытием экрана</summary>
        public event Action OnBeforeHide;
        
        /// <summary>Вызывается после скрытия экрана</summary>
        public event Action OnAfterHide;
        
        // ==================== СВОЙСТВА ====================
        
        /// <summary>Уникальный идентификатор экрана</summary>
        public string ScreenId => _screenId;
        
        /// <summary>Нужно ли показывать баннер на этом экране</summary>
        public bool ShowBanner => _showBanner;
        
        /// <summary>Разрешена ли навигация назад</summary>
        public bool AllowBackNavigation => _allowBackNavigation;
        
        /// <summary>Виден ли экран в данный момент</summary>
        public bool IsVisible { get; private set; }
        
        /// <summary>Выполняется ли сейчас анимация</summary>
        public bool IsAnimating => _isAnimating;
        
        /// <summary>CanvasGroup компонент экрана</summary>
        protected CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }
        
        // ==================== UNITY LIFECYCLE ====================
        
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Применяем дефолтную кривую анимации, если не задана
            if (_animationCurve == null || _animationCurve.keys.Length == 0)
            {
                _animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
            
            // Валидация screenId
            if (string.IsNullOrEmpty(_screenId))
            {
                _screenId = GetType().Name;
                Debug.LogWarning($"[BaseScreen] ScreenId не задан для {gameObject.name}, " +
                                 $"используется имя класса: {_screenId}");
            }
        }
        
        protected virtual void Start()
        {
            // Регистрируем экран в UIManager при старте
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterScreen(this);
            }
        }
        
        protected virtual void OnDestroy()
        {
            // Отписываемся от событий
            OnBeforeShow = null;
            OnAfterShow = null;
            OnBeforeHide = null;
            OnAfterHide = null;
        }
        
        // ==================== LIFECYCLE МЕТОДЫ ====================
        
        /// <summary>
        /// Вызывается при показе экрана.
        /// Переопределите для добавления логики инициализации UI.
        /// </summary>
        public virtual void OnScreenShow()
        {
            OnBeforeShow?.Invoke();
            
            gameObject.SetActive(true);
            IsVisible = true;
            
            // Управление баннером
            // TODO: Раскомментировать после реализации AdsManager (Фаза 7)
            // if (_showBanner)
            //     AdsManager.Instance?.ShowBanner();
            // else
            //     AdsManager.Instance?.HideBanner();
            
            OnAfterShow?.Invoke();
        }
        
        /// <summary>
        /// Вызывается при скрытии экрана.
        /// Переопределите для очистки данных.
        /// </summary>
        public virtual void OnScreenHide()
        {
            OnBeforeHide?.Invoke();
            
            IsVisible = false;
            gameObject.SetActive(false);
            
            OnAfterHide?.Invoke();
        }
        
        /// <summary>
        /// Вызывается когда экран приостанавливается (например, открыт popup).
        /// </summary>
        public virtual void OnScreenPause()
        {
            // Блокируем интерактивность
            if (CanvasGroup != null)
            {
                CanvasGroup.interactable = false;
            }
        }
        
        /// <summary>
        /// Вызывается когда экран возобновляется после паузы.
        /// </summary>
        public virtual void OnScreenResume()
        {
            // Восстанавливаем интерактивность
            if (CanvasGroup != null)
            {
                CanvasGroup.interactable = true;
            }
        }
        
        // ==================== АНИМАЦИИ ====================
        
        /// <summary>
        /// Анимация появления экрана (fade in).
        /// Переопределите для кастомной анимации.
        /// </summary>
        /// <returns>Корутина анимации</returns>
        public virtual IEnumerator AnimateIn()
        {
            if (_isAnimating)
            {
                yield break;
            }
            
            _isAnimating = true;
            
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            
            float elapsed = 0f;
            
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _animationDuration);
                float curveValue = _animationCurve.Evaluate(normalizedTime);
                
                CanvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            
            _isAnimating = false;
        }
        
        /// <summary>
        /// Анимация скрытия экрана (fade out).
        /// Переопределите для кастомной анимации.
        /// </summary>
        /// <returns>Корутина анимации</returns>
        public virtual IEnumerator AnimateOut()
        {
            if (_isAnimating)
            {
                yield break;
            }
            
            _isAnimating = true;
            
            CanvasGroup.interactable = false;
            
            float elapsed = 0f;
            
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _animationDuration);
                float curveValue = _animationCurve.Evaluate(1f - normalizedTime);
                
                CanvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            CanvasGroup.alpha = 0f;
            
            _isAnimating = false;
        }
        
        // ==================== НАВИГАЦИЯ ====================
        
        /// <summary>
        /// Возвращает на предыдущий экран.
        /// </summary>
        protected void GoBack()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.GoBack();
            }
            else
            {
                Debug.LogWarning("[BaseScreen] UIManager.Instance is null, cannot go back");
            }
        }
        
        /// <summary>
        /// Переходит на указанный экран по ID.
        /// </summary>
        /// <param name="screenId">ID экрана для перехода</param>
        /// <param name="addToStack">Добавлять ли текущий экран в стек навигации</param>
        protected void ShowScreen(string screenId, bool addToStack = true)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen(screenId, addToStack);
            }
            else
            {
                Debug.LogWarning($"[BaseScreen] UIManager.Instance is null, cannot show screen: {screenId}");
            }
        }
        
        /// <summary>
        /// Переходит на указанный экран по типу.
        /// </summary>
        /// <typeparam name="T">Тип экрана</typeparam>
        /// <param name="addToStack">Добавлять ли текущий экран в стек навигации</param>
        protected void ShowScreen<T>(bool addToStack = true) where T : BaseScreen
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen<T>(addToStack);
            }
            else
            {
                Debug.LogWarning($"[BaseScreen] UIManager.Instance is null, cannot show screen: {typeof(T).Name}");
            }
        }
        
        // ==================== УТИЛИТЫ ====================
        
        /// <summary>
        /// Мгновенно устанавливает видимость экрана без анимации.
        /// </summary>
        /// <param name="visible">Видимость</param>
        public void SetVisibleImmediate(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);
            
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = visible ? 1f : 0f;
                CanvasGroup.interactable = visible;
            }
        }
        
        /// <summary>
        /// Устанавливает интерактивность экрана.
        /// </summary>
        /// <param name="interactable">Интерактивен ли экран</param>
        public void SetInteractable(bool interactable)
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.interactable = interactable;
                CanvasGroup.blocksRaycasts = interactable;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Валидация в редакторе.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_screenId))
            {
                _screenId = GetType().Name;
            }
            
            if (_animationDuration < 0f)
            {
                _animationDuration = DEFAULT_ANIMATION_DURATION;
            }
        }
#endif
    }
}
