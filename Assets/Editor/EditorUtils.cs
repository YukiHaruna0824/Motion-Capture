using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SteeringBehavior.LevelEditor
{
    [InitializeOnLoad]
    public static class EditorUtils
    {

        public static GUIStyle titleStyle;

        public static Color guiDefaultColor;
        public static Color guiBlackColor;

        public enum Icon
        {
            EXPORT,
            IMPORT,

            CHECKMARK,
            CROSS,

            PLUS,
            MINUS,

            LOCKED,
            UNLOCKED,

            QUESTION,
            INFORMATION,
            GEAR,
            WRENCH,
            SHARE,
            TRASHCAN,

            FORWARD,
            BACKWARD,
            PAUSE,
            STOP,
            FASTFORWARD,
            REWIND,
            NEXT,
            PREVIOUS,
            CAMERA
        }

        static EditorUtils()
        {
            Init();
        }

        static void Init()
        {
            guiDefaultColor = GUI.color;
            guiBlackColor = new Color(0f, 0f, 0f, 0.55f);

            Texture2D titleBg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Res/levelTitleBg.png");
            Font titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Res/BowlbyOne-Regular.ttf");

            titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 16;
            titleStyle.normal.background = titleBg;
            titleStyle.normal.textColor = Color.white;
            titleStyle.font = titleFont;
        }

        public static void NewScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        public static void CleanScene()
        {
            GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject go in objects)
            {
                UnityEngine.GameObject.DestroyImmediate(go);
            }
        }

        public static Texture LoadIconGUI(Icon icon)
        {
            return AssetDatabase.LoadAssetAtPath<Texture>("Assets/Res/" + icon.ToString().ToLower() + ".png");
        }

        public static string RelativePath(string path)
        {
            if (path.StartsWith(Application.dataPath))
            {
                return "Assets" + path.Substring(Application.dataPath.Length);
            }

            return path;
        }
    }
}
