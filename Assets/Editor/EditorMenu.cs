using UnityEditor;

namespace SteeringBehavior.LevelEditor
{
    static class SteeringMenu
    {
        [MenuItem("GUI/Path Editor")]
        public static void ShowEditorWindow()
        {
            PathManagerWindow.ShowWindow();
        }
    }
}
