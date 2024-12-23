using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateSectionColliders : MonoBehaviour
{
    [MenuItem("Tools/Generate Sectors Collider")]
    public static void GenerateCombinedBoxCollider()
    {
        var gameObject = Selection.activeGameObject;
        
        GenerateBoxCollider(gameObject);
        
        var sectorController = gameObject.GetComponent<SectionController>();

        sectorController.sectorOutterCollides.Clear();
    }

    private static void GenerateBoxCollider(GameObject gameObject)
    {
        Bounds bounds = GetChildRendererBounds(gameObject);
        if (!gameObject.TryGetComponent<BoxCollider>(out var boxCollider))
            boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.gameObject.transform.position = bounds.center;
        boxCollider.gameObject.transform.localScale = bounds.size;
    }

    static Bounds GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }
}
