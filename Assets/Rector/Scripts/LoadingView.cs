using UnityEngine;

namespace Rector
{
    public sealed class LoadingView : MonoBehaviour
    {
        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}
