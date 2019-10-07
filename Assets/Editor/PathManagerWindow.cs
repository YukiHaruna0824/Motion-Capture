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
                CreateLevel();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void CreateLevel()
        {
           // new GameObject("Level" + FindObjectsOfType<VisualLevel>().Length).AddComponent<VisualLevel>();
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
                //CreatePath();
            }
            if (GUILayout.Button(_contentDestroy, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                if (EditorUtility.DisplayDialog("Path", "你確定要刪除路徑", "Yes", "No"))
                {

                    DestroyImmediate(_Path.gameObject);
                    //EditorApplication.update -= DrawPathPos;
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
                //AddNode();
            }
            if (GUILayout.Button(_contentRemove, GUILayout.Width(Button_Size), GUILayout.Height(Button_Size)))
            {
                //RemoveNode();
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

            // Simulate Area
            EditorGUILayout.LabelField("Simulate", EditorUtils.titleStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();


            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }
    }
}
