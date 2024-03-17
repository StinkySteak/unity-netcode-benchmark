using FishNet.Managing;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using System.IO;
using System.Text;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        private NetworkManager _networkManager;
        [SerializeField] private FrametimeCounter _counter;

        private const int STRING_BUILDER_CAPACITY = 100;
        private StringBuilder _stringBuilder = new(STRING_BUILDER_CAPACITY);

        private const string ARGS_AUTO_CLIENT = "-autoclient";
        private const string ARGS_AUTO_SERVER = "-autoserver";
        private const string ARGS_CLIENT_COUNT = "-clientcount";
        private const string ARGS_SERVER_IP = "-serverip";

        private SimulationTimer.SimulationTimer _timerServerLog;
        private float _intervalServerLog = 1f;
        private string _filePath;

        protected override void MonoStart()
        {
            _filePath = Application.persistentDataPath + "/FishnetServerOutput.txt";

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
            string serverIp = string.Empty;

            bool isAutoClient = HeadlessUtils.HasArg(ARGS_AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(ARGS_CLIENT_COUNT, out string argsClientCount))
            {
                clientCount = int.Parse(argsClientCount);
            }

            if (HeadlessUtils.TryGetArg(ARGS_SERVER_IP, out string argsServerIp))
            {
                serverIp = argsServerIp;
            }

            for (int i = 0; i < clientCount; i++)
            {
                NetworkManager networkManager = Instantiate(_networkManagerPrefab);
                networkManager.ClientManager.StartConnection(serverIp, 25565);
            }
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
            _networkManager = Instantiate(_networkManagerPrefab);
            _networkManager.ClientManager.StartConnection("127.0.0.1", 25565);
        }
        protected override void StartServer()
        {
            _networkManager = Instantiate(_networkManagerPrefab);
            _networkManager.ServerManager.StartConnection(25565);
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            if (_networkManager.IsOffline) return;

            _textLatency.SetText("Latency: {0}ms", _networkManager.TimeManager.RoundTripTime);
        }

        protected override void MonoLateUpdate()
        {
            if (!HeadlessUtils.IsHeadlessMode()) return;

            if (!_networkManager.IsServer) return;

            AutoRunStressTest();
            PrintAverageFrameTime();
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
            
            int connectedClients = _networkManager.ServerManager.Clients.Count;
            float avgFrameTime = _counter.GetAvgFrameTime();

            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("Average FrameTime: {0}ms. Connected Clients: {1}\n", avgFrameTime, connectedClients);

            File.AppendAllText(_filePath, _stringBuilder.ToString());

            _timerServerLog = SimulationTimer.SimulationTimer.CreateFromSeconds(_intervalServerLog);
        }
    }
}