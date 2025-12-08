// Assets/_Project/Scripts/UI/Popups/ResultPopup.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Game.Enums;
using TicTacToe.Utils;

namespace TicTacToe.UI.Popups
{
    /// <summary>
    /// Popup отображения результата матча
    /// </summary>
    public class ResultPopup : BaseScreen
    {
        private const float ICON_BOUNCE_DURATION = 0.5f;
        private const float BUTTON_APPEAR_DELAY = 0.3f;
        
        [Header("Result Display")]
        [SerializeField] private Image _resultIconImage;
        [SerializeField] private TextMeshProUGUI _resultTitleText;
        [SerializeField] private TextMeshProUGUI _resultSubtitleText;
        
        [Header("Icons")]
        [SerializeField] private Sprite _victoryIcon;
        [SerializeField] private Sprite _defeatIcon;
        [SerializeField] private Sprite _drawIcon;
        
        [Header("Colors")]
        [SerializeField] private Color _victoryColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color _defeatColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _drawColor = new Color(0.6f, 0.6f, 0.8f);
        
        [Header("Board Preview (Optional)")]
        [SerializeField] private GameObject _boardPreviewContainer;
        [SerializeField] private Image[] _previewCells;
        [SerializeField] private Sprite _previewSpriteX;
        [SerializeField] private Sprite _previewSpriteO;
        [SerializeField] private Sprite _previewSpriteEmpty;
        
        [Header("Buttons")]
        [SerializeField] private Button _rematchButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TextMeshProUGUI _rematchButtonText;
        [SerializeField] private TextMeshProUGUI _mainMenuButtonText;
        
        [Header("Animation")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private CanvasGroup _buttonsCanvasGroup;
        [SerializeField] private float _appearAnimationDuration = 0.4f;
        
        [Header("Background")]
        [SerializeField] private Image _backgroundOverlay;
        [SerializeField] private Button _backgroundButton;
        
        private GameResult _currentResult;
        private Coroutine _animationCoroutine;
        
        /// <summary>
        /// Событие нажатия "Реванш"
        /// </summary>
        public event Action OnRematchClicked;
        
        /// <summary>
        /// Событие нажатия "Главное меню"
        /// </summary>
        public event Action OnMainMenuClicked;
        
        /// <summary>
        /// Текущий отображаемый результат
        /// </summary>
        public GameResult CurrentResult => _currentResult;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Подписываемся на кнопки
            if (_rematchButton != null)
            {
                _rematchButton.onClick.AddListener(HandleRematchClick);
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(HandleMainMenuClick);
            }
            
            // Фоновая кнопка для закрытия (опционально)
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.AddListener(HandleBackgroundClick);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_rematchButton != null)
            {
                _rematchButton.onClick.RemoveListener(HandleRematchClick);
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(HandleMainMenuClick);
            }
            
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.RemoveListener(HandleBackgroundClick);
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
            
            // Получаем результат из GameManager
            if (GameManager.Instance != null)
            {
                GameResult result = GameManager.Instance.Board?.GetGameResult() ?? GameResult.None;
                Setup(result);
            }
            
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
        /// Настроить popup для отображения результата
        /// </summary>
        /// <param name="result">Результат игры</param>
        public void Setup(GameResult result)
        {
            _currentResult = result;
            
            UpdateResultDisplay(result);
            UpdateBoardPreview();
            UpdateButtonTexts();
        }
        
        /// <summary>
        /// Настроить popup с детальной информацией
        /// </summary>
        /// <param name="result">Результат</param>
        /// <param name="isPlayerWin">Победил ли игрок (для VsAI режима)</param>
        /// <param name="mode">Режим игры</param>
        public void Setup(GameResult result, bool isPlayerWin, GameMode mode)
        {
            _currentResult = result;
            
            UpdateResultDisplayDetailed(result, isPlayerWin, mode);
            UpdateBoardPreview();
            UpdateButtonTexts();
        }
        
        private void UpdateResultDisplay(GameResult result)
        {
            // Определяем, победил ли игрок (для VsAI: X = игрок)
            GameMode mode = GameManager.Instance?.CurrentMode ?? GameMode.VsAI;
            bool isPlayerWin = false;
            
            if (mode == GameMode.VsAI)
            {
                isPlayerWin = result == GameResult.XWins;
            }
            
            UpdateResultDisplayDetailed(result, isPlayerWin, mode);
        }
        
        private void UpdateResultDisplayDetailed(GameResult result, bool isPlayerWin, GameMode mode)
        {
            switch (result)
            {
                case GameResult.XWins:
                case GameResult.OWins:
                    if (mode == GameMode.VsAI)
                    {
                        // VsAI режим
                        if (isPlayerWin)
                        {
                            SetVictoryState();
                        }
                        else
                        {
                            SetDefeatState();
                        }
                    }
                    else if (mode == GameMode.LocalMultiplayer)
                    {
                        // Локальный мультиплеер
                        SetLocalMultiplayerWinState(result);
                    }
                    else
                    {
                        // Сетевой мультиплеер
                        SetNetworkWinState(result, isPlayerWin);
                    }
                    break;
                    
                case GameResult.Draw:
                    SetDrawState();
                    break;
                    
                default:
                    SetDrawState();
                    break;
            }
        }
        
        private void SetVictoryState()
        {
            if (_resultIconImage != null && _victoryIcon != null)
            {
                _resultIconImage.sprite = _victoryIcon;
                _resultIconImage.color = _victoryColor;
            }
            
            if (_resultTitleText != null)
            {
                _resultTitleText.text = "ВЫ ПОБЕДИЛИ!";
                _resultTitleText.color = _victoryColor;
            }
            
            if (_resultSubtitleText != null)
            {
                _resultSubtitleText.text = "Отличная игра!";
                _resultSubtitleText.gameObject.SetActive(true);
            }
        }
        
        private void SetDefeatState()
        {
            if (_resultIconImage != null && _defeatIcon != null)
            {
                _resultIconImage.sprite = _defeatIcon;
                _resultIconImage.color = _defeatColor;
            }
            
            if (_resultTitleText != null)
            {
                _resultTitleText.text = "ВЫ ПРОИГРАЛИ";
                _resultTitleText.color = _defeatColor;
            }
            
            if (_resultSubtitleText != null)
            {
                _resultSubtitleText.text = "Попробуйте ещё раз!";
                _resultSubtitleText.gameObject.SetActive(true);
            }
        }
        
        private void SetDrawState()
        {
            if (_resultIconImage != null && _drawIcon != null)
            {
                _resultIconImage.sprite = _drawIcon;
                _resultIconImage.color = _drawColor;
            }
            
            if (_resultTitleText != null)
            {
                _resultTitleText.text = "НИЧЬЯ!";
                _resultTitleText.color = _drawColor;
            }
            
            if (_resultSubtitleText != null)
            {
                _resultSubtitleText.text = "Равная игра";
                _resultSubtitleText.gameObject.SetActive(true);
            }
        }
        
        private void SetLocalMultiplayerWinState(GameResult result)
        {
            string winner = result == GameResult.XWins ? "ИГРОК 1" : "ИГРОК 2";
            Color winColor = result == GameResult.XWins ? 
                new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
            
            if (_resultIconImage != null && _victoryIcon != null)
            {
                _resultIconImage.sprite = _victoryIcon;
                _resultIconImage.color = _victoryColor;
            }
            
            if (_resultTitleText != null)
            {
                _resultTitleText.text = $"{winner} ПОБЕДИЛ!";
                _resultTitleText.color = winColor;
            }
            
            if (_resultSubtitleText != null)
            {
                _resultSubtitleText.text = result == GameResult.XWins ? "X выиграл" : "O выиграл";
                _resultSubtitleText.gameObject.SetActive(true);
            }
        }
        
        private void SetNetworkWinState(GameResult result, bool isPlayerWin)
        {
            if (isPlayerWin)
            {
                SetVictoryState();
                if (_resultSubtitleText != null)
                {
                    _resultSubtitleText.text = "Вы победили в сетевой игре!";
                }
            }
            else
            {
                SetDefeatState();
                if (_resultSubtitleText != null)
                {
                    _resultSubtitleText.text = "Противник победил";
                }
            }
        }
        
        private void UpdateBoardPreview()
        {
            if (_boardPreviewContainer == null || _previewCells == null)
            {
                return;
            }
            
            var board = GameManager.Instance?.Board;
            if (board == null)
            {
                _boardPreviewContainer.SetActive(false);
                return;
            }
            
            _boardPreviewContainer.SetActive(true);
            
            for (int i = 0; i < _previewCells.Length && i < 9; i++)
            {
                if (_previewCells[i] == null)
                {
                    continue;
                }
                
                CellState state = board.GetCell(i);
                
                switch (state)
                {
                    case CellState.X:
                        _previewCells[i].sprite = _previewSpriteX;
                        _previewCells[i].color = Color.white;
                        break;
                    case CellState.O:
                        _previewCells[i].sprite = _previewSpriteO;
                        _previewCells[i].color = Color.white;
                        break;
                    default:
                        _previewCells[i].sprite = _previewSpriteEmpty;
                        _previewCells[i].color = new Color(1f, 1f, 1f, 0.2f);
                        break;
                }
            }
        }
        
        private void UpdateButtonTexts()
        {
            if (_rematchButtonText != null)
            {
                _rematchButtonText.text = "РЕВАНШ";
            }
            
            if (_mainMenuButtonText != null)
            {
                _mainMenuButtonText.text = "ГЛАВНОЕ МЕНЮ";
            }
        }
        
        private void HandleRematchClick()
        {
            OnRematchClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.RESULT);
            
            // Перезапускаем игру
            GameManager.Instance?.Restart();
        }
        
        private void HandleMainMenuClick()
        {
            OnMainMenuClicked?.Invoke();
            
            // Скрываем popup
            UIManager.Instance?.HidePopup(Constants.Popups.RESULT);
            
            // Возвращаемся в главное меню
            GameManager.Instance?.QuitToMenu();
            UIManager.Instance?.ShowScreen(Constants.Screens.MAIN_MENU);
        }
        
        private void HandleBackgroundClick()
        {
            // Закрытие по клику на фон отключено - нужно выбрать действие
            // Можно включить, если нужно
        }
        
        private IEnumerator AnimateAppear()
        {
            // Начальное состояние
            if (_contentContainer != null)
            {
                _contentContainer.localScale = Vector3.zero;
            }
            
            if (_buttonsCanvasGroup != null)
            {
                _buttonsCanvasGroup.alpha = 0f;
            }
            
            if (_backgroundOverlay != null)
            {
                Color bgColor = _backgroundOverlay.color;
                bgColor.a = 0f;
                _backgroundOverlay.color = bgColor;
            }
            
            // Анимация фона
            float elapsed = 0f;
            float bgDuration = _appearAnimationDuration * 0.5f;
            
            while (elapsed < bgDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bgDuration;
                
                if (_backgroundOverlay != null)
                {
                    Color bgColor = _backgroundOverlay.color;
                    bgColor.a = Mathf.Lerp(0f, 0.7f, t);
                    _backgroundOverlay.color = bgColor;
                }
                
                yield return null;
            }
            
            // Анимация контента (bounce)
            elapsed = 0f;
            
            while (elapsed < _appearAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _appearAnimationDuration;
                
                if (_contentContainer != null)
                {
                    float scale = EaseOutBack(t);
                    _contentContainer.localScale = Vector3.one * scale;
                }
                
                yield return null;
            }
            
            if (_contentContainer != null)
            {
                _contentContainer.localScale = Vector3.one;
            }
            
            // Анимация иконки (дополнительный bounce)
            if (_resultIconImage != null)
            {
                StartCoroutine(AnimateIconBounce());
            }
            
            // Задержка перед показом кнопок
            yield return new WaitForSeconds(BUTTON_APPEAR_DELAY);
            
            // Анимация кнопок
            elapsed = 0f;
            float buttonDuration = 0.3f;
            
            while (elapsed < buttonDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / buttonDuration;
                
                if (_buttonsCanvasGroup != null)
                {
                    _buttonsCanvasGroup.alpha = t;
                }
                
                yield return null;
            }
            
            if (_buttonsCanvasGroup != null)
            {
                _buttonsCanvasGroup.alpha = 1f;
            }
            
            _animationCoroutine = null;
        }
        
        private IEnumerator AnimateIconBounce()
        {
            if (_resultIconImage == null)
            {
                yield break;
            }
            
            RectTransform iconRect = _resultIconImage.rectTransform;
            Vector3 originalScale = iconRect.localScale;
            
            float elapsed = 0f;
            
            // Увеличение
            while (elapsed < ICON_BOUNCE_DURATION * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (ICON_BOUNCE_DURATION * 0.5f);
                float scale = Mathf.Lerp(1f, 1.2f, EaseOutQuad(t));
                iconRect.localScale = originalScale * scale;
                yield return null;
            }
            
            elapsed = 0f;
            
            // Уменьшение
            while (elapsed < ICON_BOUNCE_DURATION * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (ICON_BOUNCE_DURATION * 0.5f);
                float scale = Mathf.Lerp(1.2f, 1f, EaseOutQuad(t));
                iconRect.localScale = originalScale * scale;
                yield return null;
            }
            
            iconRect.localScale = originalScale;
        }
        
        /// <summary>
        /// Ease Out Back функция
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        /// <summary>
        /// Ease Out Quad функция
        /// </summary>
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Проверяем массив превью ячеек
            if (_previewCells == null || _previewCells.Length != 9)
            {
                System.Array.Resize(ref _previewCells, 9);
            }
        }
#endif
    }
}
