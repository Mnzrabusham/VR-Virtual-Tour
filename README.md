# VR Virtual Tour System

A Unity-based VR application for creating immersive Virtual Reality tours.

## Features

### 🎮 Immersive VR Experience
- Native VR controller support with intuitive ray-based interaction
- Smooth teleportation between multiple locations
- VR comfort features to prevent motion sickness

### ✨ Teleportation System
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
