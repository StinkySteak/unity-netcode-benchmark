using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class SineMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _behaviourConfig;
        private SinRandomMoveWrapper _behaviour;

        public override void NetworkStart()
        {
            if (!Object.IsServer) return;

            _behaviourConfig.ApplyConfig(ref _behaviour);
            _behaviour.NetworkStart();
        }

        public override void NetworkFixedUpdate()
        {
            if (!Object.IsServer) return;

            _behaviour.NetworkUpdate(transform);
        }
    }
}