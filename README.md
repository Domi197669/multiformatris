# Multiformatris

Un juego 3D estilo Tetris donde la gravedad cambia de dirección por nivel, aumentando la dificultad progresivamente.

## Características

- **Grid 3D** con piezas que caen desde diferentes direcciones
- **4 direcciones de gravedad**: ↓ arriba→abajo, ↑ abajo→arriba, → izq→der, ← der→izq
- **7 piezas estándar** de Tetris en 3D
- **Rotación 3D** de piezas con Matrix4x4
- **Sistema de combos** con multiplicadores
- **Efectos visuales**: partículas, screen shake, animaciones
- **Controles táctiles** para móvil
- **Controles de teclado** para PC
- **UI completa**: menú, pausa, game over, ajustes
- **High score** guardado localmente

## Controles

### Teclado
| Tecla | Acción |
|-------|--------|
| ← → ↑ ↓ | Mover pieza |
| Q / E | Rotar pieza |
| Espacio | Hard drop |
| Shift | Soft drop |
| Ctrl | Hold |
| Escape | Pausa |

### Móvil
- **Swipe**: Mover pieza
- **Tap izquierda**: Rotar X
- **Tap derecha**: Rotar Z
- **Tap centro**: Hard drop
- **Botones virtuales**: Controles alternativos

## Configuración del Proyecto

### Requisitos
- Unity 6.3 LTS
- Input System Package

### Instalación
1. Clonar el repositorio
2. Abrir con Unity 6.3
3. Abrir escena `Game.unity`
4. Presionar Play

### Configuración para móvil
1. File → Build Settings
2. Seleccionar Android/iOS
3. Configurar Player Settings
4. Build

## Estructura del Proyecto

```
Assets/Scripts/
├── Core/                    # Lógica del juego (sin dependencias Unity)
│   ├── Game/               # GameStateMachine, ComboSystem
│   ├── Gravity/            # GravityConfig
│   ├── Grid/               # GridData, GridOperations
│   ├── Levels/             # LevelConfig, LevelManager
│   └── Pieces/             # PieceDefinition, PieceBag, PieceFactory
├── Infrastructure/          # Servicios
│   ├── Audio/              # AudioManager
│   ├── Build/              # BuildManager
│   ├── Input/              # InputHandler, MobileInputHandler
│   └── Pool/               # ObjectPool
├── Presentation/            # Render y animación
│   ├── Animations/         # BlockAnimator
│   ├── VFX/                # ClearEffects, ScreenShake, ScorePopup
│   ├── CameraController.cs
│   ├── GridView.cs
│   ├── GhostPiece.cs
│   ├── PieceView.cs
│   └── WellRotator.cs
├── UI/                      # Interfaz de usuario
│   ├── MobileUIController.cs
│   ├── NextPiecePreview.cs
│   ├── ResponsiveCanvas.cs
│   ├── SettingsMenu.cs
│   └── UIManager.cs
└── GameManager.cs           # Orquestador principal
```

## Licencia

MIT License
