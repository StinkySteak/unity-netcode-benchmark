using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;
using Network = Netick.Unity.Network;

namespace StinkySteak.NetickBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkTransportProvider _transportProvider;
        [SerializeField] private int _port;

        private NetworkSandbox _activeSandbox;

        protected override void StressTest(StressTestEssential stressTestEssential)
        {
            for (int i = 0; i < stressTestEssential.SpawnCount; i++)
                _activeSandbox.NetworkInstantiate(stressTestEssential.Prefab, Vector3.zero, Quaternion.identity);
        }

        protected override void StartServer()
        {
            _activeSandbox = Network.StartAsServer(_transportProvider, _port);
        }

        protected override void StartClient()
        {
            _activeSandbox = Network.StartAsClient(_transportProvider, _port);
            _activeSandbox.Connect(_port, "127.0.0.1");
        }

        protected override void UpdateNetworkStats()
        {
            if (_activeSandbox == null) return;

            _textLatency.SetText("Latency: {0}ms\n", _activeSandbox.Monitor.RTT.Latest * 1_000);
        }
    }
}