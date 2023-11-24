using FishNet.Managing;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        private NetworkManager _networkManager;

        protected override void Initialize()
        {
            base.Initialize();
            _networkManager = Instantiate(_networkManagerPrefab);
        }

        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                GameObject go = Instantiate(stressTest.Prefab);
                _networkManager.ServerManager.Spawn(go);
            }
        }

        protected override void StartClient()
        {
            _networkManager.ClientManager.StartConnection("127.0.0.1", 25565);
        }
        protected override void StartServer()
        {
            _networkManager.ServerManager.StartConnection(25565);
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            _textLatency.SetText("Latency: {0}ms", _networkManager.TimeManager.RoundTripTime);
        }
    }
}