using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;
using UnityEngine.UI;
using Network = Netick.Unity.Network;

namespace StinkySteak.NetickBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkTransportProvider _transportProvider;
        [SerializeField] private int _port;

        private NetworkSandbox _activeSandbox;

        [Header("Stress Test 1: Move Y")]
        [SerializeField] private StressTestEssential _test_1;

        [Header("Stress Test 2: Move All Axis")]
        [SerializeField] private StressTestEssential _test_2;

        [Header("Stress Test 3: Move Wander")]
        [SerializeField] private StressTestEssential _test_3;

        [System.Serializable]
        public struct StressTestEssential
        {
            public Button ButtonExecute;
            public int SpawnCount;
            public GameObject Prefab;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _test_1.ButtonExecute.onClick.AddListener(StressTest_1);
            _test_2.ButtonExecute.onClick.AddListener(StressTest_2);
            _test_3.ButtonExecute.onClick.AddListener(StressTest_3);
        }

        private void StressTest_1()
            => StressTest(_test_1);
        private void StressTest_2()
            => StressTest(_test_2);
        private void StressTest_3()
           => StressTest(_test_3);

        private void StressTest(StressTestEssential stressTestEssential)
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