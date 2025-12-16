using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages fade-in/fade-out effects for VR comfort during teleportation
/// Prevents cybersickness by fading to black during movement
/// </summary>
public class VRFadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Duration of fade out to black (seconds)")]
    public float fadeOutDuration = 0.3f;
    
    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 0.4f;
    
    [Tooltip("Color to fade to (usually black for VR comfort)")]
    public Color fadeColor = Color.black;
    
    [Header("References")]
    [Tooltip("Canvas that contains the fade overlay (auto-created if not assigned)")]
    public Canvas fadeCanvas;
    
    [Tooltip("Image component for the fade overlay (auto-created if not assigned)")]
    public Image fadeImage;
    
    private CanvasGroup canvasGroup;
    private bool isFading = false;
    
    void Awake()
    {
        SetupFadeCanvas();
    }
    
    void SetupFadeCanvas()
    {
        // Check if we need to create the fade canvas
        if (fadeCanvas == null)
        {
            Debug.Log("Creating VR Fade Canvas...");
            
            // Create canvas GameObject
            GameObject canvasObj = new GameObject("VR Fade Canvas");
            canvasObj.transform.SetParent(transform);
            
            // Add Canvas component
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // Render on top of everything
            
            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster (required for UI)
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("✓ Created VR Fade Canvas");
        }
        
        // Check if we need to create the fade image
        if (fadeImage == null)
        {
            Debug.Log("Creating Fade Image...");
            
            // Create image GameObject
            GameObject imageObj = new GameObject("Fade Overlay");
            imageObj.transform.SetParent(fadeCanvas.transform, false);
            
            // Add and configure Image component
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            
            // Make it fill the entire screen
            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            Debug.Log("✓ Created Fade Overlay Image");
        }
        
        // Add CanvasGroup for alpha control
        canvasGroup = fadeCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = fadeCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Start fully transparent (no fade)
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false; // Don't block interaction when transparent
        
        Debug.Log("✓ VR Fade Manager initialized");
    }
    
    /// <summary>
    /// Fades to black (fade out)
    /// </summary>
    public IEnumerator FadeOut()
    {
        isFading = true;
        canvasGroup.blocksRaycasts = true; // Block interaction during fade
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        Debug.Log($"Fading OUT over {fadeOutDuration} seconds...");
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f; // Ensure fully black
        Debug.Log("✓ Fade OUT complete");
        
        isFading = false;
    }
    
    /// <summary>
    /// Fades from black (fade in)
    /// </summary>
    public IEnumerator FadeIn()
    {
        isFading = true;
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        Debug.Log($"Fading IN over {fadeInDuration} seconds...");
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f; // Ensure fully transparent
        canvasGroup.blocksRaycasts = false; // Re-enable interaction
        Debug.Log("✓ Fade IN complete");
        
        isFading = false;
    }
    
    /// <summary>
    /// Full fade out -> action -> fade in sequence
    /// </summary>
    public IEnumerator FadeOutAndIn(System.Action actionDuringBlackout = null)
    {
        // Fade to black
        yield return StartCoroutine(FadeOut());
        
        // Perform action while screen is black (e.g., teleport)
        actionDuringBlackout?.Invoke();
        
        // Small delay while black (helps with VR comfort)
        yield return new WaitForSeconds(0.1f);
        
        // Fade back in
        yield return StartCoroutine(FadeIn());
    }
    
    public bool IsFading => isFading;
}
