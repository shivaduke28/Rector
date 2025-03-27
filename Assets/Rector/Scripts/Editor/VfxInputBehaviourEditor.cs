using Rector.NodeComponents;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(VfxInputBehaviour))]
    public sealed class VfxInputBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // FIXME: VFXGraphのエディタを開かないとうまく反映できない
            if (GUILayout.Button("Reset Properties"))
            {
                var vfxView = (VfxInputBehaviour) target;
                var props = VfxAssetReader.GetPropertyInputs(vfxView.VisualEffect);
                vfxView.ResetProperties(props);
                EditorUtility.SetDirty(vfxView);
            }

            // FIXME: VFXGraphのエディタを開かないとうまく反映できない
            if (GUILayout.Button("Reset Events"))
            {
                var vfxView = (VfxInputBehaviour) target;
                vfxView.ResetEvents(VfxAssetReader.GetEventNames(vfxView.VisualEffect));
                EditorUtility.SetDirty(vfxView);
            }
        }
    }
}
