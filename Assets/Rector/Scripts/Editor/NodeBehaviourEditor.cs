using Rector.NodeBehaviours;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(NodeBehaviour), true)]
    public class NodeBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var nodeBehaviour = (NodeBehaviour)target;
            if (GUILayout.Button("Retrieve Components"))
            {
                nodeBehaviour.RetrieveComponents();
                EditorUtility.SetDirty(nodeBehaviour);
            }
        }
    }
}
