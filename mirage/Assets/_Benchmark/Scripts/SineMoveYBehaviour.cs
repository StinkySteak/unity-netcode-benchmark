using Mirage;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.MirrorBenchmark
{
    public class SineMoveYBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private SinMoveYWrapper _wrapper;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        public  void OnStartServer()
        {
            if (IsClient) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
        }

        private void FixedUpdate()
        {
            if (IsClient) return;

            _wrapper.NetworkUpdate(transform);
        }
    }
}
