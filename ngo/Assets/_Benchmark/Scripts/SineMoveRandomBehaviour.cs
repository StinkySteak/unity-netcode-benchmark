using StinkySteak.NetcodeBenchmark;
using Unity.Netcode;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
{
    public class SineMoveRandomBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private SinRandomMoveWrapper _wrapper;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
            NetworkManager.NetworkTickSystem.Tick += OnTick;
        }

        private void OnTick()
        {
            if (!IsServer) return;

            _wrapper.NetworkUpdate(transform);
        }
    }
}
