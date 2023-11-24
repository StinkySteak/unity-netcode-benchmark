using Mirror;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.MirrorBenchmark
{
    public class SineMoveYBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private SinMoveYWrapper _wrapper;

        public override void OnStartServer()
        {
            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart();
        }

        private void FixedUpdate()
        {
            _wrapper.NetworkUpdate(transform);
        }
    }
}
