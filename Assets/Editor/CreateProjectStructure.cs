// Assets/Editor/CreateProjectStructure.cs
// Скрипт для создания структуры папок проекта TicTacToe
// Инструкция: 
// 1. Создайте папку Assets/Editor (если её нет)
// 2. Поместите этот файл в Assets/Editor/
// 3. В Unity: меню Tools → TicTacToe → Create Project Structure

using UnityEngine;
using UnityEditor;
using System.IO;

namespace TicTacToe.Editor
{
    /// <summary>
    /// Редакторный скрипт для создания структуры папок проекта
    /// </summary>
    public static class CreateProjectStructure
    {
        private const string PROJECT_ROOT = "Assets/_Project";
        
        [MenuItem("Tools/TicTacToe/Create Project Structure", false, 1)]
        public static void CreateStructure()
        {
            // Подтверждение
            if (!EditorUtility.DisplayDialog(
                "Create Project Structure",
                "Создать структуру папок проекта TicTacToe?\n\nЭто создаст все необходимые папки согласно TDD.",
                "Создать",
                "Отмена"))
            {
                return;
            }

            int foldersCreated = 0;

            // ===== SCRIPTS =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Core");
            
            // Game
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Game/Models");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Game/Presenters");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Game/Enums");
            
            // AI
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/AI");
            
            // Network
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Network/Messages");
            
            // UI
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/UI/Screens");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/UI/Components");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/UI/Popups");
            
            // Audio
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Audio");
            
            // Themes
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Themes");
            
            // Save
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Save");
            
            // Monetization
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Monetization/Ads");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Monetization/IAP");
            
            // Analytics
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Analytics");
            
            // Localization
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Localization");
            
            // Utils
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scripts/Utils/Extensions");

            // ===== PREFABS =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Prefabs/Core");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Prefabs/UI/Screens");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Prefabs/UI/Components");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Prefabs/UI/Popups");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Prefabs/Effects");

            // ===== SCENES =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Scenes");

            // ===== SCRIPTABLE OBJECTS =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/ScriptableObjects/Themes");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/ScriptableObjects/Config");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/ScriptableObjects/Products");

            // ===== UI ASSETS =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/UI/Sprites/Icons");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/UI/Sprites/Buttons");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/UI/Sprites/Backgrounds");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/UI/Fonts");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/UI/Animations");

            // ===== AUDIO =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Audio/SFX");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Audio/Music");

            // ===== THEMES (Assets) =====
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Classic");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Neon");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Nature");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Minimal");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Space");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Cyberpunk");
            foldersCreated += CreateFolder($"{PROJECT_ROOT}/Themes/Retro");

            // ===== PLUGINS =====
            foldersCreated += CreateFolder("Assets/Plugins/Android");
            foldersCreated += CreateFolder("Assets/Plugins/iOS");

            // ===== SETTINGS =====
            foldersCreated += CreateFolder("Assets/Settings");

            // ===== STREAMING ASSETS (для WebGL) =====
            foldersCreated += CreateFolder("Assets/StreamingAssets");

            // ===== EDITOR =====
            foldersCreated += CreateFolder("Assets/Editor");

            // Обновить Asset Database
            AssetDatabase.Refresh();

            // Результат
            EditorUtility.DisplayDialog(
                "Готово!",
                $"Структура проекта создана.\nСоздано папок: {foldersCreated}",
                "OK");

            Debug.Log($"[TicTacToe] Структура проекта создана. Папок создано: {foldersCreated}");
        }

        /// <summary>
        /// Создаёт папку если она не существует
        /// </summary>
        /// <returns>1 если папка создана, 0 если уже существует</returns>
        private static int CreateFolder(string path)
        {
            // Преобразуем путь Unity в системный путь
            string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), path);
            
            if (Directory.Exists(fullPath))
            {
                return 0;
            }

            Directory.CreateDirectory(fullPath);
            Debug.Log($"[TicTacToe] Создана папка: {path}");
            return 1;
        }

        [MenuItem("Tools/TicTacToe/Open Project Folder", false, 2)]
        public static void OpenProjectFolder()
        {
            string path = Path.Combine(Application.dataPath, "_Project");
            
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Папка не найдена",
                    "Сначала создайте структуру проекта:\nTools → TicTacToe → Create Project Structure",
                    "OK");
            }
        }

        [MenuItem("Tools/TicTacToe/Validate Structure", false, 3)]
        public static void ValidateStructure()
        {
            string[] requiredFolders = new[]
            {
                $"{PROJECT_ROOT}/Scripts/Core",
                $"{PROJECT_ROOT}/Scripts/Game/Models",
                $"{PROJECT_ROOT}/Scripts/Game/Presenters",
                $"{PROJECT_ROOT}/Scripts/Game/Enums",
                $"{PROJECT_ROOT}/Scripts/AI",
                $"{PROJECT_ROOT}/Scripts/Network",
                $"{PROJECT_ROOT}/Scripts/UI/Screens",
                $"{PROJECT_ROOT}/Scripts/Utils",
                $"{PROJECT_ROOT}/Prefabs",
                $"{PROJECT_ROOT}/Scenes",
                $"{PROJECT_ROOT}/ScriptableObjects"
            };

            int missing = 0;
            string missingList = "";

            foreach (string folder in requiredFolders)
            {
                string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), folder);
                if (!Directory.Exists(fullPath))
                {
                    missing++;
                    missingList += $"\n• {folder}";
                }
            }

            if (missing == 0)
            {
                EditorUtility.DisplayDialog(
                    "Валидация пройдена ✓",
                    "Все необходимые папки присутствуют.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Отсутствующие папки",
                    $"Найдено {missing} отсутствующих папок:{missingList}\n\nЗапустите Create Project Structure для их создания.",
                    "OK");
            }
        }
    }
}
