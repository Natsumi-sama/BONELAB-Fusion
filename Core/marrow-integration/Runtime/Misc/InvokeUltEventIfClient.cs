﻿using System;

using UnityEngine;

using UltEvents;

#if MELONLOADER
using MelonLoader;
using LabFusion.Network;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Invoke Ult Event If Client")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfClient : MonoBehaviour {
#if MELONLOADER
        public InvokeUltEventIfClient(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.IsClient && holder != null)
                holder.Invoke();
        }
#endif
    }
}
