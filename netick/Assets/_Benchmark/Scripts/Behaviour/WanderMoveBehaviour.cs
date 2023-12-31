using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class WanderMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _behaviourConfig;
        private WanderMoveWrapper _behaviour;

        public override void NetworkStart()
        {
            if (!Object.IsServer) return;

            _behaviourConfig.ApplyConfig(ref _behaviour);
            _behaviour.NetworkStart(transform);
        }

        public override void NetworkFixedUpdate()
        {
            if (!Object.IsServer) return;

            _behaviour.NetworkUpdate(transform);
        }
    }
}