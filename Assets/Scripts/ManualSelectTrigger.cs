using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// WORKAROUND: Manually triggers selection on Near-Far Interactor when Space/Click is pressed
/// This bypasses the Input Action system that isn't working properly
/// </summary>
public class ManualSelectTrigger : MonoBehaviour
{
    private NearFarInteractor[] nearFarInteractors;
    private bool wasSelectingLastFrame = false;

void Start()
    {
        // Wait a frame before finding interactors to ensure they're configured
        StartCoroutine(InitializeAfterDelay());
    }

    System.Collections.IEnumerator InitializeAfterDelay()
    {
        // Wait for end of frame to ensure XRSetupFixer has run
        yield return new WaitForEndOfFrame();
        
        // Find all Near-Far Interactors
        nearFarInteractors = FindObjectsOfType<NearFarInteractor>();
        Debug.Log($"\n★★★ ManualSelectTrigger found {nearFarInteractors.Length} Near-Far Interactors ★★★");
        
        if (nearFarInteractors.Length == 0)
        {
            Debug.LogError("\u274c ManualSelectTrigger: No Near-Far Interactors found! Make sure XRSetupFixer has run.");
        }
        else
        {
            foreach (var interactor in nearFarInteractors)
            {
                Debug.Log($"  - Found: {interactor.transform.parent.name}/{interactor.name}");
            }
        }
    }

void Update()
    {
        // Check if Space or Left Mouse is pressed THIS frame
        bool selectPressed = false;
        
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            selectPressed = true;
        }
        
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            selectPressed = true;
        }

        // CRITICAL: Only trigger on the FIRST frame of button press
        // This prevents continuous triggering while button is held
        bool selectJustPressed = selectPressed && !wasSelectingLastFrame;
        
        if (!selectJustPressed)
        {
            wasSelectingLastFrame = selectPressed;
            return; // Exit early if button wasn't just pressed
        }

        // Process each interactor ONLY on first frame of press
        foreach (var interactor in nearFarInteractors)
        {
            if (interactor == null || !interactor.enabled) continue;

            // If interactor is hovering and select is just pressed
            if (interactor.hasHover && !interactor.hasSelection)
            {
                // Manually trigger selection
                TriggerSelection(interactor);
                break; // Only trigger one selection per button press
            }
        }

        wasSelectingLastFrame = selectPressed;
    }

void TriggerSelection(NearFarInteractor interactor)
    {
        Debug.Log($"★★★ MANUAL SELECT TRIGGER on {interactor.name} ★★★");

        // Check if interactor has a valid hover target
        if (!interactor.hasHover)
        {
            Debug.LogWarning("No hover target found!");
            return;
        }

        // Get the interactables that are being hovered
        var hoveredInteractables = interactor.interactablesHovered;
        
        if (hoveredInteractables.Count > 0)
        {
            var target = hoveredInteractables[0];
            Debug.Log($"Attempting to select: {(target as Component)?.gameObject.name}");

            // Try to manually start selection
            var manager = interactor.interactionManager;
            if (manager != null && target is UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectTarget)
            {
                // Force the interaction manager to process this selection
                try
                {
                    manager.SelectEnter(interactor, selectTarget);
                    Debug.Log("✓ Selection triggered via InteractionManager!");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to trigger selection: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Hover targets list is empty!");
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.cyan;
        style.padding = new RectOffset(10, 10, 10, 10);

        string text = "MANUAL SELECT TRIGGER: ";
        
        bool selectPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            selectPressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            selectPressed = true;

        text += selectPressed ? "ACTIVE" : "Waiting...";

        GUI.Label(new Rect(Screen.width / 2 - 150, 10, 300, 30), text, style);
    }
}
