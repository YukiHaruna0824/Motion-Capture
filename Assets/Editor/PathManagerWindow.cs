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

        const float Button_Size = 48f;
        private int _state;

        // Common Icon
        private GUIContent _contentExport, _contentImport;
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
                instance.minSize = new Vector2(360, 70);
                instance.maxSize = new Vector2(360, 70);
                Repaint();
            }
            else if (_Path != null && Selection.activeGameObject == _Path.gameObject && _state != 1)
            {
                _state = 1;
                instance.minSize = new Vector2(360, 305);
                instance.maxSize = new Vector2(360, 305);
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
            _contentExport = new GUIContent("存檔", EditorUtils.LoadIconGUI(EditorUtils.Icon.EXPORT));
            _contentImport = new GUIContent("讀檔", EditorUtils.LoadIconGUI(EditorUtils.Icon.IMPORT));

            // Level Icon
            _contentCreate = new GUIContent("創建", EditorUtils.LoadIconGUI(EditorUtils.Icon.SHARE));

            // Path Icon
            _contentAdd = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.PLUS), "增加 Node");
            _contentRemove = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.MINUS), "刪除 Node");
            _contentDestroy = new GUIContent(EditorUtils.LoadIconGUI(EditorUtils.Icon.TRASHCAN), "刪除");
            _contentBVH = new GUIContent("匯入BVH", EditorUtils.LoadIconGUI(EditorUtils.Icon.INFORMATION), "匯入BVH");

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
        }

        void CreatePath()
        {
            GameObject pm = GameObject.Find("PathManager");
            if (pm == null)
                pm = new GameObject("PathManager");
            BezierPath path = new GameObject("Path" + pm.transform.childCount.ToString()).AddComponent<BezierPath>();

            path.transform.SetParent(pm.transform);
            path.transform.localPosition = Vector3.zero;
            path.transform.localRotation = Quaternion.identity;
            Selection.activeGameObject = path.gameObject;
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

            if (GUILayout.Button(_contentImport, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                //ImportPath();
            }
            if (GUILayout.Button(_contentBVH, GUILayout.Width(90), GUILayout.Height(Button_Size)))
            {
                //ImportBVH();
            }
            if (GUILayout.Button(_contentExport, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                //ExportPath();
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
    }
}
