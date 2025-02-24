using PurrNet;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.NetickBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        [SerializeField] private int _port;

        private NetworkManager _activeNetworkManager;

        protected override void Initialize()
        {
            base.Initialize();
            _activeNetworkManager = Instantiate(_networkManagerPrefab);
        }

        protected override void StressTest(StressTestEssential stressTestEssential)
        {
            for (int i = 0; i < stressTestEssential.SpawnCount; i++)
            {
                var obj = Instantiate(stressTestEssential.Prefab);
                _activeNetworkManager.Spawn(obj);
            }
        }

        protected override void StartServer()
        {
            _activeNetworkManager.StartServer();
        }

        protected override void StartClient()
        {
            _activeNetworkManager.StartClient();
        }

        protected override void UpdateNetworkStats()
        {
            if (_activeNetworkManager == null) return;

            if (_activeNetworkManager.isOffline) return;

            _textLatency.SetText("Latency: {0}ms\n", (float)_activeNetworkManager.tickModule.rtt  * 1_000);
        }
    }
}