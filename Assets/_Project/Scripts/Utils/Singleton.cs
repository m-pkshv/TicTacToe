// Assets/_Project/Scripts/Utils/Singleton.cs

using UnityEngine;

namespace TicTacToe.Utils
{
    /// <summary>
    /// Базовый класс для реализации паттерна Singleton на основе MonoBehaviour.
    /// Гарантирует единственный экземпляр компонента в сцене.
    /// </summary>
    /// <typeparam name="T">Тип класса-наследника</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Глобальный доступ к экземпляру синглтона
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<T>();

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T)}";

                            Debug.Log($"[Singleton] An instance of '{typeof(T)}' was created.");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Проверяет, существует ли экземпляр синглтона
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Должен ли синглтон сохраняться между сценами
        /// </summary>
        protected virtual bool Persistent => true;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (Persistent)
                {
                    // Убедимся, что объект находится в корне иерархии
                    if (transform.parent != null)
                    {
                        transform.SetParent(null);
                    }
                    DontDestroyOnLoad(gameObject);
                }

                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of '{typeof(T)}' found. Destroying this instance.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Вызывается при инициализации синглтона (переопределите вместо Awake)
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                OnSingletonDestroy();
            }
        }

        /// <summary>
        /// Вызывается при уничтожении синглтона (переопределите вместо OnDestroy)
        /// </summary>
        protected virtual void OnSingletonDestroy() { }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
