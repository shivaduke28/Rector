using Rector.Vfx;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(VfxInputSlotBehaviour))]
    public class VfxInputBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // FIXME: VFXGraphのエディタを開かないとうまく反映できない
            if (GUILayout.Button("Reset Properties"))
            {
                var vfxView = (VfxInputSlotBehaviour) target;
                var props = VfxAssetReader.GetPropertyInputs(vfxView.VisualEffect);
                vfxView.ResetProperties(props);
                EditorUtility.SetDirty(vfxView);
            }

            // FIXME: VFXGraphのエディタを開かないとうまく反映できない
            if (GUILayout.Button("Reset Events"))
            {
                var vfxView = (VfxInputSlotBehaviour) target;
                vfxView.ResetEvents(VfxAssetReader.GetEventNames(vfxView.VisualEffect));
                EditorUtility.SetDirty(vfxView);
            }
        }
    }
}
