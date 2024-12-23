using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class GenerateSectionColliders : MonoBehaviour
{
    [MenuItem("Tools/Generate Sectors Collider")]
    public static void GenerateCombinedBoxCollider()
    {
        var gameObject = Selection.activeGameObject;
        var boxCollider = GenerateBoxCollider(gameObject);

        var sectorController = gameObject.GetComponent<SectionController>();
        var excluyeLayer = LayerMask.GetMask("CameraColl", "PathPoint", "PathArrow");

        sectorController.sectorOutterCollides = gameObject.GetComponentsInChildren<Collider>().Where(c => c.enabled && c.GetInstanceID() != boxCollider.GetInstanceID() && (c.gameObject.layer & excluyeLayer) == 0).ToArray();
    }

    private static BoxCollider GenerateBoxCollider(GameObject gameObject)
    {
        if (!gameObject.TryGetComponent<BoxCollider>(out var boxCollider))
            boxCollider = gameObject.AddComponent<BoxCollider>();
        Bounds? combinedBounds = GetChildRendererBounds(gameObject);

        boxCollider.isTrigger = true;
        if (!combinedBounds.HasValue)
        {
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;
        }
        else
        {
            boxCollider.center = gameObject.transform.InverseTransformPoint(combinedBounds.Value.center);
            boxCollider.size = gameObject.transform.InverseTransformVector(combinedBounds.Value.size);
        }

        return boxCollider;
    }

    static Bounds? GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
            return null;

        Bounds combinedBounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers.Skip(1))
        {
            combinedBounds.Encapsulate(renderer.bounds);
            //Debug.Log(combinedBounds, renderer.gameObject);
        }

        return combinedBounds;
    }
}
