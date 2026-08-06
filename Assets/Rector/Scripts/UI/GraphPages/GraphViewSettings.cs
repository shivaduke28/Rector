using R3;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// グラフの見え方の設定。ノードやグループの構造とは関係しないものを置く。
    /// </summary>
    /// <remarks>
    /// 値は PlayerPrefs に覚える (AudioInputDeviceManager と同じ流儀)。
    /// </remarks>
    public sealed class GraphViewSettings
    {
        const string FollowSelectedNodePrefsKey = "Rector_FollowSelectedNode";

        readonly ReactiveProperty<bool> followSelectedNode;

        /// <summary>
        /// 選択中のノードに合わせてグラフを動かすか。
        /// </summary>
        /// <remarks>
        /// 今の実装は選択のたびにノードを画面中央へ持ってくる。手で見たい位置に置いたまま
        /// ノードを辿りたいときは邪魔になるので切れるようにしている。
        /// </remarks>
        public ReadOnlyReactiveProperty<bool> FollowSelectedNode => followSelectedNode;

        public GraphViewSettings()
        {
            // PlayerPrefs に bool はないので 0/1 で持つ
            followSelectedNode = new ReactiveProperty<bool>(PlayerPrefs.GetInt(FollowSelectedNodePrefsKey, 1) != 0);
        }

        public void SetFollowSelectedNode(bool value)
        {
            if (value == followSelectedNode.Value) return;

            followSelectedNode.Value = value;
            PlayerPrefs.SetInt(FollowSelectedNodePrefsKey, value ? 1 : 0);
        }

        public void ToggleFollowSelectedNode() => SetFollowSelectedNode(!followSelectedNode.Value);
    }
}
