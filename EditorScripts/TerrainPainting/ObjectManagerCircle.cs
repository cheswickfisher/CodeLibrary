using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine.AI;


[Serializable]
public struct PaintObject
{
    public Rect rect;
    public GameObject prefab;
    public Texture2D icon;
    public string name;
    public UnityEngine.Object obj;
    public string assetPath;
    public bool toggleValue;
    public bool scaleUniformToggle;
    public bool active;
    public int id;
    public Vector3 rotationOffset;
    public Vector3 scale;
    public Vector3 scaleJitter;
    public float scaleJitterUniform;
    public Vector3 rotationJitter;
    public float priority;
    public bool navmeshObstacle;

    public PaintObject(Rect rect, GameObject prefab, Texture2D icon, string name, UnityEngine.Object obj,string assetPath, int id)
    {
        this.rect = rect;
        this.prefab = prefab;
        this.icon = icon;
        this.name = name;
        this.obj = obj;
        this.assetPath = assetPath;
        this.id = id;
        toggleValue = false;
        active = false;
        rotationOffset = Vector3.zero;
        scale = Vector3.one;
        scaleJitter = Vector3.zero;
        scaleJitterUniform = 0.0f;
        rotationJitter = Vector3.zero;
        priority = .5f;
        scaleUniformToggle = false;
        navmeshObstacle = false;
    }
}

public static class IDGenerator
{

    public static int GenerateNextID(List<int> ids)
    {
        int nextId = 0;

        if (ids == null)
        {
            return nextId;
        }
        while (ids.Contains(nextId))
        {
            nextId++;
        }
        return nextId;
    }

    public static List<int> RemoveID(int id, List<int> ids)
    {
        if (ids.Contains(id))
        {
            ids.Remove(id);
        }
        return ids;
    }
}


/// <summary>
/// To paint objects, hold down Ctrl and press LMB
/// </summary>
public class ObjectManagerCircle : MonoBehaviour
{
    public Texture2D bg;

    public float radius = 5f;

    public int density = 5;

    public enum Actions { AddObjects, RemoveObjects }

    public Actions action;

    public LayerMask paintableSurface;

    public Color discColorAdd;

    public Color discColorRemove;

    [Tooltip("controls how much space is allowed between terrain prefabs.")]
    [Range(0, 20.0f)]public float prefabDistance;
    public int terrainPrefabLayer;
    private List<string> myStringList = new List<string>();

    private List<Rect> myRects = new List<Rect>();
    public PaintObject defaultBrushSettings = new PaintObject(default, null, null, "default", null, "", -1);
    public List<PaintObject> paintObjects = new List<PaintObject>();
    public List<PaintObject> brushObjects = new List<PaintObject>();
    public List<int> prefabIds = new List<int>();
    public PaintObject addedBrushObject;
    public string[] MyStrings { get => ReturnMyStrings(); }
    public List<Rect> MyRects { get => myRects;  }  
    public List<PaintObject> PaintObjects { get => paintObjects; }
    public Rect Drop_area { get => drop_area; set => drop_area = value; }
    public Vector2 ScrollPositionPrefabs { get => scrollPositionPrefabs; set => scrollPositionPrefabs = value; }
    public Vector2 StoredMousePos { get => storedMousePos; set => storedMousePos = value; }
    public Event Evt { get => evt; set => evt = value; }
    public List<Texture2D> textures = new List<Texture2D>();
    public bool checkCleared;
    public int currentBrushObjectsIndex;
    Rect drop_area;
    public Vector2 scrollPositionPrefabs = Vector2.one;
    public Vector2 scrollPositionBrush = Vector2.one;
    Vector2 storedMousePos = Vector2.zero;
    Event evt = null;
    bool shouldAddPaintObject = false;
    
    public bool ShouldAddPaintObject
    {
        get { return shouldAddPaintObject; }
        set {

            shouldAddPaintObject = value;
        }
    }

    public bool CheckCleared { get => checkCleared; set => checkCleared = value; }
    public List<Texture2D> Textures { get => textures; }
    public Vector2 ScrollPostionBrush { get => scrollPositionBrush; set => scrollPositionBrush = value; }
    public List<PaintObject> BrushObjects { get => brushObjects;  }
    
    public void AddString(string stringToAdd)
    {
        myStringList.Add(stringToAdd);
        Debug.Log("String added!");
       
    }

    public void AddRect(Rect rect)
    {
        myRects.Add(rect);
        Debug.Log("Rect added!");
    }

    public bool AddPaintObject(UnityEngine.Object p, Rect rect)
    {
        Texture2D icon = AssetPreview.GetAssetPreview(p);
        if(icon == null) { Debug.Log("null icon"); return false; }
        GameObject pp = p as GameObject;
        int id = IDGenerator.GenerateNextID(prefabIds);
        prefabIds.Add(id);
        PaintObject po = new PaintObject(rect, pp, icon, pp.name, p, AssetDatabase.GetAssetPath(p), id);
        paintObjects.Add(po);       
        shouldAddPaintObject = false;
        return true;
    }

    public void AddBrushObject(int id)
    {
        PaintObject orig = paintObjects.Find(x => x.id == id);
        PaintObject po = new PaintObject(orig.rect, orig.prefab, orig.icon, orig.name, orig.obj, orig.assetPath, orig.id);
        brushObjects.Add(po);
        Debug.Log(brushObjects.Count);
        addedBrushObject = po;
    }

    public void RemovePaintObject(int id)
    {
        prefabIds = IDGenerator.RemoveID(id, prefabIds);
        paintObjects.Remove(paintObjects.Find(x => x.id == id));
    }

    public void RemoveBrushObject(int id)
    {
        brushObjects.Remove(brushObjects.Find(x => x.id == id));
    }

    public void AddTexture(UnityEngine.Object p)
    {
        Texture2D tex = AssetPreview.GetAssetPreview(p);
        textures.Add(tex);
    }


    private string[] ReturnMyStrings()
    {
        return myStringList.ToArray();
    }

    private void Test(UnityEngine.Object r)
    {
        GameObject t = (GameObject)r;
       
    }

    private void OnValidate()
    {
        drop_area = new Rect();
        bg = Resources.Load<Texture2D>("Assets/Textures/GUIStyle/Borders/PlainBlack.png");
    }

    
    public void StartAddPrefab(IEnumerator prefabCoroutine)
    {
        StartCoroutine(prefabCoroutine);
    }
    
    public void AddPrefab(Vector3 center, Vector3 normal, PaintObject po, Func<PaintObject, Vector3, GameObject> spawnInstance)
    {
        int maxTries = 5;
        int currentTry = 0;
        
        while (currentTry < maxTries)
        {
            Vector2 randomPos2D = UnityEngine.Random.insideUnitCircle;
            Vector3 n = new Vector3(randomPos2D.x, 0.0f, randomPos2D.y);
            n = Vector3.ProjectOnPlane(n, normal).normalized;
            Vector3 rotatedPos = center + n * UnityEngine.Random.Range(0, radius);

            Ray ray = new Ray(rotatedPos + normal * 1.2f, -normal);
            RaycastHit hit;
            Vector3 orientationNormal = Vector3.up;

            if (Physics.Raycast(ray, out hit, 10.0f, paintableSurface, QueryTriggerInteraction.Ignore))
            {
                orientationNormal = hit.normal;

                Collider[] overlaps = Physics.OverlapSphere(hit.point, prefabDistance, 1 << terrainPrefabLayer, QueryTriggerInteraction.Ignore);
                if (overlaps.Length > 0)
                {
                    currentTry++;
                    continue;
                }
                currentTry = maxTries;
                GameObject newPrefabObj = spawnInstance(po, orientationNormal);
                Collider col = newPrefabObj.GetComponentInChildren<Collider>();
                if (col != null)
                {
                    col.gameObject.layer = terrainPrefabLayer;
                    if (po.navmeshObstacle)
                    {
                        NavMeshModifier nms = col.gameObject.AddComponent<NavMeshModifier>();
                        nms.overrideArea = true;
                        nms.area = 1;
                    }

                }
                newPrefabObj.transform.position = hit.point;
                newPrefabObj.transform.parent = transform;

                Physics.SyncTransforms();

            }
        }
    }


    public void RemoveObjects(Vector3 center)
    {
        GameObject[] allChildren = GetAllChildren();

        foreach (GameObject child in allChildren)
        {
            if (Vector3.SqrMagnitude(child.transform.position - center) < radius * radius)
            {
                DestroyImmediate(child);
            }
        }
    }

    public void RemoveAllObjects()
    {
        GameObject[] allChildren = GetAllChildren();
        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child);
        }
    }

    private GameObject[] GetAllChildren()
    {
        GameObject[] allChildren = new GameObject[transform.childCount];

        int childCount = 0;
        foreach (Transform child in transform)
        {
            allChildren[childCount] = child.gameObject;
            childCount += 1;
        }

        return allChildren;
    }


}
