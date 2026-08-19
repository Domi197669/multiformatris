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

## Descargar y Jugar

### Android (APK)
1. Ve a [Releases](https://github.com/Domi197669/multiformatris/releases)
2. Descarga el archivo `.apk` de la última release
3. En tu móvil, habilita "Fuentes desconocidas" en Ajustes
4. Abre el archivo APK para instalar
5. ¡Juega!

### iOS
Los builds de iOS requieren Xcode para compilar. Descarga el artifact de iOS desde Actions y ábrelo en Xcode.

### Compilar desde el código fuente
1. Clona el repositorio: `git clone https://github.com/Domi197669/multiformatris.git`
2. Abre la carpeta con Unity 6.3 LTS
3. Configura las piezas (ver abajo)
4. `Build → Build Android APK` o usa `BuildManager` en el menú

## Configuración del Proyecto

### Requisitos
- Unity 6.3 LTS
- Input System Package

### Instalación
1. Clonar el repositorio
2. Abrir con Unity 6.3
3. Abrir escena `Game.unity`
4. Presionar Play

### Configurar las 7 piezas
1. Click derecho en `Assets/ScriptableObjects/PieceDefinitions/`
2. `Create → Multiformatris → Piece Definition`
3. Repetir 7 veces con estos nombres y formas:

| Pieza | Nombre | Color | Celdas (x,y,z) |
|-------|--------|-------|-----------------|
| I | I | Cyan | (0,0,0),(1,0,0),(2,0,0),(3,0,0) |
| J | J | Azul | (0,0,0),(0,1,0),(1,1,0),(2,1,0) |
| L | L | Naranja | (2,0,0),(0,1,0),(1,1,0),(2,1,0) |
| O | O | Amarillo | (0,0,0),(1,0,0),(0,1,0),(1,1,0) |
| S | S | Verde | (1,0,0),(2,0,0),(0,1,0),(1,1,0) |
| T | T | Magenta | (1,0,0),(0,1,0),(1,1,0),(2,1,0) |
| Z | Z | Rojo | (0,0,0),(1,0,0),(1,1,0),(2,1,0) |

4. Asignar todas las piezas al `PieceBag` en el GameManager

### Configurar PieceBag
1. Crear `Create → Multiformatris → Piece Bag`
2. Asignar las 7 piezas al array `AllPieces`

### Configurar GravityConfig
1. Crear `Create → Multiformatris → Gravity System`
2. Configurar:
   - Base Speed: 1.0
   - Speed Increment: 0.15
   - Gravity Sequence: Down, Up, Right, Left

### Configurar GridConfig
1. Crear `Create → Multiformatris → Grid Config`
2. Dimensiones: Width=5, Height=10, Depth=5

### Configurar GameManager
En la escena Game, al GameManager asignar:
- GridConfig
- GravityConfig
- PieceBag
- GridView (vacío con el script GridView)
- PieceView (vacío con el script PieceView)
- CameraController (en la cámara principal)
- MobileInputHandler (para controles táctiles)
- WellRotator (en el objeto del well)
- ClearEffects
- ScreenShake (en la cámara)
- ScorePopup (en un TextMeshPro)
- GhostPiece
- ComboSystem

### Configurar para móvil
1. File → Build Settings
2. Seleccionar Android/iOS
3. Configurar Player Settings
4. Build

## GitHub Actions (Build Automático)

El proyecto incluye workflows para compilar automáticamente:

### Activar builds automáticos
1. Ve a tu repo → Settings → Secrets and variables → Actions
2. Crea un secret llamado `UNITY_LICENSE`
3. El valor debe ser tu licencia de Unity (ver `setup-unity-license.sh`)

### Cómo obtener tu licencia de Unity
```bash
chmod +x setup-unity-license.sh
./setup-unity-license.sh
```

### Crear una release con APK
```bash
git tag v1.0.0
git push origin v1.0.0
```
Esto activará el workflow y creará una release con el APK listo para descargar.

### Descargar desde GitHub
1. Ve a [Actions](https://github.com/Domi197669/multiformatris/actions)
2. Click en el workflow "Build Android APK"
3. En "Artifacts" descarga `Multiformatris-APK`

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
