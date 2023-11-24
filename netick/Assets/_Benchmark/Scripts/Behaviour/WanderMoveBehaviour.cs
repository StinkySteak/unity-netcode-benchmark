using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class WanderMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private WanderMoveWrapper _behaviour;

        private void Reset()
        {
            _behaviour = WanderMoveWrapper.CreateDefault();
        }

        public override void NetworkStart()
        {
            if (!Object.IsServer) return;

            _behaviour.NetworkStart();
        }

        public override void NetworkFixedUpdate()
        {
            if (!Object.IsServer) return;

            _behaviour.NetworkUpdate(transform);
        }
    }
}