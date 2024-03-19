using Fusion;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using UnityEngine;

namespace StinkySteak.FusionBenchmark
{
    public class GUIGame : BaseGUIGame, INetcodeBenchmarkRunner
    {
        [SerializeField] private FrametimeCounter _counter;
        [SerializeField] private NetworkProjectConfigAsset _networkProjectConfig;
        private NetworkRunner _runner;
        private FusionClientSessionDebugger _fusionClientSessionDebugger;
        private FrametimeLogger _frametimeLogger;

        protected override void MonoStart()
        {
            _frametimeLogger = new FrametimeLogger();
            _frametimeLogger.Initialize(this);

            RunAutoServer();
            RunAutoClient();
        }

        private void RunAutoServer()
        {
            bool isAutoServer = HeadlessUtils.HasArg(HeadlessArguments.AUTO_SERVER);

            if (!isAutoServer) return;

            StartServer();
            RefigureHeadlessServerProperty();
        }

        private void RunAutoClient()
        {
            int clientCount = 0;

            bool isAutoClient = HeadlessUtils.HasArg(HeadlessArguments.AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(HeadlessArguments.CLIENT_COUNT, out string argsClientCount))
            {
                clientCount = int.Parse(argsClientCount);
            }

            if (clientCount > 1)
            {
                _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Multiple;
            }

            for (int i = 0; i < clientCount; i++)
            {
                NetworkRunner runner = new GameObject($"Runner-{i}").AddComponent<NetworkRunner>();
                runner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Client,
                    SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                    Scene = 0,
                    SessionName = "my-session",
                });
            }
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            print($"[GUIGame]: OnRunnerInitialized: {runner}");
            _runner = runner;
            RefigureHeadlessServerProperty();
        }

        protected override void StartClient()
        {
            print($"[GUIGame]: Starting client...");

            _runner = new GameObject("Runner").AddComponent<NetworkRunner>();
            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Single;

            _fusionClientSessionDebugger = new FusionClientSessionDebugger();
            _runner.AddCallbacks(_fusionClientSessionDebugger);

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

            _runner = new GameObject("Runner").AddComponent<NetworkRunner>();
            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Single;

            _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Server,
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = 0,
                SessionName = "my-session",
                Initialized = OnRunnerInitialized,
            });

            print($"[GUIGame]: Server Started: {_runner}");
        }
        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                _runner.Spawn(stressTest.Prefab);
            }
        }

        private bool IsRunnerRunning()
            => _runner != null && _runner.IsRunning;

        protected override void UpdateNetworkStats()
        {
            if (!IsRunnerRunning()) return;

            if (_runner.IsServer)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                AutoRunStressTest();
                return;
            }

            float latency = (float)_runner.GetPlayerRtt(_runner.LocalPlayer) * 1000;

            _textLatency.SetText($"Latency: {latency}ms ({_runner.CurrentConnectionType})");
        }
        private void AutoRunStressTest()
        {
            if (_headlessServerProperty.TimerActivateTest.IsExpired())
            {
                _headlessServerProperty.TimerActivateTest = SimulationTimer.SimulationTimer.None;
                StressTest(_headlessServerProperty.Test);
            }
        }

        protected override void MonoUpdate()
        {
            PrintAverageFrameTime();
        }

        private void PrintAverageFrameTime()
        {
            if (!IsRunnerRunning()) return;

            _frametimeLogger.MonoUpdate();
        }

        public int GetConnectedClients()
        {
            int players = 0;

            foreach (PlayerRef player in _runner.ActivePlayers)
                players++;

            return players;
        }

        public float GetAverageFrameTime()
        {
            return _counter.GetAvgFrameTime();
        }
    }
}