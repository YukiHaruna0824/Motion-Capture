using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SteeringBehavior.LevelEditor
{
    public class PathManagerWindow : EditorWindow
    {
        public static PathManagerWindow instance;
        public static BezierPath _Path;
        public GameObject pm;
        const float Button_Size = 48f;
        private int _state;

        // Common Icon
        private GUIContent _contentExportLevel, _contentImportLevel , _contentExportPath, _contentImportPath;
        // Level Icon
        private GUIContent _contentCreate;
        // Path Icon
        private GUIContent _contentAdd, _contentRemove, _contentDestroy, _contentBVH;

        public static void ShowWindow()
        {
            instance = (PathManagerWindow)GetWindow(typeof(PathManagerWindow));
            instance.titleContent = new GUIContent("Editor");
            instance.minSize = new Vector2(360, 70);
            instance.maxSize = new Vector2(360, 70);
            instance._state = 0;
        }

        public static void CloseWindow()
        {
            _Path = null;
            Selection.activeGameObject = null;
            if (instance != null)
                instance.Repaint();
        }

        void PlayStateNotifier()
        {
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        void ModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Selection.activeGameObject = null;
                instance.Close();
            }
        }

        private void Update()
        {
            if (instance == null)
                instance = (PathManagerWindow)GetWindow(typeof(PathManagerWindow));

            if (_state != 0 && _Path == null)
            { 
                _state = 0;
                instance.minSize = new Vector2(360, 148);
                instance.maxSize = new Vector2(360, 148);
                Repaint();
            }
            else if (_Path != null && Selection.activeGameObject == _Path.gameObject && _state != 1)
            {
                _state = 1;
                instance.minSize = new Vector2(360, 225);
                instance.maxSize = new Vector2(360, 225);
                Repaint();
            }

        }

        private void OnGUI()
        {
            if (_state == 0)
                DrawInitGUI();
            else if (_state == 1)
                DrawPathGUI();
        }

        [InitializeOnLoadMethod]
        private void OnEnable()
        {
            //PlayStateNotifier();
            _state = 0;

            // Common Icon
            _contentExportLevel = new GUIContent("儲存場景",EditorUtils.LoadIconGUI(EditorUtils.Icon.EXPORT));
            _contentImportLevel = new GUIContent("讀取場景",EditorUtils.LoadIconGUI(EditorUtils.Icon.IMPORT));

            _contentExportPath = new GUIContent("儲存路徑",EditorUtils.LoadIconGUI(EditorUtils.Icon.EXPORT));
            _contentImportPath = new GUIContent("讀取路徑",EditorUtils.LoadIconGUI(EditorUtils.Icon.IMPORT));

            // Level Icon
            _contentCreate = new GUIContent("創建", EditorUtils.LoadIconGUI(EditorUtils.Icon.SHARE));

            // Path Icon
            _contentAdd = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.PLUS), "增加 Node");
            _contentRemove = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.MINUS), "刪除 Node");
            _contentDestroy = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.TRASHCAN), "刪除");
            _contentBVH = new GUIContent("匯入BVH", EditorUtils.LoadIconGUI(EditorUtils.Icon.IMPORT), "匯入BVH");

        }

        void DrawInitGUI()
        {
            EditorGUILayout.LabelField("Add Path", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_contentCreate, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                CreatePath();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Level Operation", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_contentImportLevel, GUILayout.Width(100), GUILayout.Height(Button_Size)))
            {
                ImportLevel();
            }
            if (GUILayout.Button(_contentExportLevel, GUILayout.Width(100), GUILayout.Height(Button_Size)))
            {
                ExportLevel();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void CreatePath()
        {
            if (pm == null)
                pm = GameObject.Find("PathManager");
            if (pm == null)
                pm = new GameObject("PathManager");
            BezierPath path = new GameObject("Path" + pm.transform.childCount.ToString()).AddComponent<BezierPath>();

            path.transform.SetParent(pm.transform);
            path.transform.localPosition = Vector3.zero;
            path.transform.localRotation = Quaternion.identity;
            Selection.activeGameObject = path.gameObject;
        }

        void ImportLevel()
        {
            string path = EditorUtility.OpenFilePanel("Level Import", Application.dataPath, "json");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Level", "讀取Level檔案失敗", "OK");
                return;
            }

            string jsonContent;
            using (StreamReader reader = new StreamReader(path))
            {
                jsonContent = reader.ReadToEnd();
            }

            LevelData loadLevel = JsonUtility.FromJson<LevelData>(jsonContent);

            if (loadLevel == null)
            {
                EditorUtility.DisplayDialog("LevelEditor", "解析路徑檔案失敗", "OK");
                return;
            }

            if (pm == null)
                pm = GameObject.Find("PathManager");
            if (pm == null)
                pm = new GameObject("PathManager");

            while (pm.transform.childCount > 0)
            {
                DestroyImmediate(pm.transform.GetChild(0).gameObject);
            }

            PathData[] paths = loadLevel._paths;

            for (int i = 0; i < paths.Length; i++)
            {
                BezierPath bp = new GameObject("Path" + i.ToString()).AddComponent<BezierPath>();
                bp.transform.SetParent(pm.transform);
                bp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                bp._ctrlPoint = paths[i]._ctrlPoint;
                bp.nodes = new Transform[paths[i]._pointLength];

                for (int j = 0; j < paths[i]._pointLength; j++)
                {
                    // Create obj
                    GameObject node = new GameObject("Node (" + j + ")");
                    GameObject up = new GameObject("Slope_Up");
                    GameObject down = new GameObject("Slope_Down");

                    // Bind
                    node.transform.SetParent(bp.transform);
                    up.transform.SetParent(node.transform);
                    down.transform.SetParent(node.transform);

                    // Set pos
                    node.transform.position = paths[i]._ctrlPoint[j].ori;
                    up.transform.position = paths[i]._ctrlPoint[j].up;
                    down.transform.position = paths[i]._ctrlPoint[j].down;
                    bp.nodes[j] = node.transform;
                }
            }
        }

        void ExportLevel()
        {
            if (pm == null)
                pm = GameObject.Find("PathManager");
            if (pm == null)
                pm = new GameObject("PathManager");

            if (pm.transform.childCount <= 0)
            {
                EditorUtility.DisplayDialog("Level", "PathManager下沒有任何路徑資訊", "OK");
                return;
            }

            BezierPath[] paths = pm.GetComponentsInChildren<BezierPath>();

            LevelData newLevel = new LevelData(paths);

            string jsonContent = JsonUtility.ToJson(newLevel);

            string path = EditorUtility.SaveFilePanel("Level Export", Application.dataPath, pm.name, "json");
            if (!string.IsNullOrEmpty(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(jsonContent);
                }
                AssetDatabase.Refresh();
            }
        }

        void DrawPathGUI()
        {
            if (_Path == null)
                return;

            // Edit Area
            EditorGUILayout.LabelField("Edit", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_contentCreate, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                CreatePath();
            }
            if (GUILayout.Button(_contentDestroy, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                if (EditorUtility.DisplayDialog("Path", "你確定要刪除路徑", "Yes", "No"))
                {
                    DestroyImmediate(_Path.gameObject);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Node Modify", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_contentAdd, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                AddNode();
            }
            if (GUILayout.Button(_contentRemove, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                RemoveNode();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // File Area
            EditorGUILayout.LabelField("File", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_contentImportPath, GUILayout.Width(100), GUILayout.Height(Button_Size)))
            {
                ImportPath();
            }
            if (GUILayout.Button(_contentBVH, GUILayout.Width(100), GUILayout.Height(Button_Size)))
            {
                ImportBVH();
            }
            if (GUILayout.Button(_contentExportPath, GUILayout.Width(100), GUILayout.Height(Button_Size)))
            {
                ExportPath();
            }


            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }

        void AddNode()
        {
            Transform transform = _Path.transform;
            int nodeCount = transform.childCount;

            GameObject goNode = new GameObject("Node (" + nodeCount + ")");
            GameObject up = new GameObject("Slope_Up");
            GameObject down = new GameObject("Slope_Down");

            goNode.transform.SetParent(transform);

            _Path.nodes = new Transform[nodeCount + 1];
            for (int i = 0; i < transform.childCount; i++)
            {
                _Path.nodes[i] = transform.GetChild(i);
            }

            Vector3 dir = new Vector3(5f, 0f, 0f);
            if (_Path.simulateBezierPath.Count > 1)
                dir = Vector3.Normalize(_Path.simulateBezierPath[_Path.simulateBezierPath.Count - 1] - _Path.simulateBezierPath[_Path.simulateBezierPath.Count - 2]) * 5;

            _Path.nodes[nodeCount].position = _Path.nodes[nodeCount - 1].position + dir;
            up.transform.SetParent(_Path.nodes[nodeCount].transform);
            down.transform.SetParent(_Path.nodes[nodeCount].transform);
            _Path.nodes[nodeCount].GetChild(0).position = _Path.nodes[nodeCount].position - dir / 5;
            _Path.nodes[nodeCount].GetChild(1).position = _Path.nodes[nodeCount].position + dir / 5;
            _Path._ctrlPoint.Add(new BezierPoint(_Path.nodes[nodeCount].GetChild(0).position, _Path.nodes[nodeCount].GetChild(1).position, _Path.nodes[nodeCount].position));
            _Path.nodeChange = true;

            _Path.Preview();
        }

        void RemoveNode()
        {
            Transform transform = _Path.transform;
            int nodeCount = transform.childCount - 1;

            if (nodeCount <= 0)
                return;

            DestroyImmediate(transform.GetChild(nodeCount).gameObject);
            _Path._ctrlPoint.RemoveAt(nodeCount);
            _Path.nodes = new Transform[nodeCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                _Path.nodes[i] = transform.GetChild(i);
            }
            _Path.nodeChange = true;
            _Path.Preview();
        }

        void ImportPath()
        {
            string path = EditorUtility.OpenFilePanel("File Import", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Editor", "讀取路徑檔案失敗", "OK");
                return;
            }

            string jsonContent;
            using (StreamReader reader = new StreamReader(path))
            {
                jsonContent = reader.ReadToEnd();
            }

            PathData data = JsonUtility.FromJson<PathData>(jsonContent);

            if (data == null)
            {
                EditorUtility.DisplayDialog("Editor", "讀取檔案失敗", "OK");
                return;
            }

            while (_Path.transform.childCount > 0)
            {
                DestroyImmediate(_Path.transform.GetChild(0).gameObject);
            }

            _Path._ctrlPoint.Clear();
            _Path._ctrlPoint = data._ctrlPoint;
            _Path.nodes = new Transform[_Path._ctrlPoint.Count];

            // Create Path
            for (int i = 0; i < _Path._ctrlPoint.Count; i++)
            {
                // Create obj
                GameObject node = new GameObject("Node (" + i + ")");
                GameObject up = new GameObject("Slope_Up");
                GameObject down = new GameObject("Slope_Down");

                // Bind
                node.transform.SetParent(_Path.transform);
                up.transform.SetParent(node.transform);
                down.transform.SetParent(node.transform);

                // Set pos
                node.transform.position = _Path._ctrlPoint[i].ori;
                up.transform.position = _Path._ctrlPoint[i].up;
                down.transform.position = _Path._ctrlPoint[i].down;
                _Path.nodes[i] = node.transform;
            }
        }

        void ExportPath()
        {

            if (_Path._ctrlPoint.Count <= 0)
            {
                EditorUtility.DisplayDialog("Editor", "無路徑資訊", "OK");
                return;
            }

            PathData file = new PathData(_Path._ctrlPoint);

            string jsonContent = JsonUtility.ToJson(file);
            string path = EditorUtility.SaveFilePanel("File Export", Application.dataPath, _Path.name, "json");
            if (!string.IsNullOrEmpty(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(jsonContent);
                }
                AssetDatabase.Refresh();
            }
        }

        void ImportBVH()
        {
            string path = EditorUtility.OpenFilePanel("File Import", Application.dataPath, "bvh");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Editor", "讀取BVH檔案失敗", "OK");
                return;
            }
            GameObject bonePrefab = Resources.Load<GameObject>("BoneGenerator");
            GameObject bone = Instantiate(bonePrefab);
            BoneGenerator boneGenerator = bone.GetComponent<BoneGenerator>();

            boneGenerator.Parse(path);
            boneGenerator.SetPath(_Path.simulateBezierPath);
            boneGenerator.GenerateJointBone();
            boneGenerator.Play();
        }
    }
}
