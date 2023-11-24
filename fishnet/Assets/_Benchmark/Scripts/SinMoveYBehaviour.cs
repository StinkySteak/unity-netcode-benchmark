using FishNet.Object;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class SinMoveYBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private SinMoveYWrapper _wrapper;

        public override void OnStartNetwork()
        {
            if (!IsServer) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart();
            
            TimeManager.OnTick += OnTick;
        }

        private void OnTick()
        {
            _wrapper.NetworkUpdate(transform);
        }
    }
}