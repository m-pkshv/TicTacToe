// Assets/_Project/Scripts/Utils/Singleton.cs

using UnityEngine;

namespace TicTacToe.Utils
{
    /// <summary>
    /// Базовый класс для реализации паттерна Singleton на MonoBehaviour.
    /// Гарантирует существование только одного экземпляра объекта.
    /// </summary>
    /// <typeparam name="T">Тип наследника</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // ==================== СТАТИЧЕСКИЕ ПОЛЯ ====================
        
        /// <summary>Единственный экземпляр объекта</summary>
        private static T _instance;
        
        /// <summary>Объект для синхронизации потоков</summary>
        private static readonly object _lock = new object();
        
        /// <summary>Флаг завершения работы приложения</summary>
        private static bool _applicationIsQuitting = false;
        
        // ==================== СВОЙСТВА ====================
        
        /// <summary>
        /// Получает единственный экземпляр объекта.
        /// Создаёт новый, если не существует (опционально).
        /// </summary>
        public static T Instance
        {
            get
            {
                // Проверка на выход из приложения
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. " +
                                     "Won't create again - returning null.");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Ищем существующий экземпляр в сцене
                        _instance = FindAnyObjectByType<T>();
                        
                        if (_instance == null)
                        {
                            // Создаём новый объект, если не найден
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T)}";
                            
                            // Не уничтожаем при загрузке новой сцены
                            DontDestroyOnLoad(singletonObject);
                            
                            Debug.Log($"[Singleton] An instance of '{typeof(T)}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] Using instance already created: '{_instance.gameObject.name}'");
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        /// <summary>
        /// Проверяет, существует ли экземпляр Singleton.
        /// Не создаёт новый экземпляр при проверке.
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        /// <summary>
        /// Проверяет, был ли экземпляр уничтожен при выходе из приложения.
        /// </summary>
        public static bool IsApplicationQuitting => _applicationIsQuitting;
        
        // ==================== UNITY LIFECYCLE ====================
        
        /// <summary>
        /// Вызывается при инициализации объекта.
        /// Переопределите в наследниках и вызовите base.Awake().
        /// </summary>
        protected virtual void Awake()
        {
            InitializeSingleton();
        }
        
        /// <summary>
        /// Вызывается при уничтожении объекта.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Вызывается при выходе из приложения.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
        
        // ==================== МЕТОДЫ ====================
        
        /// <summary>
        /// Инициализирует Singleton.
        /// Уничтожает дубликаты, если они существуют.
        /// </summary>
        private void InitializeSingleton()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of '{typeof(T)}' already exists! " +
                                 $"Destroying '{gameObject.name}'");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Явно уничтожает экземпляр Singleton.
        /// Используйте с осторожностью!
        /// </summary>
        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }
        
        /// <summary>
        /// Сбрасывает флаг выхода из приложения.
        /// Полезно для тестов в редакторе.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ResetApplicationQuitFlag()
        {
            _applicationIsQuitting = false;
        }
    }
    
    /// <summary>
    /// Вариант Singleton без автоматического создания.
    /// Экземпляр должен быть создан вручную в сцене.
    /// </summary>
    /// <typeparam name="T">Тип наследника</typeparam>
    public abstract class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        
        /// <summary>
        /// Получает экземпляр (без автосоздания).
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning($"[SingletonPersistent] No instance of '{typeof(T)}' found in scene. " +
                                     "Make sure to add it to the scene.");
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Проверяет, существует ли экземпляр.
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[SingletonPersistent] Duplicate instance of '{typeof(T)}' destroyed.");
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Вызывается после успешной инициализации Singleton.
        /// Переопределите для добавления логики инициализации.
        /// </summary>
        protected virtual void OnSingletonAwake() { }
    }
}
