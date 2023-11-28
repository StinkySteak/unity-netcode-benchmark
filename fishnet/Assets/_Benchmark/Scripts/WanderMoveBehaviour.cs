using FishNet.Object;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class WanderMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _config;
        private WanderMoveWrapper _wrapper;

        public override void OnStartNetwork()
        {
            if (!IsServer) return;

            _config.ApplyConfig(ref _wrapper);
            _wrapper.NetworkStart(transform);
            
            TimeManager.OnTick += OnTick;
        }

        private void OnTick()
        {
            _wrapper.NetworkUpdate(transform);
        }
    }
}