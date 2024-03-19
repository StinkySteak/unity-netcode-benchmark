using Fusion;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FusionBenchmark
{
    public class SelfNetworkedSinRandomBehaviour : NetworkBehaviour
    {
        [Networked] private Vector3 _serverPosition { get; set; }

        [SerializeField] private BehaviourConfig _config;
        private SinRandomMoveWrapper _wrapper;


        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);

            if (!Object.HasStateAuthority) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                transform.position = _serverPosition;
                return;
            }

            _wrapper.NetworkUpdate(transform);
            _serverPosition = transform.position;
        }
    }
}