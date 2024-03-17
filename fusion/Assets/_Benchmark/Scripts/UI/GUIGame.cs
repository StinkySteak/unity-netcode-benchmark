using Fusion;
using Fusion.Sockets;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StinkySteak.FusionBenchmark
{
    public class GUIGame : BaseGUIGame, INetworkRunnerCallbacks
    {
        [SerializeField] private FrametimeCounter _counter;
        [SerializeField] private NetworkRunner _runner;
        [SerializeField] private NetworkProjectConfigAsset _networkProjectConfig;

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
            _runner.AddCallbacks(this);

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

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            print($"ShutdownReason: {shutdownReason}");
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            print($"reason: {reason}");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}