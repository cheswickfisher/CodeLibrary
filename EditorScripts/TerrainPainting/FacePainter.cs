using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePainter : MonoBehaviour
{
    public bool isPainting;
    public GameObject objectToPaint;
    public Mesh currentMesh;
    public Color brushColor;
    public Color discColor;
    public LayerMask paintableSurface;
    [Range(0,1)]public float opacity;
    public enum Actions { Paint, Erase }
    public Actions action;
    public Color[] colors;

    public float radius = 5.0f;


    public void Erase(Vector3 center)
    {

    }

    public void Paint(RaycastHit hit)
    {
        if (isPainting)
        {
            currentMesh = objectToPaint.GetComponent<MeshFilter>().sharedMesh;
            PaintFacesTwo(currentMesh, hit, hit.triangleIndex, brushColor);
        }
    }

    private void PaintFacesTwo(Mesh mesh, RaycastHit hitPoint, int i, Color brushColor)
    {
        Matrix4x4 worldToLocal = objectToPaint.transform.worldToLocalMatrix;
        Vector3 localHitPoint = worldToLocal.MultiplyPoint3x4(hitPoint.point);
        Vector3[] vertices = mesh.vertices;
        colors = new Color[0];
        int[] triangles = mesh.triangles;

        if (mesh.colors.Length > 0)
        {
            colors = mesh.colors;
        }

        else
        {
            colors = new Color[vertices.Length];
        }

        float brushRadius = radius * radius;
        colors[triangles[hitPoint.triangleIndex * 3 + 0]] = brushColor;
        colors[triangles[hitPoint.triangleIndex * 3 + 1]] = brushColor;
        colors[triangles[hitPoint.triangleIndex * 3 + 2]] = brushColor;
        Debug.Log("color");
        mesh.colors = colors;

    }

    private void PaintFaces(Mesh mesh, RaycastHit hitPoint, int i, Color brushColor)
    {
        Matrix4x4 worldToLocal = objectToPaint.transform.worldToLocalMatrix;
        Vector3 localHitPoint = worldToLocal.MultiplyPoint3x4(hitPoint.point);
        Vector3[] vertices = mesh.vertices;
        colors = new Color[0];
        int[] triangles = mesh.triangles;

        if(mesh.colors.Length > 0)
        {
            colors = mesh.colors;
        }

        else
        {
            colors = new Color[vertices.Length];
        }

        float brushRadius = radius * radius;

        for (int z = 0; z < triangles.Length / 3; z++)
        {
            int inRangeCount = 0;
            bool triangleInRange = false;

            for (int v = 0; v < 3; v++)
            {
                Vector3 vertexPos = vertices[triangles[z * 3 + v]];
                if ((vertexPos - localHitPoint).sqrMagnitude < (brushRadius))
                {
                    triangleInRange = true;
                    inRangeCount++;
                }
            }
            //probably can put this in above loop
            if (triangleInRange)
            {
                for (int v = 0; v < 3; v++)
                {
                    colors[triangles[ z * 3 + v]] = Color.Lerp(colors[triangles[z * 3 + v]], brushColor, opacity);
                }
            }
            if (inRangeCount == 1)
            {
                Vector3 diagVertPos0 = Vector3.zero;
                Vector3 diagVertPos1 = Vector3.zero;

                Vector3 f0 = vertices[triangles[z * 3]];
                Vector3 f1 = vertices[triangles[z * 3 + 1]];
                Vector3 f2 = vertices[triangles[z * 3 + 2]];

                float f0f1 = (f0.x - f1.x) * (f0.x - f1.x) + (f0.z - f1.z) * (f0.z - f1.z);
                float f0f2 = (f0.x - f2.x) * (f0.x - f2.x) + (f0.z - f2.z) * (f0.z - f2.z);
                float f1f2 = (f1.x - f2.x) * (f1.x - f2.x) + (f1.z - f2.z) * (f1.z - f2.z);

                bool cornerInRange = false;
                if (f0f1 > f0f2 && f0f1 > f1f2)
                {
                    diagVertPos0 = f0;
                    diagVertPos1 = f1;
                    cornerInRange = (f2 - localHitPoint).sqrMagnitude < brushRadius;
                }
                if (f0f2 > f0f1 && f0f2 > f1f2)
                {
                    diagVertPos0 = f0;
                    diagVertPos1 = f2;
                    cornerInRange = (f1 - localHitPoint).sqrMagnitude < brushRadius;
                }
                if (f1f2 > f0f1 && f1f2 > f0f2)
                {
                    diagVertPos0 = f1;
                    diagVertPos1 = f2;
                    cornerInRange = (f0 - localHitPoint).sqrMagnitude < brushRadius;
                }

                if (!cornerInRange)
                    continue;

                for (int x = 0; x < triangles.Length / 3; x++)
                {
                    for (int v = 0; v < 3; v++)
                    {
                        if (vertices[triangles[x * 3 + v]] == diagVertPos0)
                        {
                            for (int w = 0; w < 3; w++)
                            {
                                if (vertices[triangles[x * 3 + w]] == diagVertPos1)
                                {
                                    for (int t = 0; t < 3; t++)
                                    {
                                        colors[triangles[x * 3 + t]] = Color.Lerp(colors[triangles[x * 3 + t]], brushColor, opacity);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        mesh.colors = colors;
    }

}
