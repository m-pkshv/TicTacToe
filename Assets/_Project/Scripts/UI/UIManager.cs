// Assets/_Project/Scripts/UI/UIManager.cs

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TicTacToe.Utils;

namespace TicTacToe.UI
{
    /// <summary>
    /// Центральный менеджер UI системы.
    /// Управляет экранами, навигацией, popup-ами и переходами между экранами.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // ==================== КОНСТАНТЫ ====================
        
        private const float DEFAULT_TRANSITION_DURATION = 0.3f;
        
        // ==================== SERIALIZED FIELDS ====================
        
        [Header("Screen Settings")]
        [SerializeField]
        [Tooltip("Родительский объект для всех экранов")]
        private Transform _screensContainer;
        
        [SerializeField]
        [Tooltip("Родительский объект для всех popup-ов")]
        private Transform _popupsContainer;
        
        [SerializeField]
        [Tooltip("ID экрана, который показывается при старте")]
        private string _startScreenId;
        
        [Header("Transition Settings")]
        [SerializeField]
        [Tooltip("Длительность перехода между экранами")]
        private float _transitionDuration = DEFAULT_TRANSITION_DURATION;
        
        [SerializeField]
        [Tooltip("Кривая анимации перехода")]
        private AnimationCurve _transitionCurve;
        
        [SerializeField]
        [Tooltip("Использовать ли анимации переходов")]
        private bool _useTransitions = true;
        
        [Header("Debug")]
        [SerializeField]
        private bool _debugMode = false;
        
        // ==================== PRIVATE FIELDS ====================
        
        /// <summary>Словарь всех зарегистрированных экранов</summary>
        private readonly Dictionary<string, BaseScreen> _screens = new Dictionary<string, BaseScreen>();
        
        /// <summary>Словарь всех зарегистрированных popup-ов</summary>
        private readonly Dictionary<string, BaseScreen> _popups = new Dictionary<string, BaseScreen>();
        
        /// <summary>Стек экранов для навигации назад</summary>
        private readonly Stack<BaseScreen> _screenStack = new Stack<BaseScreen>();
        
        /// <summary>Список активных popup-ов</summary>
        private readonly List<BaseScreen> _activePopups = new List<BaseScreen>();
        
        /// <summary>Выполняется ли сейчас переход между экранами</summary>
        private bool _isTransitioning;
        
        /// <summary>Корутина текущего перехода</summary>
        private Coroutine _transitionCoroutine;
        
        // ==================== СОБЫТИЯ ====================
        
        /// <summary>Вызывается перед сменой экрана</summary>
        public event Action<BaseScreen, BaseScreen> OnScreenChanging;
        
        /// <summary>Вызывается после смены экрана</summary>
        public event Action<BaseScreen> OnScreenChanged;
        
        /// <summary>Вызывается при показе popup-а</summary>
        public event Action<BaseScreen> OnPopupShown;
        
        /// <summary>Вызывается при скрытии popup-а</summary>
        public event Action<BaseScreen> OnPopupHidden;
        
        /// <summary>Вызывается при нажатии кнопки Back (Android)</summary>
        public event Action OnBackPressed;
        
        // ==================== СВОЙСТВА ====================
        
        /// <summary>Текущий активный экран</summary>
        public BaseScreen CurrentScreen { get; private set; }
        
        /// <summary>Предыдущий экран</summary>
        public BaseScreen PreviousScreen { get; private set; }
        
        /// <summary>Выполняется ли переход между экранами</summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>Есть ли экраны в стеке для возврата</summary>
        public bool CanGoBack => _screenStack.Count > 0;
        
        /// <summary>Количество экранов в стеке</summary>
        public int StackCount => _screenStack.Count;
        
        /// <summary>Есть ли активные popup-ы</summary>
        public bool HasActivePopups => _activePopups.Count > 0;
        
        /// <summary>Длительность перехода между экранами</summary>
        public float TransitionDuration => _transitionDuration;
        
        // ==================== UNITY LIFECYCLE ====================
        
        protected override void Awake()
        {
            base.Awake();
            
            // Инициализация кривой анимации по умолчанию
            if (_transitionCurve == null || _transitionCurve.keys.Length == 0)
            {
                _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
        
        private void Start()
        {
            // Автоматически находим и регистрируем все экраны, если контейнер задан
            if (_screensContainer != null)
            {
                AutoRegisterScreens(_screensContainer, false);
            }
            
            if (_popupsContainer != null)
            {
                AutoRegisterScreens(_popupsContainer, true);
            }
            
            // Показываем стартовый экран
            if (!string.IsNullOrEmpty(_startScreenId))
            {
                ShowScreen(_startScreenId, false);
            }
        }
        
        private void Update()
        {
            // Обработка кнопки Back на Android
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }
        
        // ==================== ПУБЛИЧНЫЕ МЕТОДЫ: РЕГИСТРАЦИЯ ====================
        
        /// <summary>
        /// Регистрирует экран в системе UI.
        /// </summary>
        /// <param name="screen">Экран для регистрации</param>
        public void RegisterScreen(BaseScreen screen)
        {
            if (screen == null)
            {
                Debug.LogWarning("[UIManager] Попытка зарегистрировать null экран");
                return;
            }
            
            string screenId = screen.ScreenId;
            
            if (string.IsNullOrEmpty(screenId))
            {
                Debug.LogWarning($"[UIManager] Экран {screen.gameObject.name} имеет пустой ScreenId");
                return;
            }
            
            if (_screens.ContainsKey(screenId))
            {
                LogDebug($"Экран с ID '{screenId}' уже зарегистрирован, перезаписываем");
            }
            
            _screens[screenId] = screen;
            
            // Скрываем экран при регистрации (если это не текущий)
            if (CurrentScreen != screen)
            {
                screen.SetVisibleImmediate(false);
            }
            
            LogDebug($"Зарегистрирован экран: {screenId}");
        }
        
        /// <summary>
        /// Регистрирует popup в системе UI.
        /// </summary>
        /// <param name="popup">Popup для регистрации</param>
        public void RegisterPopup(BaseScreen popup)
        {
            if (popup == null)
            {
                Debug.LogWarning("[UIManager] Попытка зарегистрировать null popup");
                return;
            }
            
            string popupId = popup.ScreenId;
            
            if (string.IsNullOrEmpty(popupId))
            {
                Debug.LogWarning($"[UIManager] Popup {popup.gameObject.name} имеет пустой ScreenId");
                return;
            }
            
            if (_popups.ContainsKey(popupId))
            {
                LogDebug($"Popup с ID '{popupId}' уже зарегистрирован, перезаписываем");
            }
            
            _popups[popupId] = popup;
            popup.SetVisibleImmediate(false);
            
            LogDebug($"Зарегистрирован popup: {popupId}");
        }
        
        /// <summary>
        /// Отменяет регистрацию экрана.
        /// </summary>
        /// <param name="screenId">ID экрана</param>
        public void UnregisterScreen(string screenId)
        {
            if (_screens.ContainsKey(screenId))
            {
                _screens.Remove(screenId);
                LogDebug($"Экран {screenId} удалён из регистрации");
            }
        }
        
        // ==================== ПУБЛИЧНЫЕ МЕТОДЫ: НАВИГАЦИЯ ====================
        
        /// <summary>
        /// Показывает экран по его ID.
        /// </summary>
        /// <param name="screenId">ID экрана</param>
        /// <param name="addToStack">Добавлять ли текущий экран в стек навигации</param>
        public void ShowScreen(string screenId, bool addToStack = true)
        {
            if (string.IsNullOrEmpty(screenId))
            {
                Debug.LogWarning("[UIManager] Попытка показать экран с пустым ID");
                return;
            }
            
            if (!_screens.TryGetValue(screenId, out BaseScreen targetScreen))
            {
                Debug.LogWarning($"[UIManager] Экран '{screenId}' не найден");
                return;
            }
            
            ShowScreenInternal(targetScreen, addToStack);
        }
        
        /// <summary>
        /// Показывает экран по его типу.
        /// </summary>
        /// <typeparam name="T">Тип экрана</typeparam>
        /// <param name="addToStack">Добавлять ли текущий экран в стек навигации</param>
        public void ShowScreen<T>(bool addToStack = true) where T : BaseScreen
        {
            BaseScreen targetScreen = null;
            
            foreach (var screen in _screens.Values)
            {
                if (screen is T)
                {
                    targetScreen = screen;
                    break;
                }
            }
            
            if (targetScreen == null)
            {
                Debug.LogWarning($"[UIManager] Экран типа '{typeof(T).Name}' не найден");
                return;
            }
            
            ShowScreenInternal(targetScreen, addToStack);
        }
        
        /// <summary>
        /// Возвращает на предыдущий экран из стека.
        /// </summary>
        /// <returns>True, если переход выполнен</returns>
        public bool GoBack()
        {
            // Сначала закрываем popup-ы, если они есть
            if (_activePopups.Count > 0)
            {
                HideTopPopup();
                return true;
            }
            
            if (_screenStack.Count == 0)
            {
                LogDebug("Стек навигации пуст, возврат невозможен");
                return false;
            }
            
            if (_isTransitioning)
            {
                LogDebug("Переход уже выполняется, игнорируем GoBack");
                return false;
            }
            
            BaseScreen previousScreen = _screenStack.Pop();
            ShowScreenInternal(previousScreen, false);
            
            return true;
        }
        
        /// <summary>
        /// Очищает стек навигации.
        /// </summary>
        public void ClearStack()
        {
            _screenStack.Clear();
            LogDebug("Стек навигации очищен");
        }
        
        /// <summary>
        /// Возвращает к корневому экрану, очищая весь стек.
        /// </summary>
        /// <param name="rootScreenId">ID корневого экрана</param>
        public void GoToRoot(string rootScreenId)
        {
            ClearStack();
            ShowScreen(rootScreenId, false);
        }
        
        // ==================== ПУБЛИЧНЫЕ МЕТОДЫ: POPUP-Ы ====================
        
        /// <summary>
        /// Показывает popup по его ID.
        /// </summary>
        /// <param name="popupId">ID popup-а</param>
        public void ShowPopup(string popupId)
        {
            if (string.IsNullOrEmpty(popupId))
            {
                Debug.LogWarning("[UIManager] Попытка показать popup с пустым ID");
                return;
            }
            
            if (!_popups.TryGetValue(popupId, out BaseScreen popup))
            {
                Debug.LogWarning($"[UIManager] Popup '{popupId}' не найден");
                return;
            }
            
            ShowPopupInternal(popup);
        }
        
        /// <summary>
        /// Показывает popup по его типу.
        /// </summary>
        /// <typeparam name="T">Тип popup-а</typeparam>
        public void ShowPopup<T>() where T : BaseScreen
        {
            BaseScreen targetPopup = null;
            
            foreach (var popup in _popups.Values)
            {
                if (popup is T)
                {
                    targetPopup = popup;
                    break;
                }
            }
            
            if (targetPopup == null)
            {
                Debug.LogWarning($"[UIManager] Popup типа '{typeof(T).Name}' не найден");
                return;
            }
            
            ShowPopupInternal(targetPopup);
        }
        
        /// <summary>
        /// Скрывает popup по его ID.
        /// </summary>
        /// <param name="popupId">ID popup-а</param>
        public void HidePopup(string popupId)
        {
            if (!_popups.TryGetValue(popupId, out BaseScreen popup))
            {
                Debug.LogWarning($"[UIManager] Popup '{popupId}' не найден для скрытия");
                return;
            }
            
            HidePopupInternal(popup);
        }
        
        /// <summary>
        /// Скрывает верхний popup из стека активных.
        /// </summary>
        public void HideTopPopup()
        {
            if (_activePopups.Count > 0)
            {
                BaseScreen topPopup = _activePopups[_activePopups.Count - 1];
                HidePopupInternal(topPopup);
            }
        }
        
        /// <summary>
        /// Скрывает все активные popup-ы.
        /// </summary>
        public void HideAllPopups()
        {
            // Создаём копию списка, т.к. он будет изменяться
            var popupsToHide = new List<BaseScreen>(_activePopups);
            
            foreach (var popup in popupsToHide)
            {
                HidePopupInternal(popup);
            }
        }
        
        // ==================== ПУБЛИЧНЫЕ МЕТОДЫ: ПОЛУЧЕНИЕ ЭКРАНОВ ====================
        
        /// <summary>
        /// Получает экран по ID.
        /// </summary>
        /// <param name="screenId">ID экрана</param>
        /// <returns>Экран или null, если не найден</returns>
        public BaseScreen GetScreen(string screenId)
        {
            _screens.TryGetValue(screenId, out BaseScreen screen);
            return screen;
        }
        
        /// <summary>
        /// Получает экран по типу.
        /// </summary>
        /// <typeparam name="T">Тип экрана</typeparam>
        /// <returns>Экран или null, если не найден</returns>
        public T GetScreen<T>() where T : BaseScreen
        {
            foreach (var screen in _screens.Values)
            {
                if (screen is T typedScreen)
                {
                    return typedScreen;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Проверяет, зарегистрирован ли экран с данным ID.
        /// </summary>
        /// <param name="screenId">ID экрана</param>
        /// <returns>True, если экран зарегистрирован</returns>
        public bool HasScreen(string screenId)
        {
            return _screens.ContainsKey(screenId);
        }
        
        // ==================== ПРИВАТНЫЕ МЕТОДЫ ====================
        
        /// <summary>
        /// Внутренний метод показа экрана.
        /// </summary>
        private void ShowScreenInternal(BaseScreen targetScreen, bool addToStack)
        {
            if (targetScreen == null)
            {
                return;
            }
            
            // Не переходим на тот же экран
            if (CurrentScreen == targetScreen)
            {
                LogDebug($"Экран {targetScreen.ScreenId} уже активен");
                return;
            }
            
            // Прерываем текущий переход
            if (_isTransitioning && _transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _isTransitioning = false;
            }
            
            // Добавляем текущий экран в стек, если требуется
            if (addToStack && CurrentScreen != null && CurrentScreen.AllowBackNavigation)
            {
                _screenStack.Push(CurrentScreen);
            }
            
            // Запоминаем предыдущий экран
            PreviousScreen = CurrentScreen;
            
            // Вызываем событие
            OnScreenChanging?.Invoke(CurrentScreen, targetScreen);
            
            // Выполняем переход
            if (_useTransitions && gameObject.activeInHierarchy)
            {
                _transitionCoroutine = StartCoroutine(TransitionScreens(CurrentScreen, targetScreen));
            }
            else
            {
                // Мгновенный переход без анимации
                CurrentScreen?.OnScreenHide();
                CurrentScreen = targetScreen;
                CurrentScreen.OnScreenShow();
                OnScreenChanged?.Invoke(CurrentScreen);
            }
            
            LogDebug($"Переход на экран: {targetScreen.ScreenId}");
        }
        
        /// <summary>
        /// Корутина перехода между экранами с анимацией.
        /// </summary>
        private IEnumerator TransitionScreens(BaseScreen from, BaseScreen to)
        {
            _isTransitioning = true;
            
            // Показываем новый экран
            to.gameObject.SetActive(true);
            
            // Параллельно выполняем анимации
            Coroutine animateOut = null;
            Coroutine animateIn = null;
            
            if (from != null)
            {
                animateOut = StartCoroutine(from.AnimateOut());
            }
            
            animateIn = StartCoroutine(to.AnimateIn());
            
            // Ждём завершения анимаций
            if (animateOut != null)
            {
                yield return animateOut;
            }
            yield return animateIn;
            
            // Скрываем старый экран
            if (from != null)
            {
                from.OnScreenHide();
            }
            
            // Обновляем состояние
            CurrentScreen = to;
            to.OnScreenShow();
            
            _isTransitioning = false;
            _transitionCoroutine = null;
            
            OnScreenChanged?.Invoke(CurrentScreen);
        }
        
        /// <summary>
        /// Внутренний метод показа popup-а.
        /// </summary>
        private void ShowPopupInternal(BaseScreen popup)
        {
            if (popup == null || _activePopups.Contains(popup))
            {
                return;
            }
            
            // Приостанавливаем текущий экран
            CurrentScreen?.OnScreenPause();
            
            // Показываем popup
            _activePopups.Add(popup);
            popup.OnScreenShow();
            
            if (_useTransitions && gameObject.activeInHierarchy)
            {
                StartCoroutine(popup.AnimateIn());
            }
            
            OnPopupShown?.Invoke(popup);
            LogDebug($"Показан popup: {popup.ScreenId}");
        }
        
        /// <summary>
        /// Внутренний метод скрытия popup-а.
        /// </summary>
        private void HidePopupInternal(BaseScreen popup)
        {
            if (popup == null || !_activePopups.Contains(popup))
            {
                return;
            }
            
            _activePopups.Remove(popup);
            
            if (_useTransitions && gameObject.activeInHierarchy)
            {
                StartCoroutine(HidePopupCoroutine(popup));
            }
            else
            {
                popup.OnScreenHide();
                OnPopupHiddenInternal(popup);
            }
        }
        
        /// <summary>
        /// Корутина скрытия popup-а с анимацией.
        /// </summary>
        private IEnumerator HidePopupCoroutine(BaseScreen popup)
        {
            yield return StartCoroutine(popup.AnimateOut());
            popup.OnScreenHide();
            OnPopupHiddenInternal(popup);
        }
        
        /// <summary>
        /// Вызывается после скрытия popup-а.
        /// </summary>
        private void OnPopupHiddenInternal(BaseScreen popup)
        {
            // Возобновляем экран, если нет больше popup-ов
            if (_activePopups.Count == 0)
            {
                CurrentScreen?.OnScreenResume();
            }
            
            OnPopupHidden?.Invoke(popup);
            LogDebug($"Скрыт popup: {popup.ScreenId}");
        }
        
        /// <summary>
        /// Автоматически регистрирует все экраны в контейнере.
        /// </summary>
        private void AutoRegisterScreens(Transform container, bool asPopups)
        {
            BaseScreen[] screens = container.GetComponentsInChildren<BaseScreen>(true);
            
            foreach (var screen in screens)
            {
                if (asPopups)
                {
                    RegisterPopup(screen);
                }
                else
                {
                    RegisterScreen(screen);
                }
            }
            
            LogDebug($"Авто-регистрация: найдено {screens.Length} {(asPopups ? "popup-ов" : "экранов")}");
        }
        
        /// <summary>
        /// Обработка кнопки Back (Android / Escape).
        /// </summary>
        private void HandleBackButton()
        {
            OnBackPressed?.Invoke();
            
            // Сначала пробуем закрыть popup
            if (_activePopups.Count > 0)
            {
                HideTopPopup();
                return;
            }
            
            // Затем пробуем вернуться назад
            if (CurrentScreen != null && CurrentScreen.AllowBackNavigation)
            {
                GoBack();
            }
        }
        
        /// <summary>
        /// Логирование в режиме отладки.
        /// </summary>
        private void LogDebug(string message)
        {
            if (_debugMode)
            {
                Debug.Log($"[UIManager] {message}");
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Отображение отладочной информации в Inspector.
        /// </summary>
        [ContextMenu("Log Registered Screens")]
        private void LogRegisteredScreens()
        {
            Debug.Log("=== Зарегистрированные экраны ===");
            foreach (var kvp in _screens)
            {
                Debug.Log($"  - {kvp.Key}: {(kvp.Value != null ? kvp.Value.gameObject.name : "NULL")}");
            }
            
            Debug.Log("=== Зарегистрированные popup-ы ===");
            foreach (var kvp in _popups)
            {
                Debug.Log($"  - {kvp.Key}: {(kvp.Value != null ? kvp.Value.gameObject.name : "NULL")}");
            }
            
            Debug.Log($"=== Стек навигации: {_screenStack.Count} экранов ===");
            Debug.Log($"=== Текущий экран: {(CurrentScreen != null ? CurrentScreen.ScreenId : "NULL")} ===");
        }
#endif
    }
}
