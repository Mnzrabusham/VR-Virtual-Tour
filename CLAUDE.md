# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity-based VR 360° tour application for VIVE XR Elite that converts iGUIDE building scans into immersive virtual reality experiences. The project displays equirectangular panoramic images inside inverted sphere meshes, enabling users to teleport between rooms using interactive hotspots.

**Target Platform:** Android (VIVE XR Elite)
**Unity Version:** 2022.3 LTS or newer
**XR Framework:** OpenXR with XR Interaction Toolkit

## Core Architecture

### Three-Component System

1. **PanoramaViewer.cs** - Sphere rendering for 360° images
   - Creates/configures inverted sphere mesh (-X scale to flip normals inward)
   - Applies Unlit/Texture shader with equirectangular panorama
   - Handles runtime panorama switching

2. **PanoramaTeleportPoint.cs** - Interactive navigation hotspots
   - XR ray interaction system for VR controller targeting
   - Visual feedback states (normal/hover/click) with color transitions
   - Teleports XR Origin to target panorama position on selection
   - Uses XRSimpleInteractable for XR Interaction Toolkit integration

3. **VRTourManager.cs** - Multi-room tour orchestration
   - Manages PanoramaRoom data structures (position, texture, connections)
   - Programmatically generates panorama spheres and hotspot network
   - Maintains references between rooms for navigation graph

### Scene Hierarchy Pattern

```
XR Interaction Manager (required singleton)
XR Origin (VR) - Camera rig with Locomotion System
├── Camera Offset
│   ├── Main Camera
│   ├── Left Controller (XR Ray Interactor)
│   └── Right Controller (XR Ray Interactor)
│
Panorama Spheres (positioned spatially apart, e.g., 20 units)
├── Panorama_RoomName
│   └── Hotspot_ToOtherRoom (child, positioned relative to doorways)
```

### Critical Technical Details

**Panorama Sphere Configuration:**
- Scale: `(-10, 10, 10)` - Negative X flips normals to render interior
- Shader: `Unlit/Texture` - No lighting calculations needed
- No collider - Users don't interact with sphere itself
- Position rooms 20+ units apart to prevent overlap

**Hotspot Configuration:**
- Scale: `0.3` typically (must be visible but not obtrusive)
- Requires: SphereCollider + XRSimpleInteractable + PanoramaTeleportPoint
- Position relative to where doorways/passages appear in panorama
- Material: Standard shader with transparency for visual feedback

**Image Requirements:**
- Format: Equirectangular (2:1 aspect ratio)
- Resolution: 4096×2048 (high-end) or 2048×1024 (mobile-optimized)
- Texture Import: Max Size 4096, Wrap Mode Clamp, Filter Mode Trilinear

## Development Commands

### Unity Editor Testing
No build required for development - use Unity's Play mode with XR Device Simulator:
- Import XR Device Simulator from XR Interaction Toolkit samples
- Add XR Device Simulator prefab to scene
- Play mode controls: Right-click drag (look), WASD (move), Space (trigger)

### Building for Device
```bash
# Unity Build Settings
File → Build Settings
Platform: Android
Switch Platform (one-time reimport)

# Player Settings
Other Settings:
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64
  - Minimum API Level: Android 10.0 (API 29)

XR Plugin Management (Android tab):
  - Enable OpenXR
  - OpenXR Feature Groups: HTC Vive Focus 3 & XR Elite

# Build and Deploy
File → Build and Run (with VIVE XR Elite connected via USB-C)
```

### Required Unity Packages
Install via Window → Package Manager:
- `com.unity.xr.interaction.toolkit` - VR interaction system
- Enable XR Plugin Management in Project Settings → XR Plugin Management
- OpenXR (check in XR Plugin Management settings)
- Interaction Profiles: HTC Vive Controller + HTC Vive Focus 3 Controller

## Key Design Patterns

### Teleportation Flow
1. XR Ray Interactor casts ray from controller
2. Ray hits hotspot collider → triggers hover event
3. PanoramaTeleportPoint changes material color (visual feedback)
4. Controller trigger → select event → Teleport() method
5. XR Origin position = target panorama position

### Material State Management
PanoramaTeleportPoint manages three color states:
- `normalColor` - Blue semi-transparent (idle)
- `hoverColor` - Green more opaque (targeted)
- `clickColor` - Red fully opaque (activated, brief flash)

Programmatically sets Standard shader to Transparent mode via MaterialPropertyBlock.

### Room Connection Graph
VRTourManager uses serialized data structures:
- `PanoramaRoom` - Contains GameObject, Texture2D, position, List<ConnectionPoint>
- `ConnectionPoint` - Stores targetRoomName, hotspotPosition (relative), hotspotObject
- Supports runtime room addition via `AddRoom()` and `AddConnection()` methods

## Important Constraints

**Performance Targets:**
- VIVE XR Elite: 90 FPS minimum
- Limit active panoramas to 3-5 simultaneous
- Use JPG compression (80-85% quality) for smaller file sizes
- Disable shadows entirely (not needed for panoramas)

**Common Pitfalls:**
- Forgetting negative X scale on sphere → user sees outside (black)
- Missing XR Interaction Manager in scene → hotspots non-interactive
- Panorama positions too close → spheres overlap/z-fighting
- Hotspot missing XRSimpleInteractable → not detectable by ray interactor
- Texture Wrap Mode not set to Clamp → visible seam on panorama

## File Structure

```
Assets/
├── Panoramas/          - Equirectangular 360° images (JPG/PNG)
├── Scripts/            - Three core C# scripts
│   ├── PanoramaViewer.cs
│   ├── PanoramaTeleportPoint.cs
│   └── VRTourManager.cs
├── Scenes/             - VRTour.unity scene
├── Materials/          - Runtime-generated materials (Unlit/Texture)
└── Prefabs/            - Reusable panorama/hotspot prefabs (optional)
```

## Extending Functionality

When adding features, maintain these principles:
- **Performance:** Every frame counts on mobile VR - profile before adding
- **VR Comfort:** Avoid artificial motion, instant teleportation only
- **XR Interaction Toolkit:** Use built-in components (XRGrabInteractable, etc.)
- **Spatial Audio:** Use AudioSource with spatial blend for immersion

Common enhancement patterns:
- UI Information Panels: WorldSpace Canvas positioned in panorama
- Audio Narration: AudioSource on panorama spheres, triggered on entry
- Minimap: Orthographic camera rendering to RenderTexture on UI overlay
- Measurement Tools: LineRenderer with distance calculation between points
