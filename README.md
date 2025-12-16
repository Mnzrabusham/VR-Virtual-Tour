# VR 360° Panorama Tour System

A professional Unity-based VR application for creating immersive 360° panorama tours with smooth teleportation between locations.

## Features

### 🎮 Immersive VR Experience
- Full 360° panoramic environments rendered on optimized spheres
- Native VR controller support with intuitive ray-based interaction
- Smooth teleportation between multiple locations
- VR comfort features to prevent motion sickness

### ✨ Professional Teleportation System
- **Visual Hotspots** - Interactive floor markers for navigation points
- **Smooth Fade Transitions** - Customizable fade-in/fade-out effects during teleportation
- **Anti-Nausea Design** - Screen fades to black during movement to reduce VR discomfort
- **Hover Feedback** - Visual indicators when pointing at teleport destinations
- **Cooldown System** - Prevents accidental rapid teleportation

### 🎯 Controller Ray Interaction
- Curved ray visualization from VR controllers
- Hover detection with visual feedback
- Selection triggering via VR controller buttons
- Automatic ray refresh system after teleportation

### 🔧 Technical Highlights
- Built on Unity XR Interaction Toolkit 3.3.0
- Optimized for VR performance
- Modular component-based architecture
- Extensible hotspot system for easy tour expansion

## System Requirements

### Unity Version
- Unity 2022.3 LTS or newer

### Required Unity Packages
- XR Interaction Toolkit (3.3.0 or newer)
- XR Plugin Management
- Input System

### VR Hardware
- Compatible with OpenXR-supported VR headsets
- Tested with desktop VR simulation
- Controller input required

## Project Structure

```
Assets/
├── Scenes/
│   └── SampleScene.unity         # Main tour scene
├── Scripts/
│   ├── PanoramaViewer.cs         # 360° panorama rendering
│   ├── PanoramaTeleportPoint.cs  # Teleportation system
│   ├── VRFadeManager.cs          # Fade transition effects
│   ├── XRSetupFixer.cs           # XR controller configuration
│   └── ...
├── Textures/
│   └── Panoramas/                # 360° panorama images
└── Materials/
    └── Panorama Materials/       # Sphere rendering materials
```

## Key Components

### VRFadeManager
Handles smooth fade transitions during teleportation:
- Configurable fade-in/fade-out duration
- Black screen overlay for VR comfort
- Automatic canvas creation and management

### PanoramaTeleportPoint
Interactive teleportation hotspots:
- Visual hover feedback
- Configurable target destinations
- Integrated fade effect support
- Cooldown system

### XR Controller System
Professional VR controller integration:
- Ray-based interaction
- Automatic controller detection
- Input system configuration

## Configuration

Key settings can be adjusted in the Unity Inspector:

**VR Fade Manager:**
- Fade Out Duration (default: 0.3s)
- Fade In Duration (default: 0.4s)
- Fade Color (default: black)

**Teleport Hotspots:**
- Target destination
- Hotspot colors (normal, hover, click)
- Cooldown duration (default: 2s)

## Development Notes

This project uses custom solutions for:
- VR controller ray interaction optimization
- Panorama sphere rendering at scale
- Smooth teleportation with fade effects
- Multi-room tour architecture

## License

All rights reserved. This project is proprietary and confidential.

## Author

Created for commercial VR tour applications.

---

*For support or licensing inquiries, please contact the project owner.*
