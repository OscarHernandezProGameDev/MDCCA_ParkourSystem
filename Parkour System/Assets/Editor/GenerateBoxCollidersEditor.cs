using UnityEngine;
using UnityEditor;

public class GenerateSingleBoxColliderEditor : MonoBehaviour
{
    [MenuItem("Tools/Generate Combined Box Collider")]
    public static void GenerateCombinedBoxCollider()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            AddSingleBoxCollider(obj);
        }
    }

    private static void AddSingleBoxCollider(GameObject obj)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning($"No se encontraron MeshFilters en {obj.name}.");
            return;
        }

        Bounds combinedBounds = meshFilters[0].sharedMesh.bounds;
        Matrix4x4 firstTransform = meshFilters[0].transform.localToWorldMatrix;

        combinedBounds = TransformBounds(combinedBounds, firstTransform);

        foreach (var meshFilter in meshFilters)
        {
            Bounds transformedBounds = TransformBounds(meshFilter.sharedMesh.bounds, meshFilter.transform.localToWorldMatrix);
            combinedBounds.Encapsulate(transformedBounds);
        }

        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = obj.AddComponent<BoxCollider>();
        }

        boxCollider.center = combinedBounds.center - obj.transform.position;
        boxCollider.size = combinedBounds.size;

        Undo.RegisterCreatedObjectUndo(boxCollider, "Create Combined Box Collider");
        EditorUtility.SetDirty(obj);
    }

    private static Bounds TransformBounds(Bounds bounds, Matrix4x4 transform)
    {
        Vector3 center = transform.MultiplyPoint(bounds.center);
        Vector3 extents = bounds.extents;
        Vector3 axisX = transform.MultiplyVector(new Vector3(extents.x, 0, 0));
        Vector3 axisY = transform.MultiplyVector(new Vector3(0, extents.y, 0));
        Vector3 axisZ = transform.MultiplyVector(new Vector3(0, 0, extents.z));

        extents = new Vector3(
            Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
            Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
            Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z)
        );

        return new Bounds(center, extents * 2);
    }
}
