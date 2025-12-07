// Assets/_Project/Scripts/Core/GameManager.cs

using System;
using System.Collections;
using UnityEngine;
using TicTacToe.AI;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;
using TicTacToe.Game.Presenters;
using TicTacToe.Utils;

namespace TicTacToe.Core
{
    /// <summary>
    /// Главный менеджер игры (Singleton).
    /// Управляет состояниями игры, игровым циклом и координирует все подсистемы.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// Состояния игры (State Machine).
        /// </summary>
        public enum State
        {
            None,
            MainMenu,
            DifficultySelect,
            Lobby,
            WaitingForPlayer,
            Playing,
            GameOver,
            Paused
        }
        
        private const float AI_THINK_DELAY_MIN = 0.3f;
        private const float AI_THINK_DELAY_MAX = 0.8f;
        private const int MATCHES_BEFORE_INTERSTITIAL = 3;
        
        // События
        /// <summary>Вызывается при смене состояния игры.</summary>
        public event Action<State> OnStateChanged;
        
        /// <summary>Вызывается при окончании игры.</summary>
        public event Action<GameResult> OnGameEnded;
        
        /// <summary>Вызывается при смене хода.</summary>
        public event Action<CellState> OnTurnChanged;
        
        /// <summary>Вызывается при совершении хода.</summary>
        public event Action<int, CellState> OnMoveMade;
        
        // Свойства
        /// <summary>Текущее состояние игры.</summary>
        public State CurrentState { get; private set; } = State.None;
        
        /// <summary>Текущий режим игры.</summary>
        public GameMode CurrentMode { get; private set; }
        
        /// <summary>Текущая сложность ИИ.</summary>
        public AIDifficulty CurrentDifficulty { get; private set; }
        
        /// <summary>Чей сейчас ход (X или O).</summary>
        public CellState CurrentTurn { get; private set; } = CellState.X;
        
        /// <summary>Модель игрового поля.</summary>
        public BoardModel Board { get; private set; }
        
        /// <summary>Результат последней игры.</summary>
        public GameResult LastGameResult { get; private set; } = GameResult.None;
        
        /// <summary>Игрок за X (true) или за O (false) в режиме VsAI.</summary>
        public bool PlayerIsX { get; private set; } = true;
        
        /// <summary>Идёт ли ход ИИ.</summary>
        public bool IsAIThinking { get; private set; }
        
        // Приватные поля
        private IAIPlayer _aiPlayer;
        private GamePresenter _presenter;
        private Coroutine _aiThinkCoroutine;
        private int _matchesPlayed;
        private State _stateBeforePause;
        
        protected override void Awake()
        {
            base.Awake();
            Board = new BoardModel();
            ChangeState(State.MainMenu);
        }
        
        private void OnDestroy()
        {
            StopAIThinking();
        }
        
        // ========== Публичные методы ==========
        
        /// <summary>
        /// Устанавливает Presenter для связи с UI.
        /// </summary>
        /// <param name="presenter">GamePresenter</param>
        public void SetPresenter(GamePresenter presenter)
        {
            _presenter = presenter;
        }
        
        /// <summary>
        /// Начинает игру против ИИ.
        /// </summary>
        /// <param name="difficulty">Уровень сложности</param>
        /// <param name="playerIsX">Игрок играет за X (первый ход)</param>
        public void StartGameVsAI(AIDifficulty difficulty, bool playerIsX = true)
        {
            CurrentMode = GameMode.VsAI;
            CurrentDifficulty = difficulty;
            PlayerIsX = playerIsX;
            
            // Создаём ИИ через фабрику
            _aiPlayer = AIFactory.Create(difficulty);
            
            StartGameInternal();
            
            // Если ИИ ходит первым
            if (!playerIsX)
            {
                HandleAITurn();
            }
        }
        
        /// <summary>
        /// Начинает локальный мультиплеер (два игрока на одном устройстве).
        /// </summary>
        public void StartLocalMultiplayer()
        {
            CurrentMode = GameMode.LocalMultiplayer;
            _aiPlayer = null;
            
            StartGameInternal();
        }
        
        /// <summary>
        /// Начинает сетевую игру (заглушка — будет реализовано в Фазе 8).
        /// </summary>
        public void StartNetworkGame()
        {
            CurrentMode = GameMode.NetworkMultiplayer;
            _aiPlayer = null;
            
            // TODO: Фаза 8 — Network Multiplayer
            ChangeState(State.Lobby);
        }
        
        /// <summary>
        /// Переводит игру в состояние выбора сложности.
        /// </summary>
        public void ShowDifficultySelect()
        {
            ChangeState(State.DifficultySelect);
        }
        
        /// <summary>
        /// Совершает ход в указанную ячейку.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки (0-8)</param>
        /// <returns>true если ход успешен</returns>
        public bool MakeMove(int cellIndex)
        {
            if (CurrentState != State.Playing)
            {
                return false;
            }
            
            // Блокируем ход во время "думания" ИИ
            if (IsAIThinking)
            {
                return false;
            }
            
            // В режиме VsAI игрок может ходить только своим символом
            if (CurrentMode == GameMode.VsAI)
            {
                CellState playerSymbol = PlayerIsX ? CellState.X : CellState.O;
                if (CurrentTurn != playerSymbol)
                {
                    return false;
                }
            }
            
            return ProcessMove(cellIndex);
        }
        
        /// <summary>
        /// Перезапускает игру с теми же настройками.
        /// </summary>
        public void Restart()
        {
            StopAIThinking();
            
            switch (CurrentMode)
            {
                case GameMode.VsAI:
                    StartGameVsAI(CurrentDifficulty, PlayerIsX);
                    break;
                case GameMode.LocalMultiplayer:
                    StartLocalMultiplayer();
                    break;
                case GameMode.NetworkMultiplayer:
                    // TODO: Фаза 8
                    StartLocalMultiplayer();
                    break;
            }
        }
        
        /// <summary>
        /// Выходит в главное меню.
        /// </summary>
        public void QuitToMenu()
        {
            StopAIThinking();
            _aiPlayer = null;
            ChangeState(State.MainMenu);
        }
        
        /// <summary>
        /// Ставит игру на паузу.
        /// </summary>
        public void Pause()
        {
            if (CurrentState == State.Playing)
            {
                _stateBeforePause = CurrentState;
                ChangeState(State.Paused);
            }
        }
        
        /// <summary>
        /// Снимает игру с паузы.
        /// </summary>
        public void Resume()
        {
            if (CurrentState == State.Paused)
            {
                ChangeState(_stateBeforePause);
            }
        }
        
        // ========== Приватные методы ==========
        
        /// <summary>
        /// Внутренняя логика начала игры.
        /// </summary>
        private void StartGameInternal()
        {
            Board.Reset();
            CurrentTurn = CellState.X;
            LastGameResult = GameResult.None;
            IsAIThinking = false;
            
            ChangeState(State.Playing);
            OnTurnChanged?.Invoke(CurrentTurn);
        }
        
        /// <summary>
        /// Обрабатывает ход.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки</param>
        /// <returns>true если ход успешен</returns>
        private bool ProcessMove(int cellIndex)
        {
            if (!Board.MakeMove(cellIndex, CurrentTurn))
            {
                return false;
            }
            
            OnMoveMade?.Invoke(cellIndex, CurrentTurn);
            
            // Проверяем окончание игры
            GameResult result = Board.GetGameResult();
            
            if (result != GameResult.None)
            {
                EndGame(result);
                return true;
            }
            
            // Переключаем ход
            SwitchTurn();
            
            // Если теперь ход ИИ
            if (CurrentMode == GameMode.VsAI && IsAITurn())
            {
                HandleAITurn();
            }
            
            return true;
        }
        
        /// <summary>
        /// Переключает ход на другого игрока.
        /// </summary>
        private void SwitchTurn()
        {
            CurrentTurn = CurrentTurn == CellState.X ? CellState.O : CellState.X;
            OnTurnChanged?.Invoke(CurrentTurn);
        }
        
        /// <summary>
        /// Проверяет, ход ли сейчас ИИ.
        /// </summary>
        /// <returns>true если ход ИИ</returns>
        private bool IsAITurn()
        {
            if (_aiPlayer == null)
            {
                return false;
            }
            
            CellState aiSymbol = PlayerIsX ? CellState.O : CellState.X;
            return CurrentTurn == aiSymbol;
        }
        
        /// <summary>
        /// Запускает ход ИИ с небольшой задержкой для реалистичности.
        /// </summary>
        private void HandleAITurn()
        {
            if (_aiPlayer == null || CurrentState != State.Playing)
            {
                return;
            }
            
            StopAIThinking();
            _aiThinkCoroutine = StartCoroutine(AIThinkCoroutine());
        }
        
        /// <summary>
        /// Корутина для имитации "размышления" ИИ.
        /// </summary>
        private IEnumerator AIThinkCoroutine()
        {
            IsAIThinking = true;
            
            // Случайная задержка для реалистичности
            float delay = UnityEngine.Random.Range(AI_THINK_DELAY_MIN, AI_THINK_DELAY_MAX);
            yield return new WaitForSeconds(delay);
            
            if (CurrentState != State.Playing)
            {
                IsAIThinking = false;
                yield break;
            }
            
            // Получаем символ ИИ
            CellState aiSymbol = PlayerIsX ? CellState.O : CellState.X;
            
            // Запрашиваем ход у ИИ
            int aiMove = _aiPlayer.GetMove(Board, aiSymbol);
            
            IsAIThinking = false;
            
            if (aiMove >= 0 && aiMove < BoardModel.TOTAL_CELLS)
            {
                ProcessMove(aiMove);
            }
            else
            {
                Debug.LogWarning($"[GameManager] AI returned invalid move: {aiMove}");
            }
        }
        
        /// <summary>
        /// Останавливает корутину размышления ИИ.
        /// </summary>
        private void StopAIThinking()
        {
            if (_aiThinkCoroutine != null)
            {
                StopCoroutine(_aiThinkCoroutine);
                _aiThinkCoroutine = null;
            }
            IsAIThinking = false;
        }
        
        /// <summary>
        /// Завершает игру с указанным результатом.
        /// </summary>
        /// <param name="result">Результат игры</param>
        private void EndGame(GameResult result)
        {
            LastGameResult = result;
            ChangeState(State.GameOver);
            
            _matchesPlayed++;
            
            // TODO: Фаза 5 — обновить статистику в SaveSystem
            // SaveSystem.Instance?.UpdateStatistics(CurrentMode, result);
            
            // TODO: Фаза 7 — показать interstitial рекламу
            // if (_matchesPlayed >= MATCHES_BEFORE_INTERSTITIAL)
            // {
            //     _matchesPlayed = 0;
            //     AdsManager.Instance?.ShowInterstitial();
            // }
            
            OnGameEnded?.Invoke(result);
        }
        
        /// <summary>
        /// Меняет состояние игры.
        /// </summary>
        /// <param name="newState">Новое состояние</param>
        private void ChangeState(State newState)
        {
            if (CurrentState == newState)
            {
                return;
            }
            
            State previousState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"[GameManager] State: {previousState} → {newState}");
            
            OnStateChanged?.Invoke(newState);
        }
    }
}
