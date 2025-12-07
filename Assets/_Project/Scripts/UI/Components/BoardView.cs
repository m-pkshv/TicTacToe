// Assets/_Project/Scripts/UI/Components/BoardView.cs

using System;
using System.Collections;
using UnityEngine;
using TicTacToe.Game.Enums;
using TicTacToe.Game.Presenters;
using TicTacToe.Game.Models;

namespace TicTacToe.UI.Components
{
    /// <summary>
    /// Компонент игрового поля 3x3, реализует IGameView
    /// </summary>
    public class BoardView : MonoBehaviour, IGameView
    {
        private const float WIN_LINE_HIGHLIGHT_DELAY = 0.1f;
        
        [Header("Cell References")]
        [SerializeField] private CellView[] _cells = new CellView[BoardModel.TOTAL_CELLS];
        
        [Header("Grid Settings")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private float _cellSpacing = 10f;
        
        private bool _isInputLocked;
        private CellState _currentTurnSymbol = CellState.X;
        private Coroutine _winHighlightCoroutine;
        
        /// <summary>
        /// Событие клика по ячейке
        /// </summary>
        public event Action<int> OnCellClicked;
        
        /// <summary>
        /// Заблокирован ли ввод
        /// </summary>
        public bool IsInputLocked => _isInputLocked;
        
        private void Awake()
        {
            // Подписываемся на события всех ячеек
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].Initialize(i);
                    _cells[i].OnCellClicked += HandleCellClicked;
                }
            }
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].OnCellClicked -= HandleCellClicked;
                }
            }
            
            if (_winHighlightCoroutine != null)
            {
                StopCoroutine(_winHighlightCoroutine);
            }
        }
        
        /// <summary>
        /// Обновить состояние ячейки
        /// </summary>
        /// <param name="index">Индекс ячейки (0-8)</param>
        /// <param name="state">Новое состояние</param>
        public void UpdateCell(int index, CellState state)
        {
            if (index < 0 || index >= _cells.Length)
            {
                Debug.LogWarning($"[BoardView] Invalid cell index: {index}");
                return;
            }
            
            if (_cells[index] != null)
            {
                _cells[index].SetState(state, animate: true);
                _cells[index].HideHoverPreview();
            }
        }
        
        /// <summary>
        /// Показать текущий ход
        /// </summary>
        /// <param name="currentTurn">Чей сейчас ход</param>
        public void ShowCurrentTurn(CellState currentTurn)
        {
            _currentTurnSymbol = currentTurn;
            
            // Обновляем интерактивность ячеек
            UpdateCellsInteractivity();
        }
        
        /// <summary>
        /// Показать результат игры
        /// </summary>
        /// <param name="result">Результат</param>
        public void ShowGameResult(GameResult result)
        {
            // Блокируем ввод при завершении игры
            SetInputLocked(true);
            
            // Скрываем все превью
            HideAllHoverPreviews();
        }
        
        /// <summary>
        /// Подсветить выигрышную линию
        /// </summary>
        /// <param name="winningIndices">Индексы выигрышных ячеек</param>
        public void HighlightWinningLine(int[] winningIndices)
        {
            if (winningIndices == null || winningIndices.Length == 0)
            {
                return;
            }
            
            if (_winHighlightCoroutine != null)
            {
                StopCoroutine(_winHighlightCoroutine);
            }
            
            _winHighlightCoroutine = StartCoroutine(AnimateWinningLine(winningIndices));
        }
        
        /// <summary>
        /// Сбросить поле
        /// </summary>
        public void ResetBoard()
        {
            if (_winHighlightCoroutine != null)
            {
                StopCoroutine(_winHighlightCoroutine);
                _winHighlightCoroutine = null;
            }
            
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].ResetCell();
                }
            }
            
            _isInputLocked = false;
            _currentTurnSymbol = CellState.X;
            UpdateCellsInteractivity();
        }
        
        /// <summary>
        /// Заблокировать/разблокировать ввод
        /// </summary>
        /// <param name="locked">Заблокировать</param>
        public void SetInputLocked(bool locked)
        {
            _isInputLocked = locked;
            UpdateCellsInteractivity();
            
            if (locked)
            {
                HideAllHoverPreviews();
            }
        }
        
        /// <summary>
        /// Показать индикатор "ИИ думает"
        /// </summary>
        /// <param name="isThinking">Показать/скрыть</param>
        public void ShowAIThinking(bool isThinking)
        {
            // Блокируем ввод пока ИИ думает
            SetInputLocked(isThinking);
            
            // TODO: Добавить визуальный индикатор "AI думает..."
            // Это можно сделать через GameScreen
        }
        
        /// <summary>
        /// Показать ожидание игрока (для сетевой игры)
        /// </summary>
        /// <param name="isWaiting">Показать/скрыть</param>
        public void ShowWaitingForPlayer(bool isWaiting)
        {
            SetInputLocked(isWaiting);
            
            // TODO: Добавить визуальный индикатор ожидания
            // Это можно сделать через GameScreen
        }
        
        /// <summary>
        /// Получить ячейку по индексу
        /// </summary>
        /// <param name="index">Индекс (0-8)</param>
        /// <returns>CellView или null</returns>
        public CellView GetCell(int index)
        {
            if (index < 0 || index >= _cells.Length)
            {
                return null;
            }
            
            return _cells[index];
        }
        
        /// <summary>
        /// Установить ссылки на ячейки программно
        /// </summary>
        /// <param name="cells">Массив ячеек</param>
        public void SetCells(CellView[] cells)
        {
            if (cells == null || cells.Length != BoardModel.TOTAL_CELLS)
            {
                Debug.LogError($"[BoardView] Cells array must have exactly {BoardModel.TOTAL_CELLS} elements");
                return;
            }
            
            // Отписываемся от старых
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].OnCellClicked -= HandleCellClicked;
                }
            }
            
            _cells = cells;
            
            // Подписываемся на новые
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].Initialize(i);
                    _cells[i].OnCellClicked += HandleCellClicked;
                }
            }
        }
        
        /// <summary>
        /// Показать превью хода при наведении на ячейку
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки</param>
        public void ShowHoverPreview(int cellIndex)
        {
            if (_isInputLocked || cellIndex < 0 || cellIndex >= _cells.Length)
            {
                return;
            }
            
            _cells[cellIndex]?.ShowHoverPreview(_currentTurnSymbol);
        }
        
        /// <summary>
        /// Скрыть превью хода
        /// </summary>
        /// <param name="cellIndex">Индекс ячейки</param>
        public void HideHoverPreview(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= _cells.Length)
            {
                return;
            }
            
            _cells[cellIndex]?.HideHoverPreview();
        }
        
        /// <summary>
        /// Применить тему к полю
        /// </summary>
        /// <param name="spriteX">Спрайт X</param>
        /// <param name="spriteO">Спрайт O</param>
        /// <param name="colorX">Цвет X</param>
        /// <param name="colorO">Цвет O</param>
        public void ApplyTheme(Sprite spriteX, Sprite spriteO, Color colorX, Color colorO)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].SetSymbolSprites(spriteX, spriteO);
                    _cells[i].SetSymbolColors(colorX, colorO);
                }
            }
        }
        
        private void HandleCellClicked(int cellIndex)
        {
            if (_isInputLocked)
            {
                return;
            }
            
            OnCellClicked?.Invoke(cellIndex);
        }
        
        private void UpdateCellsInteractivity()
        {
            bool canInteract = !_isInputLocked;
            
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].IsInteractable = canInteract;
                }
            }
        }
        
        private void HideAllHoverPreviews()
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i]?.HideHoverPreview();
            }
        }
        
        private IEnumerator AnimateWinningLine(int[] winningIndices)
        {
            // Анимируем появление подсветки с небольшой задержкой между ячейками
            for (int i = 0; i < winningIndices.Length; i++)
            {
                int index = winningIndices[i];
                
                if (index >= 0 && index < _cells.Length && _cells[index] != null)
                {
                    _cells[index].SetHighlight(true);
                    
                    if (i < winningIndices.Length - 1)
                    {
                        yield return new WaitForSeconds(WIN_LINE_HIGHLIGHT_DELAY);
                    }
                }
            }
            
            _winHighlightCoroutine = null;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Проверяем, что массив имеет правильный размер
            if (_cells == null || _cells.Length != BoardModel.TOTAL_CELLS)
            {
                System.Array.Resize(ref _cells, BoardModel.TOTAL_CELLS);
            }
        }
        
        /// <summary>
        /// Автоматический поиск ячеек в дочерних объектах (для удобства в редакторе)
        /// </summary>
        [ContextMenu("Auto Find Cells")]
        private void AutoFindCells()
        {
            CellView[] foundCells = GetComponentsInChildren<CellView>();
            
            if (foundCells.Length == BoardModel.TOTAL_CELLS)
            {
                _cells = foundCells;
                Debug.Log($"[BoardView] Found {foundCells.Length} cells");
            }
            else
            {
                Debug.LogWarning($"[BoardView] Expected {BoardModel.TOTAL_CELLS} cells, found {foundCells.Length}");
            }
        }
#endif
    }
}
