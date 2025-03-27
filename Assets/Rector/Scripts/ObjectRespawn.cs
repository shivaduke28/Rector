using UnityEngine;

namespace Rector
{
    public sealed class ObjectRespawn : MonoBehaviour
    {
        public const float RespawnHeight = -20f;

        Vector3 initialPosition;
        Quaternion initialRotation;
        Transform trans;
        Rigidbody rb;

        void Start()
        {
            trans = transform;
            initialPosition = trans.position;
            initialRotation = trans.rotation;
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            CheckRespawn();
        }

        void CheckRespawn()
        {
            if (trans.position.y < RespawnHeight)
            {
                trans.position = initialPosition;
                trans.rotation = initialRotation;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
