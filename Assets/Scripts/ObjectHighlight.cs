using System.Collections.Generic;
using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    [SerializeField] Material highlightMaterial;
    [SerializeField] bool includeInactiveChildren = true;

    private readonly List<Renderer> highlightRenderers = new();

    private void Awake()
    {
        CreateHighlightRenderers();
        HideHighlight();
    }

    private void CreateHighlightRenderers()
    {
        Renderer[] sourceRenderers =
            GetComponentsInChildren<Renderer>(includeInactiveChildren);

        foreach (Renderer sourceRenderer in sourceRenderers)
        {
            Renderer overlay = CreateOverlayForRenderer(sourceRenderer);

            if (overlay != null)
                highlightRenderers.Add(overlay);
        }
    }

    private Renderer CreateOverlayForRenderer(Renderer sourceRenderer)
    {
        GameObject overlayObject =
            new GameObject(sourceRenderer.gameObject.name + "_Highlight");

        overlayObject.transform.SetParent(sourceRenderer.transform, false);
        overlayObject.layer = sourceRenderer.gameObject.layer;

        Renderer overlayRenderer = null;

        if (sourceRenderer is SkinnedMeshRenderer sourceSkinned)
        {
            SkinnedMeshRenderer overlaySkinned =
                overlayObject.AddComponent<SkinnedMeshRenderer>();

            overlaySkinned.sharedMesh = sourceSkinned.sharedMesh;
            overlaySkinned.bones = sourceSkinned.bones;
            overlaySkinned.rootBone = sourceSkinned.rootBone;
            overlaySkinned.localBounds = sourceSkinned.localBounds;

            overlayRenderer = overlaySkinned;
        }
        else if (sourceRenderer is MeshRenderer)
        {
            MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();

            if (sourceFilter == null || sourceFilter.sharedMesh == null)
                return null;

            MeshFilter overlayFilter =
                overlayObject.AddComponent<MeshFilter>();

            overlayFilter.sharedMesh = sourceFilter.sharedMesh;

            overlayRenderer =
                overlayObject.AddComponent<MeshRenderer>();
        }

        if (overlayRenderer == null)
        {
            Destroy(overlayObject);
            return null;
        }

        Material[] materials =
            new Material[sourceRenderer.sharedMaterials.Length];

        for (int i = 0; i < materials.Length; i++)
            materials[i] = highlightMaterial;

        overlayRenderer.sharedMaterials = materials;

        overlayRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        overlayRenderer.receiveShadows = false;

        return overlayRenderer;
    }

    public void ShowHighlight()
    {
        foreach (Renderer renderer in highlightRenderers)
            renderer.enabled = true;
    }

    public void HideHighlight()
    {
        foreach (Renderer renderer in highlightRenderers)
            renderer.enabled = false;
    }
}