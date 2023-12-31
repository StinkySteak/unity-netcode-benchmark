using StinkySteak.NetcodeBenchmark;
using Unity.Netcode;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        private NetworkManager _networkManager;

        protected override void Initialize()
        {
            base.Initialize();

            _networkManager = Instantiate(_networkManagerPrefab);

            RegisterPrefabs(new StressTestEssential[] { _test_1, _test_2, _test_3 });
        }

        private void RegisterPrefabs(StressTestEssential[] stressTest)
        {
            for (int i = 0; i < stressTest.Length; i++)
            {
                _networkManager.AddNetworkPrefab(stressTest[i].Prefab);
            }
        }

        protected override void StartClient()
        {
            _networkManager.StartClient();
        }

        protected override void StartServer()
        {
            _networkManager.StartServer();
        }

        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                GameObject go = Instantiate(stressTest.Prefab);
                go.GetComponent<NetworkObject>().Spawn();
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            if (!_networkManager.IsListening) return;

            if (_networkManager.IsServer)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                return;
            }

            ulong rtt = _networkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(0);

            _textLatency.SetText("Latency: {0}ms", rtt);
        }
    }
}