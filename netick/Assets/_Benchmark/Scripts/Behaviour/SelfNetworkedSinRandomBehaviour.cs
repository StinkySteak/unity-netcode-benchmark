using Netick;
using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class SelfNetworkedSinRandomBehaviour : NetworkBehaviour
    {
        [Networked] private Vector3 _serverPosition { get; set; }

        [SerializeField] private BehaviourConfig _config;
        private SinRandomMoveWrapper _wrapper;


        public override void NetworkStart()
        {
            if (!Object.IsServer) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        public override void NetworkFixedUpdate()
        {
            if (!Object.IsServer)
            {
                transform.position = _serverPosition;
                return;
            }

            _wrapper.NetworkUpdate(transform);
            _serverPosition = transform.position;
        }
    }
}