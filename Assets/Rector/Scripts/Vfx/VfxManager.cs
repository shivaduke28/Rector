using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Rector.Vfx
{
    public sealed class VfxManager : IInitializable, IDisposable
    {
        readonly VfxSettings vfxSettings;
        readonly List<VfxNodeBehaviour> vfxNodeBehaviours = new();

        bool initialized;

        public VfxManager(VfxSettings vfxSettings)
        {
            this.vfxSettings = vfxSettings;
        }

        // 初期化
        public void Initialize()
        {
            initialized = true;

            foreach (var vfxNodeBehaviour in vfxSettings.vfxNodeBehaviours)
            {
                var behaviour = Object.Instantiate(vfxNodeBehaviour);
                behaviour.name = vfxNodeBehaviour.name;
                vfxNodeBehaviours.Add(behaviour);
            }
        }

        public IEnumerable<VfxNodeBehaviour> GetAllVfx()
        {
            Assert.IsTrue(initialized);
            return vfxNodeBehaviours;
        }

        public void Dispose()
        {
            foreach (var vfxNodeBehaviour in vfxNodeBehaviours)
            {
                Object.Destroy(vfxNodeBehaviour);
            }

            vfxNodeBehaviours.Clear();
        }
    }
}
