using Fusion;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StinkySteak.FusionBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private FrametimeCounter _counter;
        [SerializeField] private NetworkRunner _runner;
        [SerializeField] private NetworkProjectConfigAsset _networkProjectConfig;
        private FusionClientSessionDebugger _fusionClientSessionDebugger;

        private const string ARGS_AUTO_CLIENT = "-autoclient";
        private const string ARGS_AUTO_SERVER = "-autoserver";
        private const string ARGS_CLIENT_COUNT = "-clientcount";

        private const int STRING_BUILDER_CAPACITY = 100;
        private StringBuilder _stringBuilder = new(STRING_BUILDER_CAPACITY);

        private SimulationTimer.SimulationTimer _timerServerLog;
        private float _intervalServerLog = 1f;

        private string _filePath;

        protected override void MonoStart()
        {
            _filePath = Application.persistentDataPath + "/FusionV1ServerOutput.txt";
            RunAutoServer();
            RunAutoClient();
        }

        private void RunAutoServer()
        {
            bool isAutoServer = HeadlessUtils.HasArg(ARGS_AUTO_SERVER);

            if (!isAutoServer) return;

            StartServer();
            RefigureHeadlessServerProperty();
        }

        private void RunAutoClient()
        {
            int clientCount = 0;

            bool isAutoClient = HeadlessUtils.HasArg(ARGS_AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(ARGS_CLIENT_COUNT, out string argsClientCount))
            {
                clientCount = int.Parse(argsClientCount);
            }

            _networkProjectConfig.Config.PeerMode = NetworkProjectConfig.PeerModes.Multiple;
            
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
            print($"OnRunnerInitialized: {runner}");
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

        protected override void UpdateNetworkStats()
        {
            if (_runner == null) return;

            if (!_runner.IsRunning) return;

            if (_runner.IsServer)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                AutoRunStressTest();
                PrintAverageFrameTime();
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

        private void PrintAverageFrameTime()
        {
            if (!_timerServerLog.IsExpiredOrNotRunning()) return;

            int connectedClients = _runner.ActivePlayers.Count();
            float avgFrameTime = _counter.GetAvgFrameTime();

            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("Average FrameTime: {0}ms. Connected Clients: {1}\n", avgFrameTime, connectedClients);

            File.AppendAllText(_filePath, _stringBuilder.ToString());

            _timerServerLog = SimulationTimer.SimulationTimer.CreateFromSeconds(_intervalServerLog);
        }

    }
}