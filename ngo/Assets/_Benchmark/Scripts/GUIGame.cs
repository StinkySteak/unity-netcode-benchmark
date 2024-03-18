using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using System.IO;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
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
            _filePath = Application.persistentDataPath + "/NGOServerOutput.txt";

            _networkManager = Instantiate(_networkManagerPrefab);

            RegisterPrefabs(new StressTestEssential[] { _test_1, _test_2, _test_3 });

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
            string serverIp = string.Empty;

            bool isAutoClient = HeadlessUtils.HasArg(ARGS_AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(ARGS_SERVER_IP, out string argsServerIp))
                serverIp = argsServerIp;

            UnityTransport unityTransport = _networkManager.GetComponent<UnityTransport>();
            unityTransport.ConnectionData.Address = serverIp;
            _networkManager.StartClient();
        }

        private void RegisterPrefabs(StressTestEssential[] stressTest)
        {
            for (int i = 0; i < stressTest.Length; i++)
            {
                _networkManager.AddNetworkPrefab(stressTest[i].Prefab);
            }
        }

        protected override void StartClient()
        {
            _networkManager.StartClient();
        }

        protected override void StartServer()
        {
            _networkManager.StartServer();
        }

        protected override void StressTest(StressTestEssential stressTest)
        {
            for (int i = 0; i < stressTest.SpawnCount; i++)
            {
                GameObject go = Instantiate(stressTest.Prefab);
                go.GetComponent<NetworkObject>().Spawn();
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            if (!_networkManager.IsListening) return;

            if (_networkManager.IsServer)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                return;
            }

            ulong rtt = _networkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(0);

            _textLatency.SetText("Latency: {0}ms", rtt);
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

            int connectedClients = _networkManager.ConnectedClients.Count;
            float avgFrameTime = _counter.GetAvgFrameTime();

            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("Average FrameTime: {0}ms. Connected Clients: {1}\n", avgFrameTime, connectedClients);

            File.AppendAllText(_filePath, _stringBuilder.ToString());

            _timerServerLog = SimulationTimer.SimulationTimer.CreateFromSeconds(_intervalServerLog);
        }
    }
}