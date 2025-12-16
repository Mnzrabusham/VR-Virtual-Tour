using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Teleportation hotspot for moving between different panorama views
/// Place this on a sphere/cube GameObject positioned where users should look to teleport
/// </summary>
public class PanoramaTeleportPoint : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("The target panorama sphere to teleport to")]
    public GameObject targetPanorama;

    [Tooltip("The XR Rig (camera) to teleport")]
    public GameObject xrRig;

    [Header("Teleportation System")]
    [Tooltip("Reference to Unity's TeleportationProvider (auto-found if not assigned)")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    [Header("VR Comfort")]
    [Tooltip("Reference to VR Fade Manager (auto-found if not assigned)")]
    public VRFadeManager fadeManager;



    [Header("Hotspot Shape")]
    [Tooltip("Use flat disc (cylinder) for floor-based teleport circles")]
    public bool useFlatDisc = true;

    [Tooltip("Visual indicator (this GameObject's material will highlight on hover)")]
    public Color normalColor = new Color(0.2f, 0.5f, 1f, 0.5f);
    public Color hoverColor = new Color(0.5f, 1f, 0.2f, 0.8f);
    public Color clickColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Tooltip("Distance offset from panorama center")]
    public float teleportOffsetY = 0f;
    
    private Material hotspotMaterial;
    private MeshRenderer meshRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    
    // Teleport cooldown to prevent multiple rapid teleports
    private bool canTeleport = true;
    private float teleportCooldown = 2f; // 2 second cooldown to prevent stuck ray from triggering

    
void Start()
    {
        SetupHotspot();
        SetupXRInteraction();
        
        // Auto-find TeleportationProvider if not assigned
        if (teleportationProvider == null)
        {
            teleportationProvider = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
            if (teleportationProvider != null)
            {
                Debug.Log($"✓ Found TeleportationProvider: {teleportationProvider.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ TeleportationProvider not found! Will use direct transform.position instead.");
            }
        }
        
        // Auto-find VRFadeManager if not assigned
        if (fadeManager == null)
        {
            fadeManager = FindObjectOfType<VRFadeManager>();
            if (fadeManager != null)
            {
                Debug.Log($"✓ Found VRFadeManager: {fadeManager.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ VRFadeManager not found! Teleportation will not have fade effect.");
            }
        }
    }

void SetupHotspot()
    {
        Debug.Log($"\n━━━ Setting up hotspot: {gameObject.name} ━━━");

        // Get mesh renderer (should already exist from primitive)
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer not found on hotspot! Make sure this is added to a primitive object.");
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Ensure mesh filter exists
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogWarning("MeshFilter not found, adding one");
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Create material for hotspot with transparency
        hotspotMaterial = new Material(Shader.Find("Standard"));
        hotspotMaterial.color = normalColor;

        // Set to transparent rendering mode
        hotspotMaterial.SetFloat("_Mode", 3); // Transparent mode
        hotspotMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        hotspotMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        hotspotMaterial.SetInt("_ZWrite", 0);
        hotspotMaterial.DisableKeyword("_ALPHATEST_ON");
        hotspotMaterial.EnableKeyword("_ALPHABLEND_ON");
        hotspotMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        hotspotMaterial.renderQueue = 3000;

        meshRenderer.material = hotspotMaterial;
        Debug.Log($"✓ Hotspot material set up with color {normalColor}");

        // CRITICAL: Set up collider properly
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider != null)
        {
            Debug.Log($"Removing existing collider type: {existingCollider.GetType().Name}");
            Destroy(existingCollider);
        }

        // Add appropriate collider based on shape
        if (useFlatDisc)
        {
            // For flat disc, use MeshCollider with convex enabled
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true; // REQUIRED for XR interaction
            meshCollider.isTrigger = false; // Should be solid for ray interaction
            Debug.Log($"✓ Added convex MeshCollider (trigger={meshCollider.isTrigger})");
        }
        else
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.5f;
            sphereCollider.isTrigger = false;
            Debug.Log($"✓ Added SphereCollider (trigger={sphereCollider.isTrigger})");
        }

        Debug.Log($"✓ Hotspot mesh and collider setup complete");
    }

void SetupXRInteraction()
    {
        Debug.Log($"Setting up XR interaction for {gameObject.name}");

        // Add XR Simple Interactable for VR interaction
        interactable = gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            Debug.Log($"✓ Added XRSimpleInteractable component");
        }

        // Use Default layer (1) to work with Near-Far Interactor
        interactable.interactionLayers = InteractionLayerMask.GetMask("Default");

        // Configure interaction settings
        interactable.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single;

        // Subscribe to interaction events
        interactable.hoverEntered.RemoveAllListeners(); // Clear any existing
        interactable.hoverExited.RemoveAllListeners();
        interactable.selectEntered.RemoveAllListeners();

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter);

        Debug.Log($"✓✓✓ XR Interaction READY on {gameObject.name} ✓✓✓");
        Debug.Log($"  - Component: XRSimpleInteractable");
        Debug.Log($"  - Interaction Layer: {interactable.interactionLayers.value} (should be 1 for Default)");
        Debug.Log($"  - Collider: {GetComponent<Collider>()?.GetType().Name ?? "MISSING"}");
        Debug.Log($"  - Collider Convex: {(GetComponent<MeshCollider>()?.convex ?? false)}");
        Debug.Log($"  - Position: {transform.position}");
        Debug.Log($"  - Active: {gameObject.activeInHierarchy}");
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (hotspotMaterial != null)
        {
            hotspotMaterial.color = hoverColor;
            Debug.Log($"★★★ HOVERING over {gameObject.name} - Color changed to GREEN {hoverColor} ★★★");
        }
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (hotspotMaterial != null)
        {
            hotspotMaterial.color = normalColor;
            Debug.Log($"Hover exit on {gameObject.name} - Color back to normal");
        }
    }

void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"\n========================================");
        Debug.Log($"OnSelectEnter CALLED on {gameObject.name}!");
        Debug.Log($"Interactor: {args.interactorObject?.transform.name}");
        Debug.Log($"Can Teleport: {canTeleport}");
        Debug.Log($"========================================\n");
        
        // Check cooldown to prevent multiple rapid teleports
        if (!canTeleport)
        {
            Debug.Log("⚠️ Teleport on cooldown - ignoring selection");
            return;
        }
        
        if (hotspotMaterial != null)
        {
            hotspotMaterial.color = clickColor;
        }
        
        Debug.Log($"★★★ SELECTED {gameObject.name} - TELEPORTING! ★★★");
        Teleport();
    }

void Teleport()
    {
        if (targetPanorama == null || xrRig == null)
        {
            Debug.LogWarning("Target panorama or XR Rig not assigned!");
            return;
        }

        // Start cooldown immediately to prevent multiple rapid teleports
        canTeleport = false;
        Invoke(nameof(ResetCooldown), teleportCooldown);
        
        // Start the teleportation sequence with fade effect
        StartCoroutine(TeleportWithFade());
    }
    
    System.Collections.IEnumerator TeleportWithFade()
    {
        // Calculate target position
        Vector3 targetPosition = targetPanorama.transform.position;
        targetPosition.y += teleportOffsetY;
        
        Debug.Log($"Starting teleport sequence from {xrRig.transform.position} to {targetPosition}");
        
        // STEP 1: Fade to black (if fade manager available)
        if (fadeManager != null)
        {
            yield return StartCoroutine(fadeManager.FadeOut());
        }
        
        // STEP 2: Teleport while screen is black
        Debug.Log($"Teleporting XR Rig...");
        
        if (teleportationProvider != null)
        {
            var teleportRequest = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
            {
                destinationPosition = targetPosition,
                destinationRotation = xrRig.transform.rotation,
                requestTime = Time.time
            };
            
            bool success = teleportationProvider.QueueTeleportRequest(teleportRequest);
            Debug.Log($"TeleportationProvider.QueueTeleportRequest: {success}");
        }
        else
        {
            // Direct teleport as fallback
            xrRig.transform.position = targetPosition;
        }
        
        Debug.Log($"Teleported to {targetPanorama.name}");
        
        // STEP 3: Refresh rays while screen is still black
        yield return StartCoroutine(RefreshRaysAfterTeleport());
        
        // STEP 4: Fade back in
        if (fadeManager != null)
        {
            yield return StartCoroutine(fadeManager.FadeIn());
        }
        
        // STEP 5: Reset hotspot color
        ResetColor();
        
        Debug.Log("✓ Teleport sequence complete!");
    }
    
System.Collections.IEnumerator RefreshRaysAfterTeleport()
    {
        Debug.Log("Waiting for teleport to complete...");
        
        // Wait for teleport to fully complete
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Teleport complete, refreshing rays...");
        
        // Find all Near-Far Interactors AND their CurveVisualControllers
        var nearFarInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>();
        var curveVisualControllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.CurveVisualController>();
        
        // STEP 1: Disable interactors to clear state
        foreach (var interactor in nearFarInteractors)
        {
            if (interactor != null && interactor.enabled)
            {
                Debug.Log($"Disabling interactor: {interactor.transform.parent?.name}");
                interactor.enabled = false;
            }
        }
        
        // STEP 2: Also disable the CurveVisualControllers
        foreach (var curveController in curveVisualControllers)
        {
            if (curveController != null && curveController.enabled)
            {
                Debug.Log($"Disabling curve controller: {curveController.transform.parent?.name}");
                curveController.enabled = false;
            }
        }
        
        // Wait 2 frames with everything disabled
        yield return null;
        yield return null;
        
        // STEP 3: Re-enable interactors first
        foreach (var interactor in nearFarInteractors)
        {
            if (interactor != null)
            {
                Debug.Log($"Re-enabling interactor: {interactor.transform.parent?.name}");
                interactor.enabled = true;
            }
        }
        
        // Wait a frame
        yield return null;
        
        // STEP 4: Then re-enable CurveVisualControllers
        foreach (var curveController in curveVisualControllers)
        {
            if (curveController != null)
            {
                Debug.Log($"Re-enabling curve controller: {curveController.transform.parent?.name}");
                curveController.enabled = true;
            }
        }
        
        // Wait for rays to rebuild
        yield return null;
        yield return null;
        
        Debug.Log($"✓ Refresh complete - {nearFarInteractors.Length} interactors, {curveVisualControllers.Length} curve controllers");
    }
    

    


    void ResetCooldown()
    {
        canTeleport = true;
        Debug.Log("✓ Teleport cooldown reset - ready to teleport again");
    }





    void ResetColor()
    {
        hotspotMaterial.color = normalColor;
    }

    // Optional: Draw gizmo in editor to see hotspot placement
    void OnDrawGizmos()
    {
        Gizmos.color = normalColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (targetPanorama != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPanorama.transform.position);
        }
    }
}
