using System.Collections.Generic;
using System.Linq;
using Rector.NodeBehaviours;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rector.Editor
{
    public static class NodeBehaviourGUIDInitializer
    {
        [MenuItem("Tools/Rector/Initialize NodeBehaviour GUIDs")]
        public static void InitializeNodeBehaviourGUIDs()
        {
            var usedGuids = new HashSet<string>();
            var totalEmptyFixed = 0;
            var totalDuplicatesFixed = 0;

            // 空GUIDの修正と重複の修正は別々にシーンを開き直すので、両方で直したシーンは
            // 2回通る。数えたいのは保存したシーンの数なのでパスで集める
            var modifiedScenePaths = new HashSet<string>();

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Rector" });
            var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                // First pass: collect all GUIDs from all scenes
                foreach (var sceneGuid in sceneGuids)
                {
                    var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    var rootObjects = scene.GetRootGameObjects();
                    foreach (var rootObject in rootObjects)
                    {
                        var nodeBehaviours = rootObject.GetComponentsInChildren<NodeBehaviour>(true);
                        foreach (var nodeBehaviour in nodeBehaviours)
                        {
                            if (nodeBehaviour.Guid != System.Guid.Empty)
                            {
                                usedGuids.Add(nodeBehaviour.Guid.ToString());
                            }
                        }
                    }
                }

                // Second pass: fix empty GUIDs
                foreach (var sceneGuid in sceneGuids)
                {
                    var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    var sceneModified = false;

                    var rootObjects = scene.GetRootGameObjects();
                    foreach (var rootObject in rootObjects)
                    {
                        var nodeBehaviours = rootObject.GetComponentsInChildren<NodeBehaviour>(true);
                        foreach (var nodeBehaviour in nodeBehaviours)
                        {
                            if (nodeBehaviour.Guid == System.Guid.Empty)
                            {
                                string newGuid;
                                do
                                {
                                    newGuid = System.Guid.NewGuid().ToString();
                                } while (usedGuids.Contains(newGuid));

                                usedGuids.Add(newGuid);
                                nodeBehaviour.RegenerateGuid();
                                sceneModified = true;
                                totalEmptyFixed++;
                                Debug.Log($"Initialized GUID for NodeBehaviour '{nodeBehaviour.name}' in scene '{scene.name}'");
                            }
                        }
                    }

                    if (sceneModified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        modifiedScenePaths.Add(scenePath);
                        Debug.Log($"Saved scene: {scenePath}");
                    }
                }

                // Third pass: fix duplicates
                usedGuids.Clear();
                foreach (var sceneGuid in sceneGuids)
                {
                    var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    var sceneModified = false;

                    var rootObjects = scene.GetRootGameObjects();
                    foreach (var rootObject in rootObjects)
                    {
                        var nodeBehaviours = rootObject.GetComponentsInChildren<NodeBehaviour>(true);
                        foreach (var nodeBehaviour in nodeBehaviours)
                        {
                            var guidString = nodeBehaviour.Guid.ToString();
                            if (!usedGuids.Add(guidString))
                            {
                                // Duplicate found
                                string newGuid;
                                do
                                {
                                    newGuid = System.Guid.NewGuid().ToString();
                                } while (usedGuids.Contains(newGuid));

                                usedGuids.Add(newGuid);
                                nodeBehaviour.RegenerateGuid();
                                sceneModified = true;
                                totalDuplicatesFixed++;
                                Debug.Log($"Fixed duplicate GUID for NodeBehaviour '{nodeBehaviour.name}' in scene '{scene.name}'");
                            }
                        }
                    }

                    if (sceneModified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        modifiedScenePaths.Add(scenePath);
                        Debug.Log($"Saved scene: {scenePath}");
                    }
                }

                // Process prefabs
                var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Rector" });
                foreach (var prefabGuid in prefabGuids)
                {
                    var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        var nodeBehaviours = prefab.GetComponentsInChildren<NodeBehaviour>(true);
                        foreach (var nodeBehaviour in nodeBehaviours)
                        {
                            if (nodeBehaviour.Guid == System.Guid.Empty)
                            {
                                string newGuid;
                                do
                                {
                                    newGuid = System.Guid.NewGuid().ToString();
                                } while (usedGuids.Contains(newGuid));

                                usedGuids.Add(newGuid);
                                nodeBehaviour.RegenerateGuid();
                                EditorUtility.SetDirty(nodeBehaviour);
                                totalEmptyFixed++;
                                Debug.Log($"Initialized GUID for NodeBehaviour '{nodeBehaviour.name}' in prefab '{prefabPath}'");
                            }
                        }
                    }
                }

                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (originalSceneSetup != null && originalSceneSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                }
            }

            var totalFixed = totalEmptyFixed + totalDuplicatesFixed;
            if (totalFixed > 0)
            {
                Debug.Log($"NodeBehaviour GUID initialization complete. Fixed {totalFixed} NodeBehaviours " +
                          $"({totalEmptyFixed} empty, {totalDuplicatesFixed} duplicates) in {modifiedScenePaths.Count} scenes.");
            }
            else
            {
                Debug.Log("All NodeBehaviours already have unique GUIDs.");
            }
        }
    }
}
