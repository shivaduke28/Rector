using Rector.Vfx;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    [CustomEditor(typeof(VfxInputSlotBehaviour))]
    [CanEditMultipleObjects]
    public sealed class VfxInputBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // VFXGraphのエディタを一度開かないと反映されない。VFXアセットの内部状態が
            // エディタウィンドウ側で構築されるため
            if (GUILayout.Button("Reset Properties and Events"))
            {
                foreach (var t in targets)
                {
                    var vfxView = (VfxInputSlotBehaviour)t;
                    var props = VfxAssetReader.GetPropertyInputs(vfxView.VisualEffect);
                    vfxView.ResetProperties(props);
                    vfxView.ResetEvents(VfxAssetReader.GetEventNames(vfxView.VisualEffect));
                    EditorUtility.SetDirty(vfxView);
                }
            }
        }
    }
}
