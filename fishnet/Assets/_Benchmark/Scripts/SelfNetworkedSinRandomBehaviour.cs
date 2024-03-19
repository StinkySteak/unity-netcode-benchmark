using FishNet.Object;
using FishNet.Object.Synchronizing;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class SelfNetworkedSinRandomBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        [SyncVar][HideInInspector] private Vector3 _serverPosition;
        private SinRandomMoveWrapper _wrapper;

        public override void OnStartNetwork()
        {
            TimeManager.OnTick += OnTick;

            if (!IsServer) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        private void OnTick()
        {
            if (!IsServer)
            {
                transform.position = _serverPosition;
                return;
            }

            _serverPosition = transform.position;
            _wrapper.NetworkUpdate(transform);
        }
    }
}