using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor.SceneManagement;

public class QuickHierarchy : Editor
{

    public static List<GameObject> GameObjectList = new List<GameObject>();
    private static int ListCount;
    private static Rect windowRect;

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Active Panel %#H")]
    public static void ActivePanel()
    {
        windowRect = new Rect(SceneView.lastActiveSceneView.camera.pixelRect.width / 2, SceneView.lastActiveSceneView.camera.pixelRect.height / 2, 180, 25);
        SceneView.duringSceneGui += OnSceneView;
        LoadList();
    }

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Deactive Panel &#H")]
    public static void DeactivePanel()
    {
        SceneView.duringSceneGui -= OnSceneView;
    }

    private static void OnSceneView(SceneView sceneView)
    {
        windowRect = GUILayout.Window(0, windowRect, DoHierarchyDraw, "Quick Hierarchy", GUILayout.Height(25));
    }
    private static void DoHierarchyDraw(int windowID)
    {
        DrawList();
        GUI.DragWindow();
    }

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Add To List %E")]
    private static void AddToList()
    {
        foreach (GameObject a in Selection.gameObjects)
        {
            if (!GameObjectList.Contains(a))
            {
                GameObjectList.Add(a);
                SaveList();
            }
        }
    }

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Remove from List %W")]
    private static void RemoveFromList()
    {
        foreach (GameObject a in Selection.gameObjects)
        {
            if (GameObjectList.Contains(a))
            {
                GameObjectList.Remove(a);
                SaveList();
            }
        }
    }

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Load List %#L")]
    public static void LoadList()
    {
        string CurrentListCount = SceneManager.GetActiveScene().name + "_QHListCount";
        ListCount = EditorPrefs.GetInt(CurrentListCount);
        if (ListCount >= 0)
        {
            GameObjectList.Clear();
            for (int i = 0; i < ListCount; i++)
            {
                string loadName = SceneManager.GetActiveScene().name + "_QHGameObject" + i;
                string nameToFind = EditorPrefs.GetString(loadName);
                SearchInRoot(nameToFind);
            }
        }
    }

    [MenuItem("Tools/RebloomDev Tools/Quick Hierarchy/Clear List")]
    private static void ClearList()
    {
        if (GameObjectList.Count > 0)
        {
            bool ClearRequest = EditorUtility.DisplayDialog("Clear List", "Do you want to clear gizmos list ?", "Yeah !", "No");
            if (ClearRequest)
            {
                GameObjectList.Clear();
                string CurrentListCount = SceneManager.GetActiveScene().name + "_QHListCount";
                ListCount = EditorPrefs.GetInt(CurrentListCount);
                for (int i = 0; i < ListCount; i++)
                {
                    string loadName = SceneManager.GetActiveScene().name + "_QHGameObject" + i;
                    EditorPrefs.DeleteKey(loadName);
                }
            }

        }
        else
        {
            EditorUtility.DisplayDialog("Clear List", "List is empty ! :) ", "Got it !");
        }
    }

    public static void SaveList()
    {
            if (GameObjectList.Count >= 0)
            {
                for (int i = 0; i < GameObjectList.Count; i++)
                {
                    if (GameObjectList[i] != null)
                    {
                        string saveName = SceneManager.GetActiveScene().name + "_QHGameObject" + i;
                        EditorPrefs.SetString(saveName, GameObjectList[i].name);
                    }
                    else
                    {
                        string saveName = SceneManager.GetActiveScene().name + "_QHGameObject" + i;
                        EditorPrefs.DeleteKey(saveName);
                    }
                }
                string CurrentListCount = SceneManager.GetActiveScene().name + "_QHListCount";
                EditorPrefs.SetInt(CurrentListCount, GameObjectList.Count);
            }
    }

    private static void SearchInRoot(string name)
    {
        if (name != "")
        {
            GameObject[] LoadSceneRoot = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject a in LoadSceneRoot)
            {
                Transform[] AllChild = a.GetComponentsInChildren<Transform>(true);

                foreach (Transform b in AllChild)
                {
                    if (b.gameObject.name == name)
                    {
                        GameObjectList.Add(b.gameObject);
                    }
                }
            }
        }
    }

    private static void RemoveSelection(GameObject gameObject)
    {
        GameObjectList.Remove(gameObject);
        SaveList();
    }

    private static void DrawList()
    {
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.fixedHeight = 16;
        iconStyle.fixedWidth = 16;

        GUIStyle CaptionStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            fixedWidth = 120,
            normal = new GUIStyleState() {textColor = Color.white, background = Texture2D.blackTexture },
            active = new GUIStyleState() { textColor = Color.white, background = Texture2D.grayTexture },      
        };

        Texture Icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RebloomDev Tools/Quick Hierarchy/Icons/toggle.png");
        Texture selectIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RebloomDev Tools/Quick Hierarchy/Icons/select.png");
        Texture focusIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RebloomDev Tools/Quick Hierarchy/Icons/focus.png");
        Texture removeIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RebloomDev Tools/Quick Hierarchy/Icons/remove.png");
        Texture inactiveIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RebloomDev Tools/Quick Hierarchy/Icons/inactive.png");

        if (GameObjectList.Count > 0)
        {
            GUILayout.Space(5);
            foreach (GameObject a in GameObjectList.ToArray())
            {
                if (a != null)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.BeginHorizontal();
                    if(a.activeInHierarchy)
                    {
                        if (GUILayout.Button(Icon, iconStyle))
                        {
                            a.SetActive(false);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                    else if(!a.activeInHierarchy)
                    {
                        if (GUILayout.Button(inactiveIcon, iconStyle))
                        {
                            a.SetActive(true);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }

                    GUILayout.Space(2);
                    if (GUILayout.Button(a.name,CaptionStyle))
                    {
                        Selection.activeGameObject = a;
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button(focusIcon,iconStyle))
                    {
                        Selection.activeGameObject = a;
                        SceneView.FrameLastActiveSceneView();
                    }
                    if (GUILayout.Button(removeIcon, iconStyle))
                    {
                        RemoveSelection(a);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Add GameObjects from Hierarchy", MessageType.Warning);
        }
        EditorGUILayout.Space();
    }

    private void OnEnable()
    {
        LoadList();
    }



}
