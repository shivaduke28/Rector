using Rector.NodeBehaviours;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(NodeBehaviour), true)]
    [CanEditMultipleObjects]
    public class NodeBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Retrieve Components"))
            {
                foreach (var t in targets)
                {
                    var nodeBehaviour = (NodeBehaviour)t;
                    nodeBehaviour.RetrieveComponents();
                    EditorUtility.SetDirty(nodeBehaviour);
                }
            }
        }
    }
}
