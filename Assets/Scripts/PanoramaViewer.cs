using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Main script for displaying 360° panoramas in VR
/// Attach this to a sphere GameObject with the panorama texture
/// </summary>
public class PanoramaViewer : MonoBehaviour
{
    [Header("Panorama Settings")]
    [Tooltip("The 360° equirectangular image to display")]
    public Texture2D panoramaTexture;
    
    [Tooltip("Scale of the panorama sphere - increase for larger spaces")]
    public float sphereScale = 10f;
    
    private Material panoramaMaterial;
    private MeshRenderer meshRenderer;

    void Start()
    {
        SetupPanoramaSphere();
        ApplyPanoramaTexture();
    }

    void SetupPanoramaSphere()
    {
        // Get or add required components
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Create sphere mesh (if not already assigned)
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = CreateSphereMesh();
        }

        // CRITICAL: Invert the sphere by using negative X scale
        // This flips the normals inward so you can see the texture from inside
        transform.localScale = new Vector3(-sphereScale, sphereScale, sphereScale);

        // Create and setup material
        panoramaMaterial = new Material(Shader.Find("Unlit/Texture"));
        panoramaMaterial.SetFloat("_Cull", 1); // Cull front faces (render inside)
        meshRenderer.material = panoramaMaterial;
    }

    void ApplyPanoramaTexture()
    {
        if (panoramaTexture != null && panoramaMaterial != null)
        {
            panoramaMaterial.mainTexture = panoramaTexture;
            Debug.Log($"Applied panorama texture: {panoramaTexture.name}");
        }
        else
        {
            Debug.LogWarning("Panorama texture or material is missing!");
        }
    }

    Mesh CreateSphereMesh()
    {
        // Unity's built-in primitive is easier, but this shows the concept
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempSphere);
        return sphereMesh;
    }

    // Allow changing panorama at runtime
    public void ChangePanorama(Texture2D newTexture)
    {
        panoramaTexture = newTexture;
        ApplyPanoramaTexture();
    }
}
