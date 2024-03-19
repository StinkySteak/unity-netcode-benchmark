using Mirror;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.MirrorBenchmark
{
    public class SelfNetworkedSinRandomBehaviour : NetworkBehaviour
    {
        [SyncVar] private Vector3 _serverPosition;
        [SerializeField] private BehaviourConfig _config;
        private SinRandomMoveWrapper _wrapper;

        public override void OnStartServer()
        {
            if (isClient) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        private void FixedUpdate()
        {
            if (isClient)
            {
                transform.position = _serverPosition;
                return;
            }

            _wrapper.NetworkUpdate(transform);
            _serverPosition = transform.position;
        }
    }
}