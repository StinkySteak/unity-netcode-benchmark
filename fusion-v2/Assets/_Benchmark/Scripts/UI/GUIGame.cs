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
        [SerializeField][ReadOnly] private NetworkRunner _runner;
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

            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Multiple;

            for (int i = 0; i < clientCount; i++)
            {
                NetworkRunner runner = CreateNetworkRunner($"Runner-{i}");
                runner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Client,
                    SceneManager = runner.gameObject.GetComponent<NetworkSceneManagerDefault>(),
                    Scene = SceneRef.FromIndex(0),
                    SessionName = "my-session",
                    ObjectProvider = runner.gameObject.GetComponent<NetworkObjectProviderDefault>(),
                    ObjectInitializer = new NetworkObjectInitializerUnity()
                });
            }
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            RefigureHeadlessServerProperty();
        }

        protected override void StartClient()
        {
            print($"[GUIGame]: Starting client...");

            _runner = CreateNetworkRunner();
            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Single;

            _fusionClientSessionDebugger = new FusionClientSessionDebugger();
            _runner.AddCallbacks(_fusionClientSessionDebugger);

            _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = _runner.gameObject.GetComponent<NetworkSceneManagerDefault>(),
                Scene = SceneRef.FromIndex(0),
                SessionName = "my-session",
                ObjectProvider = _runner.gameObject.GetComponent<NetworkObjectProviderDefault>(),
                ObjectInitializer = new NetworkObjectInitializerUnity()
            });
        }

        private NetworkRunner CreateNetworkRunner(string runnerName = "NetworkRunner")
        {
            NetworkRunner runner = new GameObject(runnerName).AddComponent<NetworkRunner>();
            runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
            return runner;
        }

        protected override void StartServer()
        {
            print($"[GUIGame]: Starting server...");

            _runner = CreateNetworkRunner();
            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Single;

            _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Server,
                SceneManager = _runner.gameObject.GetComponent<NetworkSceneManagerDefault>(),
                Scene = SceneRef.FromIndex(0),
                SessionName = "my-session",
                ObjectProvider = _runner.gameObject.GetComponent<NetworkObjectProviderDefault>(),
                ObjectInitializer = new NetworkObjectInitializerUnity(),
                OnGameStarted = OnRunnerInitialized,
            });
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
            if (!IsRunnerRunning()) return;

            if (!_runner.IsServer) return;
            
            if (!HeadlessUtils.IsHeadlessMode()) return;

            AutoRunStressTest();
            PrintAverageFrameTime();
        }

        private void PrintAverageFrameTime()
        {
            _frametimeLogger.MonoUpdate();
        }

        public int GetConnectedClients()
        {
            int playersCount = 0;

            foreach (PlayerRef playerRef in _runner.ActivePlayers)
                playersCount++;

            return playersCount;
        }

        public float GetAverageFrameTime()
        {
            return _counter.GetAvgFrameTime();
        }
    }
}