using Fusion;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;
using UnityEngine.UI;

namespace StinkySteak.FusionBenchmark
{
    public class GUIGame : BaseGUIGame
    {
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
            public int Count;
            public NetworkObject Prefab;
        }

        private NetworkRunner _runner;

        protected override void Initialize()
        {
            base.Initialize();
            _runner = new GameObject("Runner").AddComponent<NetworkRunner>();

            _test_1.ButtonExecute.onClick.AddListener(StartTest_1);
            _test_2.ButtonExecute.onClick.AddListener(StartTest_2);
            _test_3.ButtonExecute.onClick.AddListener(StartTest_3);
        }

        protected override void StartClient()
        {
            print($"[GUIGame]: Starting client...");

            _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = 0,
                SessionName = "my-session",
            });
        }

        protected override void StartServer()
        {
            print($"[GUIGame]: Starting server...");

            _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Server,
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = 0,
                SessionName = "my-session",
            });
        }

        private void StartTest_1()
            => StartTest(_test_1);

        private void StartTest_2()
           => StartTest(_test_2);

        private void StartTest_3()
           => StartTest(_test_3);

        private void StartTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.Count; i++)
            {
                _runner.Spawn(stressTest.Prefab);
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_runner == null) return;

            if (!_runner.IsRunning) return;

            float latency = (float)_runner.GetPlayerRtt(_runner.LocalPlayer) * 1000;

            _textLatency.SetText("Latency: {0}ms", latency);
        }
    }
}