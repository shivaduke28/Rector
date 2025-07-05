using System;
using System.Collections.Generic;

namespace Rector.NodeBehaviours
{
    public sealed class NodeBehaviourProxyRepository : IDisposable
    {
        readonly Dictionary<Guid, NodeBehaviourProxy> proxies = new();

        public NodeBehaviourProxy GetOrCreateProxy(NodeBehaviour nodeBehaviour)
        {
            if (nodeBehaviour == null || nodeBehaviour.Guid == Guid.Empty)
            {
                throw new ArgumentException("NodeBehaviour must have a valid GUID");
            }

            var guid = nodeBehaviour.Guid;

            if (proxies.TryGetValue(guid, out var existingProxy))
            {
                // Update existing proxy with new NodeBehaviour instance
                existingProxy.UpdateNodeBehaviour(nodeBehaviour);
                return existingProxy;
            }

            // Create new proxy
            var newProxy = new NodeBehaviourProxy(guid, nodeBehaviour);
            proxies[guid] = newProxy;
            return newProxy;
        }

        public void UpdateProxy(Guid guid, NodeBehaviour nodeBehaviour)
        {
            if (proxies.TryGetValue(guid, out var proxy))
            {
                proxy.UpdateNodeBehaviour(nodeBehaviour);
            }
        }

        void Clear()
        {
            proxies.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
