using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rector.Editor
{
    /// <summary>
    /// ビルド前後で Hierarchy のシーン構成を保つ。
    ///
    /// com.unity.pipeline (0.4.0-exp.1) の PipelineRuntimeBuildProcessor は、ビルド前処理で
    /// RuntimePipelineManager を探すために Build Settings の全シーンを Additive で開くが、
    /// 一度も閉じない。そのためエディタでビルドすると Base / Room / White が Hierarchy に
    /// 開きっぱなしになり、手で unload する必要がある (issue #64)。
    /// PackageCache は再取得で書き戻されるので、こちら側で後始末する。
    ///
    /// ビルドが失敗すると後処理が呼ばれないため、そのケースでは開いたままになる。
    /// 未保存の Untitled シーンしか開いていない場合はスナップショットが空になるので何もしない。
    /// </summary>
    public sealed class BuildSceneSetupRestorer : IPreprocessBuildWithContext, IPostprocessBuildWithContext
    {
        // スナップショットは PipelineRuntimeBuildProcessor (callbackOrder = 0) が
        // シーンを開く前に取る必要がある。Unity 6000.3 ではあちらも
        // IPreprocessBuildWithContext を実装するので、同一インターフェイス内での
        // callbackOrder の順序保証だけに頼ればよい。
        public int callbackOrder => int.MinValue;

        static SceneSetup[] setupBeforeBuild;

        public void OnPreprocessBuild(BuildCallbackContext context)
        {
            setupBeforeBuild = EditorSceneManager.GetSceneManagerSetup();
        }

        public void OnPostprocessBuild(BuildCallbackContext context)
        {
            var setup = setupBeforeBuild;
            setupBeforeBuild = null;
            if (setup == null || setup.Length == 0) return;

            var before = new Dictionary<string, SceneSetup>();
            foreach (var s in setup)
            {
                before[s.path] = s;
            }

            // 元から開いていたシーンには触らない。開き直すと未保存の変更やエディタの
            // 状態が飛ぶので、ビルドが開いたものだけを閉じる。
            var closed = new List<string>();
            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                // path は閉じたあとには読めなくなるので先に控える。
                var path = scene.path;
                if (!before.TryGetValue(path, out var s))
                {
                    EditorSceneManager.CloseScene(scene, true);
                    closed.Add(path);
                }
                else if (scene.isLoaded && !s.isLoaded)
                {
                    // Hierarchy には並んでいたが unload されていた状態に戻す。
                    EditorSceneManager.CloseScene(scene, false);
                    closed.Add(path);
                }
            }

            if (closed.Count == 0) return;

            // Additive で開かれた影響でアクティブシーンが変わっていることがある。
            var active = setup.FirstOrDefault(s => s.isActive);
            if (active != null)
            {
                var scene = SceneManager.GetSceneByPath(active.path);
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.SetActiveScene(scene);
                }
            }

            Debug.Log($"Closed {closed.Count} scene(s) opened by the build: {string.Join(", ", closed)}");
        }
    }
}
