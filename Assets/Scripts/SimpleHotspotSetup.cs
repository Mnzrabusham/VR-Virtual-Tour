using UnityEngine;

/// <summary>
/// Simple script to add floor-based teleportation hotspots to existing panorama spheres
/// Attach this to an empty GameObject and assign your existing panorama GameObjects
/// </summary>
public class SimpleHotspotSetup : MonoBehaviour
{
    [Header("Existing Panorama Spheres")]
    [Tooltip("Drag your existing panorama sphere GameObjects here")]
    public GameObject room1Sphere;
    public GameObject room2Sphere;
    public GameObject room3Sphere;

    [Header("XR Setup")]
    [Tooltip("Reference to your XR Origin/XR Rig in the scene")]
    public GameObject xrRig;

    [Header("Hotspot Settings")]
    [Tooltip("Scale of teleport hotspot discs")]
    public float hotspotScale = 0.5f;

    [Tooltip("Height of hotspots relative to room center (negative = below)")]
    public float hotspotHeightOffset = -1.5f;

    [Tooltip("Distance from center where hotspots are placed")]
    public float hotspotDistance = 8f;

    [Tooltip("Add hotspots when you press this key (for testing)")]
    public KeyCode setupKey = KeyCode.H;

    private bool hotspotsCreated = false;

    void Start()
    {
        // Find XR Rig if not assigned
        if (xrRig == null)
        {
            xrRig = GameObject.Find("XR Origin");
            if (xrRig == null)
            {
                xrRig = GameObject.Find("XR Rig");
            }
            if (xrRig == null)
            {
                Debug.LogError("XR Rig not found! Please assign it in the inspector.");
                return;
            }
        }

        // Automatically create hotspots on start
        CreateHotspots();
    }

    void Update()
    {
        // Allow manual triggering with H key
        if (Input.GetKeyDown(setupKey) && !hotspotsCreated)
        {
            CreateHotspots();
        }
    }

    public void CreateHotspots()
    {
        if (room1Sphere == null || room2Sphere == null || room3Sphere == null)
        {
            Debug.LogError("Please assign all three room sphere GameObjects in the inspector!");
            return;
        }

        if (hotspotsCreated)
        {
            Debug.LogWarning("Hotspots already created!");
            return;
        }

        // Create bidirectional connections
        // Room 1 <-> Room 2
        CreateFloorHotspot(room1Sphere, room2Sphere, "ToRoom2", new Vector3(hotspotDistance, hotspotHeightOffset, 0));
        CreateFloorHotspot(room2Sphere, room1Sphere, "ToRoom1", new Vector3(-hotspotDistance, hotspotHeightOffset, 0));

        // Room 2 <-> Room 3
        CreateFloorHotspot(room2Sphere, room3Sphere, "ToRoom3", new Vector3(hotspotDistance, hotspotHeightOffset, 0));
        CreateFloorHotspot(room3Sphere, room2Sphere, "ToRoom2", new Vector3(-hotspotDistance, hotspotHeightOffset, 0));

        hotspotsCreated = true;
        Debug.Log("Floor hotspots created! Look for blue circles on the floor and point your controller at them.");
    }

GameObject CreateFloorHotspot(GameObject fromRoom, GameObject toRoom, string hotspotName, Vector3 relativePosition)
    {
        Debug.Log($"\n━━━ Creating Floor Hotspot: {hotspotName} ━━━");

        // Create a flat cylinder (disc) for the floor hotspot
        GameObject hotspot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hotspot.name = $"Hotspot_{hotspotName}";

        // Make it a thin, LARGER disc for better visibility
        hotspot.transform.localScale = new Vector3(hotspotScale * 2f, 0.05f, hotspotScale * 2f);
        Debug.Log($"Hotspot scale: {hotspot.transform.localScale}");

        // Position it relative to the from-room
        hotspot.transform.position = fromRoom.transform.position + relativePosition;
        hotspot.transform.SetParent(fromRoom.transform);
        Debug.Log($"Hotspot position: {hotspot.transform.position}");
        Debug.Log($"From room position: {fromRoom.transform.position}");
        Debug.Log($"Relative position: {relativePosition}");

        // CRITICAL: Remove the default capsule collider BEFORE adding teleport script
        Collider defaultCollider = hotspot.GetComponent<Collider>();
        if (defaultCollider != null)
        {
            Debug.Log($"Destroying default {defaultCollider.GetType().Name}");
            DestroyImmediate(defaultCollider); // Use DestroyImmediate in setup
        }

        // Add the teleport script (it will add the correct collider)
        PanoramaTeleportPoint teleport = hotspot.AddComponent<PanoramaTeleportPoint>();
        teleport.targetPanorama = toRoom;
        teleport.xrRig = xrRig;
        teleport.useFlatDisc = true;
        
        // BRIGHTER, MORE VISIBLE colors
        teleport.normalColor = new Color(0.4f, 0.7f, 1f, 0.8f); // Brighter blue, more opaque
        teleport.hoverColor = new Color(0.5f, 1f, 0.3f, 1f);    // Bright green, fully opaque
        teleport.clickColor = new Color(1f, 0.3f, 0.3f, 1f);    // Red solid

        Debug.Log($"✓✓✓ Created floor hotspot: {hotspotName}");
        Debug.Log($"  From: {fromRoom.name}");
        Debug.Log($"  To: {toRoom.name}");
        Debug.Log($"  Position: {hotspot.transform.position}");
        Debug.Log($"  Active: {hotspot.activeInHierarchy}");
        Debug.Log($"  Layer: {LayerMask.LayerToName(hotspot.layer)}");

        return hotspot;
    }

    // Visualize hotspot positions in the editor
    void OnDrawGizmos()
    {
        if (room1Sphere != null && room2Sphere != null && room3Sphere != null)
        {
            // Draw hotspot positions
            Gizmos.color = Color.cyan;

            // Room 1 -> Room 2 hotspot
            Vector3 r1ToR2 = room1Sphere.transform.position + new Vector3(hotspotDistance, hotspotHeightOffset, 0);
            Gizmos.DrawWireSphere(r1ToR2, hotspotScale * 0.5f);

            // Room 2 -> Room 1 hotspot
            Vector3 r2ToR1 = room2Sphere.transform.position + new Vector3(-hotspotDistance, hotspotHeightOffset, 0);
            Gizmos.DrawWireSphere(r2ToR1, hotspotScale * 0.5f);

            // Room 2 -> Room 3 hotspot
            Vector3 r2ToR3 = room2Sphere.transform.position + new Vector3(hotspotDistance, hotspotHeightOffset, 0);
            Gizmos.DrawWireSphere(r2ToR3, hotspotScale * 0.5f);

            // Room 3 -> Room 2 hotspot
            Vector3 r3ToR2 = room3Sphere.transform.position + new Vector3(-hotspotDistance, hotspotHeightOffset, 0);
            Gizmos.DrawWireSphere(r3ToR2, hotspotScale * 0.5f);
        }
    }
}
