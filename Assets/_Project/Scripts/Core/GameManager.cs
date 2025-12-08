// Assets/_Project/Scripts/Core/GameManager.cs

using System;
using System.Collections;
using UnityEngine;
using TicTacToe.AI;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Models;
using TicTacToe.Game.Presenters;
using TicTacToe.Save;
using TicTacToe.Utils;

namespace TicTacToe.Core
{
    /// <summary>
    /// Центральный контроллер игры (Singleton).
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
        public void SetPresenter(GamePresenter presenter)
        {
            _presenter = presenter;
        }
        
        /// <summary>
        /// Устанавливает состояние игры напрямую (для UI).
        /// </summary>
        public void SetState(State newState)
        {
            ChangeState(newState);
        }
        
        /// <summary>
        /// Переход к выбору сложности.
        /// </summary>
        public void GoToDifficultySelect()
        {
            ChangeState(State.DifficultySelect);
        }
        
        /// <summary>
        /// Переход к сетевому лобби.
        /// </summary>
        public void GoToLobby()
        {
            ChangeState(State.Lobby);
        }
        
        /// <summary>
        /// Начинает игру против ИИ.
        /// </summary>
        /// <param name="difficulty">Уровень сложности</param>
        /// <param name="playerIsX">Играет ли игрок за X</param>
        public void StartGameVsAI(AIDifficulty difficulty, bool playerIsX = true)
        {
            CurrentMode = GameMode.VsAI;
            CurrentDifficulty = difficulty;
            PlayerIsX = playerIsX;
            
            // Создаём ИИ через фабрику
            _aiPlayer = AIFactory.Create(difficulty);
            
            StartNewGame();
            
            // Если ИИ ходит первым (игрок за O)
            if (!playerIsX)
            {
                HandleAITurn();
            }
        }
        
        /// <summary>
        /// Начинает локальную игру для двух игроков.
        /// </summary>
        public void StartLocalMultiplayer()
        {
            CurrentMode = GameMode.LocalMultiplayer;
            _aiPlayer = null;
            PlayerIsX = true;
            
            StartNewGame();
        }
        
        /// <summary>
        /// Начинает сетевую игру.
        /// </summary>
        /// <param name="isHost">Является ли игрок хостом</param>
        public void StartNetworkMultiplayer(bool isHost = true)
        {
            CurrentMode = GameMode.NetworkMultiplayer;
            _aiPlayer = null;
            PlayerIsX = isHost;
            
            // TODO: Фаза 8 — Network implementation
            ChangeState(State.WaitingForPlayer);
        }
        
        /// <summary>
        /// Делает ход в указанную ячейку.
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки (0-8)</param>
        /// <returns>True, если ход успешен</returns>
        public bool MakeMove(int cellIndex)
        {
            // Проверки
            if (CurrentState != State.Playing)
            {
                Debug.LogWarning("[GameManager] Cannot make move: game not in Playing state");
                return false;
            }
            
            if (IsAIThinking)
            {
                Debug.LogWarning("[GameManager] Cannot make move: AI is thinking");
                return false;
            }
            
            // В режиме VsAI проверяем, что сейчас ход игрока
            if (CurrentMode == GameMode.VsAI && IsAITurn())
            {
                Debug.LogWarning("[GameManager] Cannot make move: it's AI's turn");
                return false;
            }
            
            return ProcessMove(cellIndex);
        }
        
        /// <summary>
        /// Перезапускает текущую игру.
        /// </summary>
        public void Restart()
        {
            StopAIThinking();
            
            if (CurrentMode == GameMode.VsAI)
            {
                StartGameVsAI(CurrentDifficulty, PlayerIsX);
            }
            else if (CurrentMode == GameMode.LocalMultiplayer)
            {
                StartLocalMultiplayer();
            }
            else
            {
                StartNewGame();
            }
        }
        
        /// <summary>
        /// Выход в главное меню.
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
        /// Начинает новую игру (общая логика).
        /// </summary>
        private void StartNewGame()
        {
            Board.Reset();
            CurrentTurn = CellState.X;
            LastGameResult = GameResult.None;
            
            ChangeState(State.Playing);
            
            OnTurnChanged?.Invoke(CurrentTurn);
        }
        
        /// <summary>
        /// Обрабатывает ход.
        /// </summary>
        private bool ProcessMove(int cellIndex)
        {
            // Делаем ход в модели
            bool success = Board.MakeMove(cellIndex, CurrentTurn);
            
            if (!success)
            {
                return false;
            }
            
            // Уведомляем о сделанном ходе
            OnMoveMade?.Invoke(cellIndex, CurrentTurn);
            
            // Проверяем результат
            GameResult result = Board.GetGameResult();
            
            if (result != GameResult.None)
            {
                EndGame(result);
                return true;
            }
            
            // Меняем ход
            SwitchTurn();
            
            // Если следующий ход ИИ
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
        /// Проверяет, является ли текущий ход ходом ИИ.
        /// </summary>
        private bool IsAITurn()
        {
            if (CurrentMode != GameMode.VsAI || _aiPlayer == null)
            {
                return false;
            }
            
            // Если игрок за X, то ИИ за O (и наоборот)
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
            
            // === ИНТЕГРАЦИЯ С SAVE SYSTEM ===
            RecordGameResult(result);
            
            // TODO: Фаза 7 — показать interstitial рекламу
            // if (ShouldShowInterstitial())
            // {
            //     _matchesPlayed = 0;
            //     AdsManager.Instance?.ShowInterstitial();
            // }
            
            OnGameEnded?.Invoke(result);
        }
        
        /// <summary>
        /// Записывает результат игры в SaveSystem.
        /// </summary>
        /// <param name="result">Результат игры</param>
        private void RecordGameResult(GameResult result)
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem?.Data == null)
            {
                Debug.LogWarning("[GameManager] SaveSystem not available, result not saved");
                return;
            }
            
            switch (CurrentMode)
            {
                case GameMode.VsAI:
                    RecordAIGameResult(result);
                    break;
                    
                case GameMode.LocalMultiplayer:
                    RecordLocalMultiplayerResult(result);
                    break;
                    
                case GameMode.NetworkMultiplayer:
                    RecordNetworkMultiplayerResult(result);
                    break;
            }
            
            // Сохраняем немедленно после игры
            saveSystem.ForceSave();
            
            Debug.Log($"[GameManager] Game result recorded: {CurrentMode} - {result}");
        }
        
        /// <summary>
        /// Записывает результат игры против ИИ.
        /// </summary>
        private void RecordAIGameResult(GameResult result)
        {
            // Определяем, выиграл ли игрок
            bool playerWon = (result == GameResult.XWins && PlayerIsX) ||
                            (result == GameResult.OWins && !PlayerIsX);
            bool isDraw = result == GameResult.Draw;
            
            // Индекс сложности: Easy=0, Medium=1, Hard=2
            int difficultyIndex = (int)CurrentDifficulty;
            
            SaveSystem.Instance.RecordAIGameResult(difficultyIndex, playerWon, isDraw);
        }
        
        /// <summary>
        /// Записывает результат локальной мультиплеерной игры.
        /// </summary>
        private void RecordLocalMultiplayerResult(GameResult result)
        {
            bool playerXWon = result == GameResult.XWins;
            bool isDraw = result == GameResult.Draw;
            
            SaveSystem.Instance.RecordLocalMultiplayerResult(playerXWon, isDraw);
        }
        
        /// <summary>
        /// Записывает результат сетевой мультиплеерной игры.
        /// </summary>
        private void RecordNetworkMultiplayerResult(GameResult result)
        {
            // Определяем, выиграл ли игрок (зависит от того, за кого он играет)
            bool playerWon = (result == GameResult.XWins && PlayerIsX) ||
                            (result == GameResult.OWins && !PlayerIsX);
            bool isDraw = result == GameResult.Draw;
            
            SaveSystem.Instance.RecordNetworkMultiplayerResult(playerWon, isDraw);
        }
        
        /// <summary>
        /// Проверяет, нужно ли показать interstitial рекламу.
        /// </summary>
        private bool ShouldShowInterstitial()
        {
            // Проверяем, куплено ли отключение рекламы
            if (SaveSystem.Instance?.IsAdsRemoved() == true)
            {
                return false;
            }
            
            return _matchesPlayed >= MATCHES_BEFORE_INTERSTITIAL;
        }
        
        /// <summary>
        /// Меняет состояние игры.
        /// </summary>
        private void ChangeState(State newState)
        {
            if (CurrentState == newState)
            {
                return;
            }
            
            State oldState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"[GameManager] State: {oldState} → {newState}");
            
            OnStateChanged?.Invoke(newState);
        }
    }
}
