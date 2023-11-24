using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class SineMoveYBehaviour : NetworkBehaviour
    {
        [SerializeField] private SinMoveYWrapper _behaviour;

        private void Reset()
        {
            _behaviour = SinMoveYWrapper.CreateDefault();
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