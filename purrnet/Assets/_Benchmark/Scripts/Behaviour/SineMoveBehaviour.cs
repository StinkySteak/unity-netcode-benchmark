using PurrNet;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class SineMoveBehaviour : NetworkBehaviour
    {
        [SerializeField] private BehaviourConfig _behaviourConfig;
        private SinRandomMoveWrapper _behaviour;

        private void Start()
        {
            if (!isServer) return;

            _behaviourConfig.ApplyConfig(ref _behaviour);
            _behaviour.NetworkStart(transform);
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            _behaviour.NetworkUpdate(transform);
        }
    }
}