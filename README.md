# Tic-Tac-Toe

Кроссплатформенная игра "Крестики-нолики" на Unity 6.

## 🎮 Особенности

- **Режимы игры:** vs AI, локальный мультиплеер, сетевой LAN
- **AI:** 3 уровня сложности (Easy, Medium, Hard с Minimax)
- **Темы оформления:** 7 уникальных тем
- **Платформы:** Android, iOS, Windows, macOS, WebGL

## 🛠 Технологии

- **Движок:** Unity 6 (6000.x)
- **Архитектура:** MVP (Model-View-Presenter)
- **Сеть:** Unity Netcode for GameObjects
- **Монетизация:** AdMob + Unity Ads, Unity IAP

## 📁 Структура проекта

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/           # GameManager, Bootstrap
    │   ├── Game/           # Models, Presenters, Enums
    │   ├── AI/             # AI игроки
    │   ├── Network/        # Сетевой код
    │   ├── UI/             # UI система
    │   ├── Themes/         # Система тем
    │   ├── Save/           # Сохранения
    │   ├── Monetization/   # Реклама и IAP
    │   └── Audio/          # Звуковая система
    ├── Prefabs/
    ├── Scenes/
    ├── ScriptableObjects/
    └── UI/
```

## 🚀 Начало работы

### Требования

- Unity 6 (6000.x)
- Git LFS (для больших файлов)

### Установка

```bash
git clone https://github.com/YOUR_USERNAME/TicTacToe.git
cd TicTacToe
```

Откройте проект в Unity Hub.

## 📋 Документация

- [Game Design Document](docs/GDD_TicTacToe.md)
- [Technical Design Document](docs/TDD_TicTacToe.md)

## 📄 Лицензия

MIT License
