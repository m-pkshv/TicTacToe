// Assets/_Project/Scripts/UI/Screens/GameScreen.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Core;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;
using TicTacToe.UI.Components;
using TicTacToe.Utils;

namespace TicTacToe.UI.Screens
{
    /// <summary>
    /// Экран игрового процесса
    /// </summary>
    public class GameScreen : BaseScreen
    {
        private const float AI_THINKING_INDICATOR_DELAY = 0.3f;
        
        [Header("Header")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _settingsButton;
        
        [Header("Turn Indicator")]
        [SerializeField] private RectTransform _turnIndicatorContainer;
        [SerializeField] private Image _playerXIndicator;
        [SerializeField] private Image _playerOIndicator;
        [SerializeField] private TextMeshProUGUI _playerXLabel;
        [SerializeField] private TextMeshProUGUI _playerOLabel;
        [SerializeField] private TextMeshProUGUI _turnStatusText;
        
        [Header("Board")]
        [SerializeField] private BoardView _boardView;
        
        [Header("Footer")]
        [SerializeField] private TextMeshProUGUI _gameModeText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        
        [Header("AI Thinking Indicator")]
        [SerializeField] private GameObject _aiThinkingContainer;
        [SerializeField] private TextMeshProUGUI _aiThinkingText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _activeIndicatorColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _inactiveIndicatorColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color _colorX = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _colorO = new Color(1f, 0.4f, 0.4f);
        
        private GameMode _currentMode;
        private AIDifficulty _currentDifficulty;
        private bool _isAIThinking;
        private Coroutine _aiThinkingCoroutine;
        
        /// <summary>
        /// Событие нажатия паузы
        /// </summary>
        public event Action OnPauseClicked;
        
        /// <summary>
        /// Событие нажатия настроек
        /// </summary>
        public event Action OnSettingsClicked;
        
        /// <summary>
        /// Компонент игрового поля
        /// </summary>
        public BoardView BoardView => _boardView;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Подписываемся на кнопки
            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(HandlePauseClick);
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(HandleSettingsClick);
            }
            
            // Подписываемся на события BoardView
            if (_boardView != null)
            {
                _boardView.OnCellClicked += HandleCellClicked;
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Отписываемся от кнопок
            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveListener(HandlePauseClick);
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(HandleSettingsClick);
            }
            
            // Отписываемся от BoardView
            if (_boardView != null)
            {
                _boardView.OnCellClicked -= HandleCellClicked;
            }
            
            // Отписываемся от GameManager
            UnsubscribeFromGameManager();
            
            if (_aiThinkingCoroutine != null)
            {
                StopCoroutine(_aiThinkingCoroutine);
            }
        }
        
        /// <summary>
        /// Вызывается при показе экрана
        /// </summary>
        public override void OnScreenShow()
        {
            base.OnScreenShow();
            
            // Подписываемся на события GameManager
            SubscribeToGameManager();
            
            // Инициализируем UI из текущего состояния GameManager
            InitializeFromGameManager();
            
            // Скрываем индикатор AI
            HideAIThinking();
        }
        
        /// <summary>
        /// Вызывается при скрытии экрана
        /// </summary>
        public override void OnScreenHide()
        {
            base.OnScreenHide();
            
            UnsubscribeFromGameManager();
            
            if (_aiThinkingCoroutine != null)
            {
                StopCoroutine(_aiThinkingCoroutine);
                _aiThinkingCoroutine = null;
            }
        }
        
        /// <summary>
        /// Настроить экран для режима игры
        /// </summary>
        /// <param name="mode">Режим игры</param>
        /// <param name="difficulty">Сложность AI (для режима VsAI)</param>
        public void Setup(GameMode mode, AIDifficulty difficulty = AIDifficulty.Easy)
        {
            _currentMode = mode;
            _currentDifficulty = difficulty;
            
            UpdateGameModeText();
            UpdateDifficultyText();
            UpdatePlayerLabels();
        }
        
        /// <summary>
        /// Обновить индикатор текущего хода
        /// </summary>
        /// <param name="currentTurn">Чей сейчас ход</param>
        public void UpdateTurnIndicator(CellState currentTurn)
        {
            bool isXTurn = currentTurn == CellState.X;
            
            // Обновляем визуальные индикаторы
            if (_playerXIndicator != null)
            {
                _playerXIndicator.color = isXTurn ? _activeIndicatorColor : _inactiveIndicatorColor;
            }
            
            if (_playerOIndicator != null)
            {
                _playerOIndicator.color = !isXTurn ? _activeIndicatorColor : _inactiveIndicatorColor;
            }
            
            // Обновляем текст статуса
            UpdateTurnStatusText(currentTurn);
            
            // Передаём в BoardView
            _boardView?.ShowCurrentTurn(currentTurn);
        }
        
        /// <summary>
        /// Показать индикатор "ИИ думает"
        /// </summary>
        public void ShowAIThinking()
        {
            if (_isAIThinking)
            {
                return;
            }
            
            _isAIThinking = true;
            
            // Показываем с небольшой задержкой, чтобы не мелькало при быстрых ходах
            if (_aiThinkingCoroutine != null)
            {
                StopCoroutine(_aiThinkingCoroutine);
            }
            
            _aiThinkingCoroutine = StartCoroutine(ShowAIThinkingDelayed());
        }
        
        /// <summary>
        /// Скрыть индикатор "ИИ думает"
        /// </summary>
        public void HideAIThinking()
        {
            _isAIThinking = false;
            
            if (_aiThinkingCoroutine != null)
            {
                StopCoroutine(_aiThinkingCoroutine);
                _aiThinkingCoroutine = null;
            }
            
            if (_aiThinkingContainer != null)
            {
                _aiThinkingContainer.SetActive(false);
            }
        }
        
        /// <summary>
        /// Показать результат игры
        /// </summary>
        /// <param name="result">Результат</param>
        public void ShowGameResult(GameResult result)
        {
            HideAIThinking();
            
            // Обновляем текст статуса
            string statusText = result switch
            {
                GameResult.XWins => GetWinText(CellState.X),
                GameResult.OWins => GetWinText(CellState.O),
                GameResult.Draw => "НИЧЬЯ!",
                _ => ""
            };
            
            if (_turnStatusText != null)
            {
                _turnStatusText.text = statusText;
            }
            
            // BoardView покажет подсветку выигрышной линии
            _boardView?.ShowGameResult(result);
        }
        
        /// <summary>
        /// Подсветить выигрышную линию
        /// </summary>
        /// <param name="winningIndices">Индексы выигрышных ячеек</param>
        public void HighlightWinningLine(int[] winningIndices)
        {
            _boardView?.HighlightWinningLine(winningIndices);
        }
        
        /// <summary>
        /// Сбросить экран для новой игры
        /// </summary>
        public void ResetGame()
        {
            _boardView?.ResetBoard();
            HideAIThinking();
            UpdateTurnIndicator(CellState.X);
        }
        
        /// <summary>
        /// Заблокировать/разблокировать ввод
        /// </summary>
        /// <param name="locked">Заблокировать</param>
        public void SetInputLocked(bool locked)
        {
            _boardView?.SetInputLocked(locked);
        }
        
        private void SubscribeToGameManager()
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            
            GameManager.Instance.OnTurnChanged += HandleTurnChanged;
            GameManager.Instance.OnGameEnded += HandleGameEnded;
            GameManager.Instance.OnMoveMade += HandleMoveMade;
        }
        
        private void UnsubscribeFromGameManager()
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            
            GameManager.Instance.OnTurnChanged -= HandleTurnChanged;
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
            GameManager.Instance.OnMoveMade -= HandleMoveMade;
        }
        
        private void InitializeFromGameManager()
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            
            _currentMode = GameManager.Instance.CurrentMode;
            _currentDifficulty = GameManager.Instance.CurrentDifficulty;
            
            Setup(_currentMode, _currentDifficulty);
            
            // Синхронизируем состояние поля
            BoardModel board = GameManager.Instance.Board;
            if (board != null)
            {
                for (int i = 0; i < BoardModel.TOTAL_CELLS; i++)
                {
                    _boardView?.UpdateCell(i, board.GetCell(i));
                }
            }
            
            // Обновляем индикатор хода
            UpdateTurnIndicator(GameManager.Instance.CurrentTurn);
        }
        
        private void HandleCellClicked(int cellIndex)
        {
            // Передаём ход в GameManager
            GameManager.Instance?.MakeMove(cellIndex);
        }
        
        private void HandlePauseClick()
        {
            OnPauseClicked?.Invoke();
            
            // Показываем popup паузы
            GameManager.Instance?.Pause();
            UIManager.Instance?.ShowPopup(Constants.Popups.PAUSE);
        }
        
        private void HandleSettingsClick()
        {
            OnSettingsClicked?.Invoke();
            
            // TODO: Показать быстрые настройки (звук)
        }
        
        private void HandleTurnChanged(CellState newTurn)
        {
            UpdateTurnIndicator(newTurn);
            
            // Если ход AI, показываем индикатор
            if (_currentMode == GameMode.VsAI && newTurn == CellState.O)
            {
                ShowAIThinking();
                _boardView?.ShowAIThinking(true);
            }
            else
            {
                HideAIThinking();
                _boardView?.ShowAIThinking(false);
            }
        }
        
        private void HandleGameEnded(GameResult result)
        {
            HideAIThinking();
            ShowGameResult(result);
            
            // Получаем выигрышную линию
            int[] winningLine = GameManager.Instance?.Board?.GetWinningLine();
            if (winningLine != null && winningLine.Length > 0)
            {
                HighlightWinningLine(winningLine);
            }
            
            // Показываем popup результата с задержкой
            StartCoroutine(ShowResultPopupDelayed(result));
        }
        
        private void HandleMoveMade(int cellIndex, CellState symbol)
        {
            _boardView?.UpdateCell(cellIndex, symbol);
            
            // Скрываем индикатор AI после хода
            if (symbol == CellState.O && _currentMode == GameMode.VsAI)
            {
                HideAIThinking();
            }
        }
        
        private void UpdateGameModeText()
        {
            if (_gameModeText == null)
            {
                return;
            }
            
            string modeText = _currentMode switch
            {
                GameMode.VsAI => "VS AI",
                GameMode.LocalMultiplayer => "LOCAL",
                GameMode.NetworkMultiplayer => "NETWORK",
                _ => ""
            };
            
            _gameModeText.text = modeText;
        }
        
        private void UpdateDifficultyText()
        {
            if (_difficultyText == null)
            {
                return;
            }
            
            if (_currentMode == GameMode.VsAI)
            {
                string diffText = _currentDifficulty switch
                {
                    AIDifficulty.Easy => "EASY",
                    AIDifficulty.Medium => "MEDIUM",
                    AIDifficulty.Hard => "HARD",
                    _ => ""
                };
                
                _difficultyText.text = diffText;
                _difficultyText.gameObject.SetActive(true);
            }
            else
            {
                _difficultyText.gameObject.SetActive(false);
            }
        }
        
        private void UpdatePlayerLabels()
        {
            if (_currentMode == GameMode.VsAI)
            {
                if (_playerXLabel != null)
                {
                    _playerXLabel.text = "ВЫ";
                }
                
                if (_playerOLabel != null)
                {
                    _playerOLabel.text = "AI";
                }
            }
            else if (_currentMode == GameMode.LocalMultiplayer)
            {
                if (_playerXLabel != null)
                {
                    _playerXLabel.text = "P1";
                }
                
                if (_playerOLabel != null)
                {
                    _playerOLabel.text = "P2";
                }
            }
            else // NetworkMultiplayer
            {
                if (_playerXLabel != null)
                {
                    _playerXLabel.text = "HOST";
                }
                
                if (_playerOLabel != null)
                {
                    _playerOLabel.text = "CLIENT";
                }
            }
        }
        
        private void UpdateTurnStatusText(CellState currentTurn)
        {
            if (_turnStatusText == null)
            {
                return;
            }
            
            string statusText;
            
            if (_currentMode == GameMode.VsAI)
            {
                statusText = currentTurn == CellState.X ? "ВАШ ХОД" : "ХОД AI";
            }
            else if (_currentMode == GameMode.LocalMultiplayer)
            {
                statusText = currentTurn == CellState.X ? "ХОД P1" : "ХОД P2";
            }
            else
            {
                statusText = currentTurn == CellState.X ? "ХОД HOST" : "ХОД CLIENT";
            }
            
            _turnStatusText.text = statusText;
            _turnStatusText.color = currentTurn == CellState.X ? _colorX : _colorO;
        }
        
        private string GetWinText(CellState winner)
        {
            if (_currentMode == GameMode.VsAI)
            {
                return winner == CellState.X ? "ВЫ ПОБЕДИЛИ!" : "ВЫ ПРОИГРАЛИ";
            }
            else if (_currentMode == GameMode.LocalMultiplayer)
            {
                return winner == CellState.X ? "P1 ПОБЕДИЛ!" : "P2 ПОБЕДИЛ!";
            }
            else
            {
                return winner == CellState.X ? "HOST ПОБЕДИЛ!" : "CLIENT ПОБЕДИЛ!";
            }
        }
        
        private IEnumerator ShowAIThinkingDelayed()
        {
            yield return new WaitForSeconds(AI_THINKING_INDICATOR_DELAY);
            
            if (_isAIThinking && _aiThinkingContainer != null)
            {
                _aiThinkingContainer.SetActive(true);
                
                if (_aiThinkingText != null)
                {
                    _aiThinkingText.text = "AI думает...";
                }
            }
            
            _aiThinkingCoroutine = null;
        }
        
        private IEnumerator ShowResultPopupDelayed(GameResult result)
        {
            // Даём время на анимацию выигрышной линии
            yield return new WaitForSeconds(0.8f);
            
            // Показываем popup результата
            UIManager.Instance?.ShowPopup(Constants.Popups.RESULT);
            
            // TODO: Передать результат в popup через UIManager или событие
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_boardView == null)
            {
                _boardView = GetComponentInChildren<BoardView>();
            }
        }
#endif
    }
}
