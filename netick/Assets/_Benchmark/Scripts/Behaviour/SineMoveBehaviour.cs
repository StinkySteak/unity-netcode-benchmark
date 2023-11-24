using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class SineMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private SinRandomMoveWrapper _behaviour;

        private void Reset()
        {
            _behaviour = SinRandomMoveWrapper.CreateDefault();
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