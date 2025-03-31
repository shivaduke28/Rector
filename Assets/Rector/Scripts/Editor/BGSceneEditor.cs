using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(BGScene))]
    public class BGSceneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Retrieve Node Behaviours"))
            {
                foreach (var t in targets)
                {
                    var bgScene = (BGScene)t;
                    bgScene.RetrieveNodeBehaviours();
                    EditorUtility.SetDirty(bgScene);
                }
            }
        }
    }
}
