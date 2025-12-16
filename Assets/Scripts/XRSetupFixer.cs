using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Automatically adds XR Ray Interactors to controllers if they don't have them
/// This is needed for hotspot interaction to work
/// Attach this to any GameObject and it will run on Start
/// </summary>
public class XRSetupFixer : MonoBehaviour
{
    [Header("Auto-Find Controllers")]
    public bool autoFindControllers = true;

    [Header("Manual Assignment (optional)")]
    public GameObject leftController;
    public GameObject rightController;

    [Header("Ray Visual Settings")]
    public Material rayMaterial;
    public float rayWidth = 0.02f;
    public Color validRayColor = Color.white;
    public Color invalidRayColor = Color.red;

void Start()
    {
        Debug.Log("=== XR INTERACTION FIXER STARTING ===");

        // Find controllers if not assigned
        if (autoFindControllers || leftController == null || rightController == null)
        {
            FindControllers();
        }

        // Fix existing ray interactors instead of adding new ones
        if (leftController != null)
        {
            FixControllerInteraction(leftController, "Left");
        }
        else
        {
            Debug.LogError("❌ Left Controller not found!");
        }

        if (rightController != null)
        {
            FixControllerInteraction(rightController, "Right");
        }
        else
        {
            Debug.LogError("❌ Right Controller not found!");
        }

        Debug.Log("=== XR INTERACTION FIXER COMPLETE ===");
    }

    void FindControllers()
    {
        // Search for common controller names
        string[] leftNames = { "Left Controller", "LeftHand Controller", "LeftHandController" };
        string[] rightNames = { "Right Controller", "RightHand Controller", "RightHandController" };

        foreach (string name in leftNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                leftController = found;
                Debug.Log($"Found left controller: {name}");
                break;
            }
        }

        foreach (string name in rightNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                rightController = found;
                Debug.Log($"Found right controller: {name}");
                break;
            }
        }
    }

void FixControllerInteraction(GameObject controller, string side)
    {
        Debug.Log($"\n━━━ Fixing {side} Controller Interaction ━━━");

        // Remove any extra XRRayInteractor we might have added previously
        XRRayInteractor extraRay = controller.GetComponent<XRRayInteractor>();
        if (extraRay != null)
        {
            Debug.Log($"Removing extra XRRayInteractor from {side} controller");
            Destroy(extraRay);
        }

        // Look for the existing Near-Far Interactor (this is the correct interactor)
        Transform nearFarTransform = controller.transform.Find("Near-Far Interactor");
        
        if (nearFarTransform == null)
        {
            Debug.LogError($"❌ Near-Far Interactor not found on {side} controller!");
            return;
        }

        // Get the NearFarInteractor component
        var nearFarInteractor = nearFarTransform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>();
        
        if (nearFarInteractor == null)
        {
            Debug.LogError($"❌ NearFarInteractor component not found on {side} Near-Far Interactor!");
            return;
        }

        // Enable and configure the Near-Far Interactor
        ConfigureNearFarInteractor(nearFarInteractor, side);
    }

void ConfigureNearFarInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor interactor, string side)
    {
        // Enable the interactor
        interactor.enabled = true;

        // CRITICAL: Set interaction layers to match hotspots
        interactor.interactionLayers = InteractionLayerMask.GetMask("Default");

        // Configure selection behavior
        interactor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
        interactor.keepSelectedTargetValid = true;
        interactor.allowHoveredActivate = false; // IMPORTANT: Change to false - don't auto-select on hover
        interactor.allowHover = true;
        interactor.allowSelect = true;

        // Make sure far casting is enabled (this is what creates the ray)
        interactor.enableFarCasting = true;
        interactor.enableUIInteraction = true;

        Debug.Log($"✓✓✓ Configured {side} Near-Far Interactor ✓✓✓");
        Debug.Log($"  - Enabled: {interactor.enabled}");
        Debug.Log($"  - Interaction Layers: {interactor.interactionLayers.value}");
        Debug.Log($"  - Allow Hover: {interactor.allowHover}");
        Debug.Log($"  - Allow Select: {interactor.allowSelect}");
        Debug.Log($"  - Far Casting: {interactor.enableFarCasting}");
        Debug.Log($"  - Allow Hovered Activate: {interactor.allowHoveredActivate}");
    }





    Material CreateDefaultRayMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = validRayColor;
        return mat;
    }

    // Debug info
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        string info = "XR Setup Fixer:\n";
        info += leftController != null ? "✓ Left Controller Found\n" : "✗ Left Controller Missing\n";
        info += rightController != null ? "✓ Right Controller Found\n" : "✗ Right Controller Missing\n";

        if (leftController != null)
        {
            info += leftController.GetComponent<XRRayInteractor>() != null ? "✓ Left Ray Active\n" : "✗ Left Ray Missing\n";
        }
        if (rightController != null)
        {
            info += rightController.GetComponent<XRRayInteractor>() != null ? "✓ Right Ray Active\n" : "✗ Right Ray Missing\n";
        }

        GUI.Label(new Rect(10, 10, 300, 200), info, style);
    }
}
