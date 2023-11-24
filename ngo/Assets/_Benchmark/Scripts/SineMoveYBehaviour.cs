using StinkySteak.NetcodeBenchmark;
using Unity.Netcode;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
{
    public class SineMoveYBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private SinMoveYWrapper _wrapper;

        public override void OnNetworkSpawn()
        {
            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart();

            NetworkManager.NetworkTickSystem.Tick += OnTick;
        }

        private void OnTick()
        {
            if (!IsServer) return;

            _wrapper.NetworkUpdate(transform);
        }
    }
}