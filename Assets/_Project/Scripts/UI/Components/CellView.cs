// Assets/_Project/Scripts/UI/Components/CellView.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Game.Enums;

namespace TicTacToe.UI.Components
{
    /// <summary>
    /// Компонент отображения одной ячейки игрового поля
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CellView : MonoBehaviour
    {
        private const float SYMBOL_APPEAR_DURATION = 0.2f;
        private const float HIGHLIGHT_PULSE_DURATION = 0.5f;
        private const float HOVER_PREVIEW_ALPHA = 0.3f;
        
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _symbolImage;
        [SerializeField] private Image _highlightImage;
        
        [Header("Symbols")]
        [SerializeField] private Sprite _spriteX;
        [SerializeField] private Sprite _spriteO;
        
        [Header("Colors")]
        [SerializeField] private Color _colorX = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _colorO = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private Color _highlightColor = new Color(1f, 0.84f, 0f, 0.5f);
        [SerializeField] private Color _normalBackgroundColor = new Color(1f, 1f, 1f, 0.1f);
        [SerializeField] private Color _hoverBackgroundColor = new Color(1f, 1f, 1f, 0.2f);
        
        private int _cellIndex;
        private CellState _currentState = CellState.Empty;
        private bool _isInteractable = true;
        private bool _isHighlighted;
        private Coroutine _animationCoroutine;
        private Coroutine _highlightCoroutine;
        
        /// <summary>
        /// Событие клика по ячейке
        /// </summary>
        public event Action<int> OnCellClicked;
        
        /// <summary>
        /// Индекс ячейки (0-8)
        /// </summary>
        public int CellIndex => _cellIndex;
        
        /// <summary>
        /// Текущее состояние ячейки
        /// </summary>
        public CellState CurrentState => _currentState;
        
        /// <summary>
        /// Доступна ли ячейка для клика
        /// </summary>
        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                _isInteractable = value;
                UpdateInteractableState();
            }
        }
        
        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            
            _button.onClick.AddListener(HandleClick);
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
            
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            
            if (_highlightCoroutine != null)
            {
                StopCoroutine(_highlightCoroutine);
            }
        }
        
        /// <summary>
        /// Инициализация ячейки
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8)</param>
        public void Initialize(int index)
        {
            _cellIndex = index;
            ResetCell();
        }
        
        /// <summary>
        /// Установить состояние ячейки
        /// </summary>
        /// <param name="state">Новое состояние</param>
        /// <param name="animate">Анимировать появление символа</param>
        public void SetState(CellState state, bool animate = true)
        {
            if (_currentState == state)
            {
                return;
            }
            
            _currentState = state;
            
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            
            if (state == CellState.Empty)
            {
                _symbolImage.gameObject.SetActive(false);
                _symbolImage.sprite = null;
            }
            else
            {
                _symbolImage.sprite = state == CellState.X ? _spriteX : _spriteO;
                _symbolImage.color = state == CellState.X ? _colorX : _colorO;
                _symbolImage.gameObject.SetActive(true);
                
                if (animate && gameObject.activeInHierarchy)
                {
                    _animationCoroutine = StartCoroutine(AnimateSymbolAppear());
                }
                else
                {
                    _symbolImage.transform.localScale = Vector3.one;
                    SetSymbolAlpha(1f);
                }
            }
            
            UpdateInteractableState();
        }
        
        /// <summary>
        /// Сбросить ячейку в начальное состояние
        /// </summary>
        public void ResetCell()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            
            if (_highlightCoroutine != null)
            {
                StopCoroutine(_highlightCoroutine);
                _highlightCoroutine = null;
            }
            
            _currentState = CellState.Empty;
            _isHighlighted = false;
            
            _symbolImage.gameObject.SetActive(false);
            _symbolImage.sprite = null;
            _symbolImage.transform.localScale = Vector3.one;
            
            if (_highlightImage != null)
            {
                _highlightImage.gameObject.SetActive(false);
            }
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalBackgroundColor;
            }
            
            UpdateInteractableState();
        }
        
        /// <summary>
        /// Подсветить ячейку как часть выигрышной линии
        /// </summary>
        /// <param name="highlight">Включить/выключить подсветку</param>
        public void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;
            
            if (_highlightCoroutine != null)
            {
                StopCoroutine(_highlightCoroutine);
                _highlightCoroutine = null;
            }
            
            if (_highlightImage != null)
            {
                _highlightImage.gameObject.SetActive(highlight);
                
                if (highlight && gameObject.activeInHierarchy)
                {
                    _highlightImage.color = _highlightColor;
                    _highlightCoroutine = StartCoroutine(AnimateHighlightPulse());
                }
            }
        }
        
        /// <summary>
        /// Показать превью символа при наведении
        /// </summary>
        /// <param name="symbol">Символ для превью</param>
        public void ShowHoverPreview(CellState symbol)
        {
            if (_currentState != CellState.Empty || !_isInteractable)
            {
                return;
            }
            
            if (symbol == CellState.Empty)
            {
                HideHoverPreview();
                return;
            }
            
            _symbolImage.sprite = symbol == CellState.X ? _spriteX : _spriteO;
            _symbolImage.color = symbol == CellState.X ? 
                new Color(_colorX.r, _colorX.g, _colorX.b, HOVER_PREVIEW_ALPHA) : 
                new Color(_colorO.r, _colorO.g, _colorO.b, HOVER_PREVIEW_ALPHA);
            _symbolImage.transform.localScale = Vector3.one;
            _symbolImage.gameObject.SetActive(true);
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _hoverBackgroundColor;
            }
        }
        
        /// <summary>
        /// Скрыть превью символа
        /// </summary>
        public void HideHoverPreview()
        {
            if (_currentState != CellState.Empty)
            {
                return;
            }
            
            _symbolImage.gameObject.SetActive(false);
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalBackgroundColor;
            }
        }
        
        /// <summary>
        /// Установить спрайты символов (для поддержки тем)
        /// </summary>
        /// <param name="spriteX">Спрайт X</param>
        /// <param name="spriteO">Спрайт O</param>
        public void SetSymbolSprites(Sprite spriteX, Sprite spriteO)
        {
            _spriteX = spriteX;
            _spriteO = spriteO;
            
            // Обновляем текущий символ, если он установлен
            if (_currentState == CellState.X)
            {
                _symbolImage.sprite = _spriteX;
            }
            else if (_currentState == CellState.O)
            {
                _symbolImage.sprite = _spriteO;
            }
        }
        
        /// <summary>
        /// Установить цвета символов (для поддержки тем)
        /// </summary>
        /// <param name="colorX">Цвет X</param>
        /// <param name="colorO">Цвет O</param>
        public void SetSymbolColors(Color colorX, Color colorO)
        {
            _colorX = colorX;
            _colorO = colorO;
            
            // Обновляем текущий цвет, если символ установлен
            if (_currentState == CellState.X)
            {
                _symbolImage.color = _colorX;
            }
            else if (_currentState == CellState.O)
            {
                _symbolImage.color = _colorO;
            }
        }
        
        private void HandleClick()
        {
            if (!_isInteractable || _currentState != CellState.Empty)
            {
                return;
            }
            
            OnCellClicked?.Invoke(_cellIndex);
        }
        
        private void UpdateInteractableState()
        {
            bool canClick = _isInteractable && _currentState == CellState.Empty;
            _button.interactable = canClick;
        }
        
        private void SetSymbolAlpha(float alpha)
        {
            Color color = _symbolImage.color;
            color.a = alpha;
            _symbolImage.color = color;
        }
        
        private IEnumerator AnimateSymbolAppear()
        {
            // Начальное состояние: маленький и прозрачный
            _symbolImage.transform.localScale = Vector3.zero;
            SetSymbolAlpha(0f);
            
            float elapsed = 0f;
            
            while (elapsed < SYMBOL_APPEAR_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / SYMBOL_APPEAR_DURATION;
                
                // Ease out back для эффекта "пружины"
                float scale = EaseOutBack(t);
                _symbolImage.transform.localScale = Vector3.one * scale;
                
                // Линейное появление альфы
                SetSymbolAlpha(t);
                
                yield return null;
            }
            
            // Финальное состояние
            _symbolImage.transform.localScale = Vector3.one;
            SetSymbolAlpha(1f);
            
            _animationCoroutine = null;
        }
        
        private IEnumerator AnimateHighlightPulse()
        {
            while (_isHighlighted && _highlightImage != null)
            {
                float elapsed = 0f;
                
                // Пульсация: увеличение альфы
                while (elapsed < HIGHLIGHT_PULSE_DURATION / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (HIGHLIGHT_PULSE_DURATION / 2f);
                    float alpha = Mathf.Lerp(0.3f, 0.7f, t);
                    
                    Color color = _highlightColor;
                    color.a = alpha;
                    _highlightImage.color = color;
                    
                    yield return null;
                }
                
                elapsed = 0f;
                
                // Пульсация: уменьшение альфы
                while (elapsed < HIGHLIGHT_PULSE_DURATION / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (HIGHLIGHT_PULSE_DURATION / 2f);
                    float alpha = Mathf.Lerp(0.7f, 0.3f, t);
                    
                    Color color = _highlightColor;
                    color.a = alpha;
                    _highlightImage.color = color;
                    
                    yield return null;
                }
            }
            
            _highlightCoroutine = null;
        }
        
        /// <summary>
        /// Ease Out Back функция для анимации
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }
#endif
    }
}
