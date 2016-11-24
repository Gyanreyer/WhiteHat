using UnityEngine;
using UnityEditor;

/// <summary>
/// Comment this class out if you really hate all the names over objects in the scene view!!!
/// </summary>
public class NameInEditor : Editor
{
    
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void sdf(Transform transform, GizmoType gizmoType)
    {
        Handles.color = Color.white;
        Handles.Label(transform.position, transform.gameObject.name);
    }
}