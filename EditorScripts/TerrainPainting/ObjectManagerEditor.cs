using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public delegate GameObject CreatePrefabInstance(GameObject prefab);

[CustomEditor(typeof(ObjectManagerCircle))][CanEditMultipleObjects]
public class ObjectManagerEditor : Editor
{

    private ObjectManagerCircle objectManager;
    private Vector3 center;
    Vector3 normal;
    LayerMask paintableSurface;
    private float deltaMtp = .25f;

    private string enteredText;
    int selGridInt = 0;
    Rect defaultDropRect;
    Rect dropZone;
    GUIStyle border;
    GUIStyle horizontalScrollBarStyle;
    GUIStyle bgStyle;
    GUIContent c;
    GUIStyle b;
    GUIContent[] floatFieldLabels;
    GUIContent scaleLabel;
    float[] scaleFloatFieldValues;
    int brushToRemoveId;

    private delegate void RectContainAction(Event evt, Rect r);
   
    SerializedProperty testScroll;
    SerializedProperty scrollPositionPrefabs;
    SerializedProperty scrollPositionBrush;
    SerializedProperty paintObjs;
    SerializedProperty brushObjs;
    SerializedProperty checkCleared;
    SerializedProperty textures;
    SerializedProperty currentBrushObjectIndex;
    SerializedProperty toggle;
    bool scaleJitterToggle;
    SerializedProperty activePrefab;
    SerializedProperty defaultBrushSettings;
    SerializedProperty dragObject;
    
    int activePrefabID;
    Vector3 scale;
    Vector3 rotationOffset;
    Vector2 scrollPositionVertical;
    float scaleJitter;
    float rotationJitter;
    float priority;

    Texture2D borderTexture;
    Texture2D borderTextureB;

    RectContainAction dropAction;
    CreatePrefabInstance createPrefabInstance;

    private void OnEnable()
    {
        objectManager = target as ObjectManagerCircle;
        Tools.hidden = true;
        normal = Vector3.up;
        center = Vector3.zero;
        defaultDropRect = new Rect(0.0f, 0.0f, 50.0f, 50.0f);
        dropZone = new Rect(0.0f, 0.0f, 1000f, 100f);
        scrollPositionPrefabs = serializedObject.FindProperty("scrollPositionPrefabs");
        scrollPositionBrush = serializedObject.FindProperty("scrollPositionBrush");
        paintObjs = serializedObject.FindProperty("paintObjects");
        brushObjs = serializedObject.FindProperty("brushObjects");
        textures = serializedObject.FindProperty("textures");
        checkCleared = serializedObject.FindProperty("checkCleared");
        currentBrushObjectIndex = serializedObject.FindProperty("currentBrushObjectsIndex");
        borderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/GUIStyle/Borders/PlainBlackThick.png");
        borderTextureB = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/GUIStyle/Borders/PlainBlackThin.png");
        defaultBrushSettings = serializedObject.FindProperty("defaultBrushSettings");

        border = new GUIStyle();
        border.normal.background = borderTextureB;
        border.border = new RectOffset(0, 0, 0, 0);
        border.border.left = 65;
        border.border.right = 65;
        border.border.top = 65;
        border.border.bottom = 65;

        b = new GUIStyle();
        c = new GUIContent();      

        floatFieldLabels = new GUIContent[3] { new GUIContent(), new GUIContent(), new GUIContent() };
        floatFieldLabels[0].text = "X";
        floatFieldLabels[1].text = "Y";
        floatFieldLabels[2].text = "Z";

        scaleFloatFieldValues = new float[3] { 1, 1, 1 };
        scaleLabel = new GUIContent();
        scaleLabel.text = "Scale";

        if(activePrefab == null)
        {
            activePrefab = defaultBrushSettings;
            activePrefabID = -1;
        }

        if(activePrefab == defaultBrushSettings)
        {
            ResetBrushOptions();
        }
    }

    private void OnDisable()
    {
        Tools.hidden = false;
        if (activePrefab == defaultBrushSettings)
        {
            ResetBrushOptions();
        }

    }



    private void OnSceneGUI()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, objectManager.paintableSurface, QueryTriggerInteraction.Ignore))
        {
            center = hit.point;
            normal = hit.normal;

            SceneView.RepaintAll();
        }

        if (objectManager.action == ObjectManagerCircle.Actions.AddObjects)
        {
            Handles.color = objectManager.discColorAdd;
        }

        else if (objectManager.action == ObjectManagerCircle.Actions.RemoveObjects)
        {
            Handles.color = objectManager.discColorRemove;
        }

        Handles.DrawWireDisc(center, normal, objectManager.radius);

        HandleUtility.AddDefaultControl(0);
        if (Event.current.shift)
        {
            float mouseDelta = Event.current.delta.y;
            objectManager.radius += mouseDelta * deltaMtp;
            objectManager.radius = Mathf.Clamp(objectManager.radius, 0.0f, 20.0f);

        }
        if (Event.current.control)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (objectManager.action == ObjectManagerCircle.Actions.AddObjects)
                {
                    AddNewPrefabs();

                    MarkSceneAsDirty();
                }

                else if (objectManager.action == ObjectManagerCircle.Actions.RemoveObjects)
                {
                    objectManager.RemoveObjects(center);

                    MarkSceneAsDirty();
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("density"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("action"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("paintableSurface"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("discColorAdd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("discColorRemove"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainPrefabLayer"));

        if (GUILayout.Button("Remove all objects", GUILayout.Height(20), GUILayout.Width(200)))
        {
            if (EditorUtility.DisplayDialog("Safety check!", "Do you want to remove all objects?", "Yes", "No"))
            {
                objectManager.RemoveAllObjects();

                MarkSceneAsDirty();
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("Brush Contents");

        GUILayout.Space(5);

        horizontalScrollBarStyle = GUI.skin.horizontalScrollbar;

        bgStyle = GUI.skin.box;

        if (brushObjs.arraySize < 1)
        {
            c.text = "- Empty -";
            b.alignment = TextAnchor.UpperCenter;
            b.padding = new RectOffset();
            b.padding.top = 30;
        }

        scrollPositionBrush.vector2Value = GUILayout.BeginScrollView(scrollPositionBrush.vector2Value, true, true, horizontalScrollBarStyle, GUIStyle.none, bgStyle, GUILayout.ExpandWidth(true), GUILayout.Height(85));
        GUILayout.BeginHorizontal(c,b);
        for (int i = 0; i < brushObjs.arraySize; i++)
        {
            SerializedProperty obj = brushObjs.GetArrayElementAtIndex(i);
            int objId = obj.FindPropertyRelative("id").intValue;
            if(objId == brushToRemoveId)
            {
                brushObjs.DeleteArrayElementAtIndex(i);
                brushToRemoveId = -1;
                continue;
            }
            BrushBoxGUI(obj);
        }

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        Rect brushRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(20);
        GUILayout.Label("Prefabs");
        GUILayout.Space(5);

        c.text = "";
        b.alignment = TextAnchor.UpperCenter;
        b.padding = new RectOffset();
        b.padding.top = 0;

        if (paintObjs.arraySize < 1)
        {
            c.text = "- Drag Prefabs Here -";
            b.alignment = TextAnchor.UpperCenter;
            b.padding = new RectOffset();
            b.padding.top = 30;
        }

        scrollPositionPrefabs.vector2Value = GUILayout.BeginScrollView(scrollPositionPrefabs.vector2Value, true,true, horizontalScrollBarStyle, GUIStyle.none, bgStyle,  GUILayout.ExpandWidth(true), GUILayout.Height(85));
        GUILayout.BeginHorizontal(c,b);
       
        for (int i = 0; i < paintObjs.arraySize; i++)
        {   
                PrefabBoxGUI(paintObjs.GetArrayElementAtIndex(i));      
        }

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        Rect prefabRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(20);

        if (OkToAddThingsToGUI())
        {
            dropAction = AddPaintObject;
            
            CheckDropArea(prefabRect, dropAction);
        }

        if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.Height(25)))
        {
            paintObjs.ClearArray();
            brushObjs.ClearArray();
            activePrefab = defaultBrushSettings;
        }
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
        }

        GUILayout.Space(20);

        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(280), GUILayout.ExpandWidth(true));
        GUI.Box(r, "Brush Options");
        Rect rr = GUILayoutUtility.GetLastRect();
        Rect scaleRect = new Rect(rr.x + 25.0f, rr.y + 25.0f, rr.width - 50.0f, rr.height);
        activePrefab.FindPropertyRelative("scale").vector3Value = EditorGUI.Vector3Field(scaleRect, "Scale", activePrefab.FindPropertyRelative("scale").vector3Value);
        Rect rotRect = new Rect(rr.x + 25.0f, rr.y + 45.0f, rr.width - 50.0f, rr.height);
        activePrefab.FindPropertyRelative("rotationOffset").vector3Value = EditorGUI.Vector3Field(rotRect, "Rotation", activePrefab.FindPropertyRelative("rotationOffset").vector3Value);

        Rect scaleJitterLabelRect = new Rect(rr.x + 25.0f, rr.y + 75, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(scaleJitterLabelRect, "Scale Jitter");
        Rect scaleToggleRect = new Rect(rr.x + 100.0f, rr.y + 75, 10.0f, 15.0f);
        EditorGUI.BeginChangeCheck();
        scaleJitterToggle =  EditorGUI.Toggle(scaleToggleRect, scaleJitterToggle);
        EditorGUI.EndChangeCheck();
        if (scaleJitterToggle)
        {
            Rect scaleJitterXLabelRect = new Rect(rr.x + 185.0f, rr.y + 75, rr.width - 50.0f, 15.0f);
            EditorGUI.LabelField(scaleJitterXLabelRect, "X");
            Rect scaleJitterXRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 75.0f, rr.width - 230.0f, 15.0f);
            float scaleJitterX = EditorGUI.Slider(scaleJitterXRect, activePrefab.FindPropertyRelative("scaleJitter").vector3Value.x, 0.0f, 1.0f);
            Rect scaleJitterYLabelRect = new Rect(rr.x + 185.0f, rr.y + 95, rr.width - 50.0f, 15.0f);
            EditorGUI.LabelField(scaleJitterYLabelRect, "Y");
            Rect scaleJitterYRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 95.0f, rr.width - 230.0f, 15.0f);
            float scaleJitterY = EditorGUI.Slider(scaleJitterYRect, activePrefab.FindPropertyRelative("scaleJitter").vector3Value.y, 0.0f, 1.0f);
            Rect scaleJitterZLabelRect = new Rect(rr.x + 185.0f, rr.y + 115, rr.width - 50.0f, 15.0f);
            EditorGUI.LabelField(scaleJitterZLabelRect, "Z");
            Rect scaleJitterZRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 115.0f, rr.width - 230.0f, 15.0f);
            float scaleJitterZ = EditorGUI.Slider(scaleJitterZRect, activePrefab.FindPropertyRelative("scaleJitter").vector3Value.z, 0.0f, 1.0f);
            Vector3 scaleJitterXYZ = new Vector3(scaleJitterX, scaleJitterY, scaleJitterZ);
            activePrefab.FindPropertyRelative("scaleJitter").vector3Value = scaleJitterXYZ;
        }

        else
        {
            Rect scaleJitterUniformRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 75.0f, rr.width - 230.0f, 15.0f);
            activePrefab.FindPropertyRelative("scaleJitterUniform").floatValue = EditorGUI.Slider(scaleJitterUniformRect, activePrefab.FindPropertyRelative("scaleJitterUniform").floatValue, 0.0f, 1.0f);
        }


        Rect rotationJitterLabelRect = new Rect(rr.x + 25.0f, rr.y + 145, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(rotationJitterLabelRect, "Rotation Jitter");        
        Rect rotationJitterXLabelRect = new Rect(rr.x + 185.0f, rr.y + 145, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(rotationJitterXLabelRect, "X");
        Rect rotationJitterXRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 145, rr.width - 230.0f, 15.0f);
        float rotationJitterX = EditorGUI.Slider(rotationJitterXRect, activePrefab.FindPropertyRelative("rotationJitter").vector3Value.x, 0.0f, 1.0f);
        Rect rotationJitterYLabelRect = new Rect(rr.x + 185.0f, rr.y + 165, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(rotationJitterYLabelRect, "Y");
        Rect rotationJitterYRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 165, rr.width - 230.0f, 15.0f);
        float rotationJitterY = EditorGUI.Slider(rotationJitterYRect, activePrefab.FindPropertyRelative("rotationJitter").vector3Value.y, 0.0f, 1.0f);
        Rect rotationJitterZLabelRect = new Rect(rr.x + 185.0f, rr.y + 185, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(rotationJitterZLabelRect, "Z");
        Rect rotationJitterZRect = new Rect(rr.x + rr.width - 25.0f - 180, rr.y + 185, rr.width - 230.0f, 15.0f);
        float rotationJitterZ = EditorGUI.Slider(rotationJitterZRect, activePrefab.FindPropertyRelative("rotationJitter").vector3Value.z, 0.0f, 1.0f);

        Rect priorityRect = new Rect(rr.x + 25.0f, rr.y + 225, rr.width - 50.0f, 15.0f);
        activePrefab.FindPropertyRelative("priority").floatValue = EditorGUI.Slider(priorityRect, "Priority", activePrefab.FindPropertyRelative("priority").floatValue, 0.0f, 1.0f);

        Rect navmeshObstacleLabelRect = new Rect(rr.x + 25.0f, rr.y + 245, rr.width - 50.0f, 15.0f);
        EditorGUI.LabelField(navmeshObstacleLabelRect, "Navmesh Obstacle");
        Rect naveMeshObstacleRect = new Rect(rr.x + 143.0f, rr.y + 245, 10.0f, 15.0f);
        activePrefab.FindPropertyRelative("navmeshObstacle").boolValue = EditorGUI.Toggle(naveMeshObstacleRect, activePrefab.FindPropertyRelative("navmeshObstacle").boolValue);

        Vector3 rotationJitterXYZ = new Vector3(rotationJitterX, rotationJitterY, rotationJitterZ);
        activePrefab.FindPropertyRelative("rotationJitter").vector3Value = rotationJitterXYZ;
        activePrefab.FindPropertyRelative("scaleUniformToggle").boolValue = scaleJitterToggle;

        GUILayout.Space(5);

        if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(25)))
        {
            ResetBrushOptions();          
        }
        
        bool w = serializedObject.ApplyModifiedProperties();        

    }

    private void PrefabBoxGUI(SerializedProperty obj)
    {

        Event evt = Event.current;
        GUILayout.Space(10);

        Texture2D icon = AssetPreview.GetAssetPreview(obj.FindPropertyRelative("icon").objectReferenceValue);
        if (icon == null)
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(obj.FindPropertyRelative("assetPath").stringValue);
            icon = AssetPreview.GetAssetPreview(prefab);
        }
        Rect rect = obj.FindPropertyRelative("rect").rectValue;
        toggle = obj.FindPropertyRelative("toggleValue");
        Rect prefabBox = EditorGUILayout.GetControlRect(GUILayout.Height(rect.height), GUILayout.Width(rect.width));
        GUI.Box(prefabBox, icon, "");
        Rect borderBox = new Rect(prefabBox.min, prefabBox.size * 1.0f);
        if (obj.FindPropertyRelative("id").intValue == activePrefabID)
        {            
            GUI.Box(borderBox, "", border);
            CheckForPrefabRemoval(obj.FindPropertyRelative("id").intValue);
        }
        Vector2 toggleSize = new Vector2(25f, 25f);

        Rect toggleBox = new Rect(prefabBox.max - (toggleSize * .6f), toggleSize);

        EditorGUI.BeginChangeCheck();

        toggle.boolValue = GUI.Toggle(toggleBox, toggle.boolValue, "");

        if (EditorGUI.EndChangeCheck())
        {
            if (toggle.boolValue)
            {
                brushObjs.arraySize++;
                brushObjs.GetArrayElementAtIndex(brushObjs.arraySize - 1).FindPropertyRelative("rect").rectValue = obj.FindPropertyRelative("rect").rectValue;
                brushObjs.GetArrayElementAtIndex(brushObjs.arraySize - 1).FindPropertyRelative("name").stringValue = obj.FindPropertyRelative("name").stringValue;
                brushObjs.GetArrayElementAtIndex(brushObjs.arraySize - 1).FindPropertyRelative("obj").objectReferenceValue = obj.FindPropertyRelative("obj").objectReferenceValue;
                brushObjs.GetArrayElementAtIndex(brushObjs.arraySize - 1).FindPropertyRelative("assetPath").stringValue = obj.FindPropertyRelative("assetPath").stringValue;
                brushObjs.GetArrayElementAtIndex(brushObjs.arraySize - 1).FindPropertyRelative("id").intValue = obj.FindPropertyRelative("id").intValue;
            }
            else if (!toggle.boolValue)
            {
                 brushToRemoveId = obj.FindPropertyRelative("id").intValue;                
            }
        }

        RectContainAction prefabBoxAction = new RectContainAction(DragPrefabFromScrollBar); 
        CheckDropArea(prefabBox, prefabBoxAction);
        CheckButtonPress(evt, prefabBox, obj);

        GUILayout.Space(10);
    }

    private void BrushBoxGUI(SerializedProperty obj)
    {
        GUILayout.Space(10);
        Texture2D icon = AssetPreview.GetAssetPreview(obj.FindPropertyRelative("icon").objectReferenceValue);
        if (icon == null)
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(obj.FindPropertyRelative("assetPath").stringValue);
            icon = AssetPreview.GetAssetPreview(prefab);
        }

        Rect rect = obj.FindPropertyRelative("rect").rectValue;
        Rect prefabBox = EditorGUILayout.GetControlRect(GUILayout.Height(rect.height), GUILayout.Width(rect.width));
        GUI.Box(prefabBox, icon, "");
        GUILayout.Space(10);
    }

    private void CheckDropArea(Rect r, RectContainAction rectContainAction)
    {
        Event evt = Event.current;
        if (r.Contains(evt.mousePosition))
        {
            rectContainAction(evt, r);
        }
    }

    private void AddPaintObject(Event evt, Rect r)
    {
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!r.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is GameObject)
                        {

                            objectManager.AddPaintObject(dragged_object, new Rect(100.0f, 50.0f, 64.0f, 64.0f));                            
                        }
                    }
                }
                break;
        }
    }

    private void CheckButtonPress(Event evt, Rect r,  SerializedProperty obj)
    {

        switch (evt.type)
        { 

            case EventType.MouseDown:
                if (!r.Contains(evt.mousePosition))
                    return;
                else
                {
                    if(activePrefabID == obj.FindPropertyRelative("id").intValue)
                    {
                        activePrefabID = -1;
                        activePrefab = defaultBrushSettings;
                        ResetBrushOptions();
                        return;
                    }
                    else
                    {
                        activePrefabID = obj.FindPropertyRelative("id").intValue;
                        activePrefab = obj;
                    }
                }
                break;

        }

    }

    private void ResetBrushOptions()
    {
        activePrefab.FindPropertyRelative("scale").vector3Value = Vector3.one;
        activePrefab.FindPropertyRelative("rotationOffset").vector3Value = Vector3.zero;
        activePrefab.FindPropertyRelative("scaleJitter").vector3Value = Vector3.zero;
        activePrefab.FindPropertyRelative("scaleJitterUniform").floatValue = 0.0f;
        activePrefab.FindPropertyRelative("rotationJitter").vector3Value = Vector3.zero;
        activePrefab.FindPropertyRelative("priority").floatValue = 0.5f;
        activePrefab.FindPropertyRelative("navmeshObstacle").boolValue = false;
        serializedObject.ApplyModifiedProperties();
    }

        private void AddBrushObject(int id)
    {
        Event evt = Event.current;
        objectManager.AddBrushObject(id);
        brushObjs = serializedObject.FindProperty("brushObjects");
        bool w = serializedObject.ApplyModifiedProperties();

    }

    private void CheckForPrefabRemoval(int id)
    {
        Event evt = Event.current;
        if(evt.type == EventType.KeyDown)
        {
            if(Event.current.keyCode == KeyCode.Backspace)
            {
                objectManager.RemovePaintObject(id);
                brushToRemoveId = id;
                activePrefabID = -1;
                activePrefab = defaultBrushSettings;
                ResetBrushOptions();
            }
        }
    }

    private void RemoveBrushObject(int index)
    {
       objectManager.RemoveBrushObject(index);
    }

    private  void DragPrefabFromScrollBar(Event evt, Rect r)
    {
       // Debug.Log("evt2: " + evt.type);

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }
    }

    private bool OkToAddThingsToGUI()
    {
        if ((Event.current.type == EventType.Repaint && checkCleared.boolValue == true) ||
     Event.current.type == EventType.Layout)
        {
            checkCleared.boolValue = false;
        }
        if (Event.current.type == EventType.Layout)
        {
            checkCleared.boolValue = true;
        }

        return checkCleared.boolValue;
    }

    //forces unity to save
    private void MarkSceneAsDirty()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
    }

    private void AddNewPrefabs()
    {
        Spawn();
       // IEnumerator timeredSpawnCoroutine = TimeredSpawn();
       // objectManager.StartAddPrefab(timeredSpawnCoroutine);

       // IEnumerator testCoroutine = Test();
       // objectManager.StartAddPrefab(testCoroutine);
    }

    private void Spawn()
    {
        int i = 0;
        while (i < objectManager.density)
        {
            int index = UnityEngine.Random.Range(0, objectManager.brushObjects.Count);
            float p = UnityEngine.Random.Range(0, 1.0f);
            int brushId = objectManager.brushObjects[index].id;
            PaintObject po = objectManager.paintObjects.Find(x => x.id == brushId);
            float priority = po.priority;
            if (priority > p)
            {
                Func<PaintObject, Vector3, GameObject> spawnInstanceFromPrefab = SpawnPrefab;
                objectManager.AddPrefab(center, normal, po, spawnInstanceFromPrefab);
            }
            i++;
        }
    }

    private IEnumerator TimeredSpawn()
    {
        WaitForFixedUpdate wffu = new WaitForFixedUpdate();
        int i = 0;
        while (i < objectManager.density)
        {
            int index = UnityEngine.Random.Range(0, objectManager.brushObjects.Count);
            float p = UnityEngine.Random.Range(0, 1.0f);
            int brushId = objectManager.brushObjects[index].id;
            PaintObject po = objectManager.paintObjects.Find(x => x.id == brushId);
            float priority = po.priority;
            if (priority > p)
            {
                Func<PaintObject, Vector3, GameObject> spawnInstanceFromPrefab = SpawnPrefab;
                objectManager.AddPrefab(center, normal, po, spawnInstanceFromPrefab);                
            }
            i++;
            yield return null;
        }        
    }

    private IEnumerator Test()
    {
        int i = 0;
        int t = 100;
        while(i < t)
        {
            i++;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    private GameObject SpawnPrefab(PaintObject po, Vector3 orientationNormal)
    {
        GameObject newPrefabObj = PrefabUtility.InstantiatePrefab(po.prefab) as GameObject;

        float scaleX = 1;
        float scaleY = 1;
        float scaleZ = 1;
        if (!po.scaleUniformToggle)
        {
            float jitter = UnityEngine.Random.Range(1.0f - po.scaleJitterUniform, 1.0f + po.scaleJitterUniform);
            scaleX = po.scale.x * jitter;
            scaleY = po.scale.y * jitter;
            scaleZ = po.scale.z * jitter;
        }
        else if (po.scaleUniformToggle)
        {
            scaleX = po.scale.x * UnityEngine.Random.Range(1.0f - po.scaleJitter.x, 1.0f + po.scaleJitter.x);
            scaleY = po.scale.y * UnityEngine.Random.Range(1.0f - po.scaleJitter.y, 1.0f + po.scaleJitter.y);
            scaleZ = po.scale.z * UnityEngine.Random.Range(1.0f - po.scaleJitter.z, 1.0f + po.scaleJitter.z);
        }

        newPrefabObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        Vector3 rotation = po.rotationOffset;

        float rotationJitterX = UnityEngine.Random.Range(-po.rotationJitter.x * 180.0f, po.rotationJitter.x * 180.0f);
        float rotationJitterY = UnityEngine.Random.Range(-po.rotationJitter.y * 180.0f, po.rotationJitter.y * 180.0f);
        float rotationJitterZ = UnityEngine.Random.Range(-po.rotationJitter.z * 180.0f, po.rotationJitter.z * 180.0f);
        Vector3 finalRotation = new Vector3(po.rotationOffset.x + rotationJitterX, po.rotationOffset.y + rotationJitterY, po.rotationOffset.z + rotationJitterZ);

        Quaternion up = Quaternion.FromToRotation(Vector3.up, orientationNormal);
        Quaternion finalRot = up * Quaternion.Euler(finalRotation);

        newPrefabObj.transform.rotation = finalRot;

        return newPrefabObj;
    }


}
