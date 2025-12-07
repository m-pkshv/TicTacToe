// Assets/_Project/Scripts/Core/GameManager.cs

using System;
using System.Collections;
using UnityEngine;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;
using TicTacToe.Game.Presenters;
using TicTacToe.Utils;

namespace TicTacToe.Core
{
    /// <summary>
    /// Центральный контроллер игры (Singleton).
    /// Управляет состояниями игры, режимами и координирует все подсистемы.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// Состояния игрового автомата (State Machine).
        /// </summary>
        public enum State
        {
            /// <summary>Начальное состояние до инициализации.</summary>
            None,
            /// <summary>Главное меню.</summary>
            MainMenu,
            /// <summary>Выбор сложности AI.</summary>
            DifficultySelect,
            /// <summary>Лобби сетевой игры.</summary>
            Lobby,
            /// <summary>Ожидание подключения игрока.</summary>
            WaitingForPlayer,
            /// <summary>Игра в процессе.</summary>
            Playing,
            /// <summary>Игра завершена.</summary>
            GameOver,
            /// <summary>Игра на паузе.</summary>
            Paused
        }
        
        private const float AI_MOVE_DELAY = 0.5f;
        
        private GamePresenter _presenter;
        private IGameView _currentView;
        private State _previousState;
        private Coroutine _aiMoveCoroutine;
        
        // TODO: Заменить на реальный IAIPlayer после создания AI системы
        private object _aiPlayer;
        
        /// <summary>
        /// Вызывается при изменении состояния игры.
        /// </summary>
        public event Action<State> OnStateChanged;
        
        /// <summary>
        /// Вызывается при завершении игры.
        /// </summary>
        public event Action<GameResult> OnGameEnded;
        
        /// <summary>
        /// Вызывается при смене хода.
        /// </summary>
        public event Action<CellState> OnTurnChanged;
        
        /// <summary>
        /// Вызывается при выполнении хода.
        /// Параметры: индекс ячейки, игрок.
        /// </summary>
        public event Action<int, CellState> OnMoveMade;
        
        /// <summary>
        /// Текущее состояние игры.
        /// </summary>
        public State CurrentState { get; private set; }
        
        /// <summary>
        /// Текущий режим игры.
        /// </summary>
        public GameMode CurrentMode { get; private set; }
        
        /// <summary>
        /// Текущая сложность AI (актуально для режима VsAI).
        /// </summary>
        public AIDifficulty CurrentDifficulty { get; private set; }
        
        /// <summary>
        /// Текущий игрок (чей ход).
        /// </summary>
        public CellState CurrentTurn => _presenter?.CurrentPlayer ?? CellState.X;
        
        /// <summary>
        /// Модель игрового поля.
        /// </summary>
        public BoardModel Board => _presenter?.Board;
        
        /// <summary>
        /// Презентер игры.
        /// </summary>
        public GamePresenter Presenter => _presenter;
        
        /// <summary>
        /// Активна ли игра.
        /// </summary>
        public bool IsGameActive => CurrentState == State.Playing;
        
        /// <summary>
        /// Символ игрока (в режиме VsAI).
        /// </summary>
        public CellState PlayerSymbol { get; private set; } = CellState.X;
        
        /// <summary>
        /// Символ AI (в режиме VsAI).
        /// </summary>
        public CellState AISymbol => PlayerSymbol == CellState.X ? CellState.O : CellState.X;
        
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        private void Initialize()
        {
            _presenter = new GamePresenter();
            _presenter.OnTurnChanged += HandleTurnChanged;
            _presenter.OnGameEnded += HandleGameEnded;
            _presenter.OnMoveMade += HandleMoveMade;
            
            ChangeState(State.MainMenu);
        }
        
        private void Cleanup()
        {
            if (_aiMoveCoroutine != null)
            {
                StopCoroutine(_aiMoveCoroutine);
                _aiMoveCoroutine = null;
            }
            
            if (_presenter != null)
            {
                _presenter.OnTurnChanged -= HandleTurnChanged;
                _presenter.OnGameEnded -= HandleGameEnded;
                _presenter.OnMoveMade -= HandleMoveMade;
                _presenter.Dispose();
                _presenter = null;
            }
        }
        
        /// <summary>
        /// Привязать View к игре.
        /// </summary>
        /// <param name="view">Реализация IGameView.</param>
        public void SetView(IGameView view)
        {
            _currentView = view;
            _presenter?.SetView(view);
        }
        
        /// <summary>
        /// Начать игру против AI.
        /// </summary>
        /// <param name="difficulty">Уровень сложности.</param>
        /// <param name="playerSymbol">Символ игрока (X ходит первым).</param>
        public void StartGameVsAI(AIDifficulty difficulty, CellState playerSymbol = CellState.X)
        {
            CurrentMode = GameMode.VsAI;
            CurrentDifficulty = difficulty;
            PlayerSymbol = playerSymbol;
            
            // TODO: Создать AI через AIFactory
            // _aiPlayer = AIFactory.Create(difficulty);
            
            StartGameInternal();
            
            // Если AI ходит первым
            if (PlayerSymbol == CellState.O)
            {
                _presenter.LockInput();
                ScheduleAIMove();
            }
        }
        
        /// <summary>
        /// Начать локальную игру (два игрока на одном устройстве).
        /// </summary>
        public void StartLocalMultiplayer()
        {
            CurrentMode = GameMode.LocalMultiplayer;
            CurrentDifficulty = AIDifficulty.Easy; // Не используется
            PlayerSymbol = CellState.X;
            _aiPlayer = null;
            
            StartGameInternal();
        }
        
        /// <summary>
        /// Начать сетевую игру.
        /// </summary>
        public void StartNetworkMultiplayer()
        {
            CurrentMode = GameMode.NetworkMultiplayer;
            ChangeState(State.Lobby);
            
            // TODO: Инициализация сетевого менеджера
        }
        
        /// <summary>
        /// Начать игру (общий метод из TDD).
        /// </summary>
        /// <param name="mode">Режим игры.</param>
        /// <param name="difficulty">Сложность AI (для режима VsAI).</param>
        public void StartGame(GameMode mode, AIDifficulty difficulty = AIDifficulty.Easy)
        {
            switch (mode)
            {
                case GameMode.VsAI:
                    StartGameVsAI(difficulty);
                    break;
                case GameMode.LocalMultiplayer:
                    StartLocalMultiplayer();
                    break;
                case GameMode.NetworkMultiplayer:
                    StartNetworkMultiplayer();
                    break;
            }
        }
        
        /// <summary>
        /// Выполнить ход игрока.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки (0-8).</param>
        public void MakeMove(int cellIndex)
        {
            if (CurrentState != State.Playing)
            {
                return;
            }
            
            // В режиме VsAI проверяем, что сейчас ход игрока
            if (CurrentMode == GameMode.VsAI && CurrentTurn != PlayerSymbol)
            {
                return;
            }
            
            bool success = _presenter.TryMakeMove(cellIndex);
            
            // После успешного хода игрока в режиме VsAI — ход AI
            if (success && CurrentMode == GameMode.VsAI && CurrentState == State.Playing)
            {
                _presenter.LockInput();
                ScheduleAIMove();
            }
        }
        
        /// <summary>
        /// Перезапустить игру с теми же настройками.
        /// </summary>
        public void Restart()
        {
            if (_aiMoveCoroutine != null)
            {
                StopCoroutine(_aiMoveCoroutine);
                _aiMoveCoroutine = null;
            }
            
            switch (CurrentMode)
            {
                case GameMode.VsAI:
                    StartGameVsAI(CurrentDifficulty, PlayerSymbol);
                    break;
                case GameMode.LocalMultiplayer:
                    StartLocalMultiplayer();
                    break;
                case GameMode.NetworkMultiplayer:
                    // TODO: Перезапуск сетевой игры
                    StartGameInternal();
                    break;
            }
        }
        
        /// <summary>
        /// Вернуться в главное меню.
        /// </summary>
        public void QuitToMenu()
        {
            if (_aiMoveCoroutine != null)
            {
                StopCoroutine(_aiMoveCoroutine);
                _aiMoveCoroutine = null;
            }
            
            // TODO: Отключиться от сети если нужно
            
            ChangeState(State.MainMenu);
        }
        
        /// <summary>
        /// Поставить игру на паузу.
        /// </summary>
        public void Pause()
        {
            if (CurrentState != State.Playing)
            {
                return;
            }
            
            _previousState = CurrentState;
            ChangeState(State.Paused);
            _presenter?.LockInput();
        }
        
        /// <summary>
        /// Продолжить игру после паузы.
        /// </summary>
        public void Resume()
        {
            if (CurrentState != State.Paused)
            {
                return;
            }
            
            ChangeState(_previousState);
            
            // Разблокировать ввод только если сейчас ход игрока
            if (CurrentMode != GameMode.VsAI || CurrentTurn == PlayerSymbol)
            {
                _presenter?.UnlockInput();
            }
        }
        
        /// <summary>
        /// Перейти к выбору сложности.
        /// </summary>
        public void GoToDifficultySelect()
        {
            ChangeState(State.DifficultySelect);
        }
        
        /// <summary>
        /// Перейти в лобби.
        /// </summary>
        public void GoToLobby()
        {
            ChangeState(State.Lobby);
        }
        
        private void StartGameInternal()
        {
            _presenter.StartGame(CellState.X);
            ChangeState(State.Playing);
        }
        
        private void ChangeState(State newState)
        {
            if (CurrentState == newState)
            {
                return;
            }
            
            State oldState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"[GameManager] State changed: {oldState} -> {newState}");
            
            OnStateChanged?.Invoke(newState);
        }
        
        private void HandleTurnChanged(CellState newTurn)
        {
            OnTurnChanged?.Invoke(newTurn);
        }
        
        private void HandleGameEnded(GameResult result)
        {
            ChangeState(State.GameOver);
            OnGameEnded?.Invoke(result);
            
            // TODO: Показать interstitial рекламу (каждые N игр)
            // TODO: Обновить статистику
        }
        
        private void HandleMoveMade(int cellIndex, CellState player)
        {
            OnMoveMade?.Invoke(cellIndex, player);
        }
        
        private void ScheduleAIMove()
        {
            if (_aiMoveCoroutine != null)
            {
                StopCoroutine(_aiMoveCoroutine);
            }
            
            _aiMoveCoroutine = StartCoroutine(AITurnCoroutine());
        }
        
        private IEnumerator AITurnCoroutine()
        {
            _currentView?.ShowAIThinking(true);
            
            // Небольшая задержка для естественности
            yield return new WaitForSeconds(AI_MOVE_DELAY);
            
            if (CurrentState != State.Playing)
            {
                _currentView?.ShowAIThinking(false);
                yield break;
            }
            
            // TODO: Получить ход от AI
            // int aiMove = _aiPlayer.GetMove(Board, AISymbol);
            
            // Временная заглушка: случайный ход
            int aiMove = GetRandomEmptyCell();
            
            if (aiMove >= 0)
            {
                _presenter.MakeMove(aiMove, AISymbol);
            }
            
            _currentView?.ShowAIThinking(false);
            
            // Разблокировать ввод, если игра продолжается
            if (CurrentState == State.Playing)
            {
                _presenter.UnlockInput();
            }
            
            _aiMoveCoroutine = null;
        }
        
        /// <summary>
        /// Временный метод: получить случайную пустую ячейку.
        /// Будет заменён на AI.GetMove() после создания AI системы.
        /// </summary>
        private int GetRandomEmptyCell()
        {
            if (Board == null)
            {
                return -1;
            }
            
            var emptyCells = Board.GetEmptyCells();
            
            if (emptyCells.Count == 0)
            {
                return -1;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, emptyCells.Count);
            return emptyCells[randomIndex];
        }
    }
}
