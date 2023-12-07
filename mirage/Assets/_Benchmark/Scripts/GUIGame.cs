using Mirage;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.MirrorBenchmark
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

        private void RegisterPrefabs(StressTestEssential[] stressTestEssential)
        {
            for (int i = 0; i < stressTestEssential.Length; i++)
            {
                _networkManager.ClientObjectManager.RegisterPrefab(stressTestEssential[i].Prefab.GetNetworkIdentity());
            }
        }

        protected override void StartClient()
        {
            _networkManager.Client.Connect();
        }

        protected override void StartServer()
        {
            _networkManager.Server.StartServer();
        }

        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                GameObject go = Instantiate(stressTest.Prefab);
                _networkManager.ServerObjectManager.Spawn(go);
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            if (!_networkManager.IsNetworkActive) return;

            if (_networkManager.NetworkMode == NetworkManagerMode.Server)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                return;
            }
            else // client 
            {
                var networkTime = _networkManager.Client.World.Time;
                _textLatency.SetText("Latency: {0}ms", (float)networkTime.Rtt * 1_000);
            }
        }
    }
}