using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateBoxColliderFromMeshColliderEditor : MonoBehaviour
{
    [MenuItem("Tools/Generate Box Collider from mesh collider")]
    public static void GenerateCombinedBoxCollider()
    {
        GenerateBoxCollider(Selection.activeGameObject);
    }

    public static void GenerateBoxCollider(GameObject gameObject)
    {
        // Obtener el MeshCollider del objeto
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            Debug.LogWarning("No se encontró un MeshCollider válido en el objeto.");
            return;
        }

        // Obtener los límites del MeshCollider
        Bounds bounds = meshCollider.sharedMesh.bounds;

        // Crear o actualizar el BoxCollider en el mismo GameObject
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Configurar el BoxCollider basado en los límites del MeshCollider
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;

        Debug.Log("BoxCollider generado a partir del MeshCollider.");
    }
}
