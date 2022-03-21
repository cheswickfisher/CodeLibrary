using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FacePainter))]
public class FacePainterEditor : Editor
{
    private FacePainter facePainter;
    private Vector3 center;
    LayerMask paintableSurface;
    private float deltaMtp = .25f;
    Vector3 normal;

    private void OnEnable()
    {
        facePainter = target as FacePainter;
        Tools.hidden = true;
        center = Vector3.zero;
        normal = Vector3.up;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void OnSceneGUI()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        RaycastHit hit;
        bool hitCheck = false;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, facePainter.paintableSurface, QueryTriggerInteraction.Ignore))
        {
            center = hit.point;
            normal = hit.normal;
            SceneView.RepaintAll();
            hitCheck = true;
        }

        Handles.color = facePainter.discColor;
        Handles.DrawWireDisc(center, normal, facePainter.radius);

        HandleUtility.AddDefaultControl(0);

        if (Event.current.shift)
        {
            float mouseDelta = Event.current.delta.y;
            facePainter.radius += mouseDelta * .25f;
            facePainter.radius = Mathf.Clamp(facePainter.radius, 0.0f, 20.0f);

        }
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            if (facePainter.action == FacePainter.Actions.Paint)
            {
                if (hitCheck)
                {
                    AddPaint(hit);
                }

                MarkSceneAsDirty();

            }

            else if (facePainter.action == FacePainter.Actions.Erase)
            {
                facePainter.Erase(center);

                MarkSceneAsDirty();

            }
        }

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void MarkSceneAsDirty()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
    }

    private void AddPaint(RaycastHit hit)
    {
        facePainter.Paint(hit);
    }


}
