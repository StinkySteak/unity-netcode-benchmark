using StinkySteak.NetcodeBenchmark;
using Unity.Netcode;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
{
    public class SelfNetworkedSinRandomBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;

        private NetworkVariable<Vector3> _serverPosition = new NetworkVariable<Vector3>();
        private SinRandomMoveWrapper _wrapper;
        
        public override void OnNetworkSpawn()
        {
            NetworkManager.NetworkTickSystem.Tick += OnTick;

            if (!IsServer) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        private void OnTick()
        {
            if (!IsServer)
            {
                transform.position = _serverPosition.Value;
                return;
            }

            _wrapper.NetworkUpdate(transform);
            _serverPosition.Value = transform.position;
        }
    }
}