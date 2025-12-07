// Assets/Editor/SetupPlayerSettings.cs
// Скрипт для настройки Player Settings проекта TicTacToe
// Согласно TDD спецификации
//
// Инструкция:
// 1. Поместите этот файл в Assets/Editor/
// 2. В Unity: меню Tools → TicTacToe → Setup Player Settings
// 3. Скрипт настроит все платформы автоматически

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Collections.Generic;

namespace TicTacToe.Editor
{
    /// <summary>
    /// Редакторный скрипт для настройки Player Settings всех платформ
    /// </summary>
    public static class SetupPlayerSettings
    {
        // ===== КОНФИГУРАЦИЯ ПРОЕКТА =====
        private const string COMPANY_NAME = "Maksim Pekshev";  // TODO: Заменить
        private const string PRODUCT_NAME = "Tic Tac Toe";
        private const string BUNDLE_ID_ANDROID = "com.maksimpekshev.tictactoe";  // TODO: Заменить
        private const string BUNDLE_ID_IOS = "com.maksimpekshev.tictactoe";      // TODO: Заменить
        private const string VERSION = "1.0.0";
        private const int BUNDLE_VERSION_CODE = 1;

        [MenuItem("Tools/TicTacToe/Setup Player Settings", false, 10)]
        public static void SetupAll()
        {
            if (!EditorUtility.DisplayDialog(
                "Setup Player Settings",
                "Настроить Player Settings для всех платформ согласно TDD?\n\n" +
                "Будут настроены:\n" +
                "• Общие настройки (Company, Product, Version)\n" +
                "• Android (API 24-34, IL2CPP, ARM64)\n" +
                "• iOS (iOS 13+, ARM64)\n" +
                "• Standalone (Windows/macOS)\n" +
                "• WebGL (Gzip, 256MB)\n\n" +
                "ВАЖНО: Проверьте Bundle ID в скрипте!",
                "Настроить",
                "Отмена"))
            {
                return;
            }

            SetupCommonSettings();
            SetupAndroid();
            SetupIOS();
            SetupStandalone();
            SetupWebGL();

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Готово!",
                "Player Settings настроены для всех платформ.\n\n" +
                "Не забудьте:\n" +
                "1. Заменить Company Name и Bundle ID\n" +
                "2. Добавить иконки приложения\n" +
                "3. Настроить Keystore для Android",
                "OK");

            Debug.Log("[TicTacToe] Player Settings настроены для всех платформ");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Common Only", false, 11)]
        public static void SetupCommonSettings()
        {
            Debug.Log("[TicTacToe] Настройка общих параметров...");

            // Основные настройки
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.bundleVersion = VERSION;

            // Ориентация экрана (только портретная для мобильных)
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Rendering
            PlayerSettings.colorSpace = ColorSpace.Linear;  // Лучше для UI
            
            // Scripting
            PlayerSettings.SetApiCompatibilityLevel(
                BuildTargetGroup.Android, 
                ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetApiCompatibilityLevel(
                BuildTargetGroup.iOS, 
                ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetApiCompatibilityLevel(
                BuildTargetGroup.Standalone, 
                ApiCompatibilityLevel.NET_Standard);

            Debug.Log("[TicTacToe] ✓ Общие настройки применены");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Android", false, 12)]
        public static void SetupAndroid()
        {
            Debug.Log("[TicTacToe] Настройка Android...");

            // Package Name
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Android, 
                BUNDLE_ID_ANDROID);

            // Bundle Version Code
            PlayerSettings.Android.bundleVersionCode = BUNDLE_VERSION_CODE;

            // API Levels (согласно TDD: min 24, target 34)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;

            // Scripting Backend: IL2CPP (обязательно для релиза)
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android, 
                ScriptingImplementation.IL2CPP);

            // Архитектура: ARMv7 + ARM64 (согласно TDD)
            PlayerSettings.Android.targetArchitectures = 
                AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            // Ориентация
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Splash Screen
            PlayerSettings.Android.showActivityIndicatorOnLoading = 
                AndroidShowActivityIndicatorOnLoading.DontShow;

            // Разрешения (автоматически через манифест, но можно указать)
            // INTERNET, ACCESS_NETWORK_STATE, ACCESS_WIFI_STATE - для мультиплеера
            // VIBRATE - для вибрации
            // AD_ID - для рекламы

            // Incremental IL2CPP builds (ускоряет сборку)
            PlayerSettings.Android.useCustomKeystore = false;  // TODO: Включить для релиза

            // Build settings
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;

            Debug.Log("[TicTacToe] ✓ Android настроен (API 24-34, IL2CPP, ARMv7+ARM64)");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/iOS", false, 13)]
        public static void SetupIOS()
        {
            Debug.Log("[TicTacToe] Настройка iOS...");

            // Bundle Identifier
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.iOS, 
                BUNDLE_ID_IOS);

            // Build Number
            PlayerSettings.iOS.buildNumber = BUNDLE_VERSION_CODE.ToString();

            // Minimum iOS Version (согласно TDD: 13.0)
            PlayerSettings.iOS.targetOSVersionString = "13.0";

            // Architecture: ARM64 only (согласно TDD)
            PlayerSettings.SetArchitecture(
                BuildTargetGroup.iOS, 
                (int)AppleMobileArchitecture.ARM64);

            // Scripting Backend: IL2CPP (обязательно для iOS)
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.iOS, 
                ScriptingImplementation.IL2CPP);

            // Ориентация
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Скрыть status bar
            PlayerSettings.statusBarHidden = true;

            // Требовать постоянный WiFi (для мультиплеера)
            PlayerSettings.iOS.requiresPersistentWiFi = false;  // Опционально

            // Camera/Microphone Usage (не нужны для этой игры)
            PlayerSettings.iOS.cameraUsageDescription = "";
            PlayerSettings.iOS.microphoneUsageDescription = "";

            // Location Usage (не нужен)
            PlayerSettings.iOS.locationUsageDescription = "";

            Debug.Log("[TicTacToe] ✓ iOS настроен (iOS 13+, ARM64, IL2CPP)");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Standalone", false, 14)]
        public static void SetupStandalone()
        {
            Debug.Log("[TicTacToe] Настройка Standalone (Windows/macOS)...");

            // Scripting Backend (согласно TDD: Mono или IL2CPP для Windows, IL2CPP для macOS)
            // Используем Mono для быстрой отладки, IL2CPP для релиза
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Standalone, 
                ScriptingImplementation.Mono2x);  // Для разработки

            // Разрешение экрана
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 540;   // 9:16 портретное
            PlayerSettings.defaultScreenHeight = 960;

            // Полноэкранный режим
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            // macOS специфичные настройки
            PlayerSettings.macOS.buildNumber = BUNDLE_VERSION_CODE.ToString();

            // Standalone Input
            PlayerSettings.defaultIsNativeResolution = false;

            Debug.Log("[TicTacToe] ✓ Standalone настроен (Mono, Windowed 540x960)");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/WebGL", false, 15)]
        public static void SetupWebGL()
        {
            Debug.Log("[TicTacToe] Настройка WebGL...");

            // Compression (согласно TDD: Gzip)
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;

            // Memory Size (согласно TDD: 256MB)
            PlayerSettings.WebGL.memorySize = 256;

            // Exception Handling (отключить для размера)
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

            // Data Caching
            PlayerSettings.WebGL.dataCaching = true;

            // Template
            PlayerSettings.WebGL.template = "APPLICATION:Default";

            // Code Stripping (согласно TDD: High)
            PlayerSettings.SetManagedStrippingLevel(
                BuildTargetGroup.WebGL, 
                ManagedStrippingLevel.High);

            // Decompression Fallback (для старых браузеров)
            PlayerSettings.WebGL.decompressionFallback = true;

            // Power Preference
            PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.HighPerformance;

            Debug.Log("[TicTacToe] ✓ WebGL настроен (Gzip, 256MB, High Stripping)");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Switch to IL2CPP (Release)", false, 20)]
        public static void SwitchToIL2CPP()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            
            Debug.Log("[TicTacToe] Scripting Backend переключён на IL2CPP для Android и Standalone");
            EditorUtility.DisplayDialog("IL2CPP", "Scripting Backend переключён на IL2CPP.\nСборка будет медленнее, но быстрее в рантайме.", "OK");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Switch to Mono (Debug)", false, 21)]
        public static void SwitchToMono()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            
            Debug.Log("[TicTacToe] Scripting Backend переключён на Mono для Android и Standalone");
            EditorUtility.DisplayDialog("Mono", "Scripting Backend переключён на Mono.\nБыстрее собирается, но медленнее в рантайме.", "OK");
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Validate Settings", false, 30)]
        public static void ValidateSettings()
        {
            var issues = new List<string>();

            // Проверка Company Name
            if (PlayerSettings.companyName == "YourCompanyName" || 
                PlayerSettings.companyName == "DefaultCompany")
            {
                issues.Add("• Company Name не изменён");
            }

            // Проверка Bundle ID
            string androidId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (androidId.Contains("yourcompany"))
            {
                issues.Add("• Android Bundle ID содержит 'yourcompany'");
            }

            string iosId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            if (iosId.Contains("yourcompany"))
            {
                issues.Add("• iOS Bundle ID содержит 'yourcompany'");
            }

            // Проверка Android API
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            {
                issues.Add("• Android Min API ниже 24 (требуется 24+)");
            }

            // Проверка Scripting Backend для релиза
            var androidBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            if (androidBackend != ScriptingImplementation.IL2CPP)
            {
                issues.Add("• Android использует Mono (для релиза нужен IL2CPP)");
            }

            // Проверка iOS версии
            if (string.IsNullOrEmpty(PlayerSettings.iOS.targetOSVersionString) ||
                float.Parse(PlayerSettings.iOS.targetOSVersionString) < 13.0f)
            {
                issues.Add("• iOS Target Version ниже 13.0");
            }

            // Результат
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Валидация пройдена ✓",
                    "Все настройки соответствуют TDD спецификации.",
                    "OK");
            }
            else
            {
                string message = "Найдены проблемы:\n\n" + string.Join("\n", issues);
                EditorUtility.DisplayDialog(
                    "Требуются исправления",
                    message,
                    "OK");
            }
        }

        [MenuItem("Tools/TicTacToe/Setup Player Settings/Open Player Settings", false, 100)]
        public static void OpenPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
    }
}
