using Fusion;
using StinkySteak.NetcodeBenchmark;
using UnityEngine;

namespace StinkySteak.FusionBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        private NetworkRunner _runner;

        protected override void Initialize()
        {
            base.Initialize();
            _runner = new GameObject("Runner").AddComponent<NetworkRunner>();
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
        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                _runner.Spawn(stressTest.Prefab);
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_runner == null) return;

            if (!_runner.IsRunning) return;

            if (_runner.IsServer)
            {
                _textLatency.SetText("Latency: {0}ms (Server)");
                return;
            }

            float latency = (float)_runner.GetPlayerRtt(_runner.LocalPlayer) * 1000;

            _textLatency.SetText($"Latency: {latency}ms ({_runner.NATType})");
        }
    }
}