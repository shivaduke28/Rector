using System.Linq;
using Rector.NodeBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rector
{
    [AddComponentMenu("Rector/BG Scene")]
    public sealed class BGScene : MonoBehaviour
    {
        [SerializeField] NodeBehaviour[] nodeBehaviours;
        public NodeBehaviour[] NodeBehaviours => nodeBehaviours;

        public void RetrieveNodeBehaviours()
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            nodeBehaviours = rootObjects.SelectMany(go => go.GetComponentsInChildren<NodeBehaviour>()).ToArray();
        }

        void Reset()
        {
            RetrieveNodeBehaviours();
        }
    }
}
