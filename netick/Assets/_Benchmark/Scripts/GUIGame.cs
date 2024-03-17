using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using System.IO;
using System.Text;
using UnityEngine;
using Network = Netick.Unity.Network;

namespace StinkySteak.NetickBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkTransportProvider _transportProvider;
        [SerializeField] private NetworkSandbox _sandboxPrefab;
        [SerializeField] private int _port;
        [SerializeField] private FrametimeCounter _counter;

        private const int STRING_BUILDER_CAPACITY = 100;
        private StringBuilder _stringBuilder = new(STRING_BUILDER_CAPACITY);

        private NetworkSandbox _activeSandbox;
        private const string ARGS_AUTO_CLIENT = "-autoclient";
        private const string ARGS_AUTO_SERVER = "-autoserver";
        private const string ARGS_CLIENT_COUNT = "-clientcount";
        private const string ARGS_SERVER_IP = "-serverip";

        private SimulationTimer.SimulationTimer _timerServerLog;
        private float _intervalServerLog = 1f;
        private string _filePath;


        protected override void MonoStart()
        {
            _filePath = Application.persistentDataPath + "/NetickServerOutput.txt";

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

            Network.Sandboxs sandboxes = Network.StartAsMultiplePeers(_transportProvider, _port, null, false, clientCount);

            if (sandboxes.Clients == null) return;

            for (int i = 0; i < sandboxes.Clients.Length; i++)
            {
                NetworkSandbox sandbox = sandboxes.Clients[i];

                sandbox.Connect(_port, serverIp);
                sandbox.InputEnabled = true;
            }
        }

        protected override void StressTest(StressTestEssential stressTestEssential)
        {
            for (int i = 0; i < stressTestEssential.SpawnCount; i++)
                _activeSandbox.NetworkInstantiate(stressTestEssential.Prefab, Vector3.zero, Quaternion.identity);
        }

        protected override void StartServer()
        {
            _activeSandbox = Network.StartAsServer(_transportProvider, _port, _sandboxPrefab.gameObject);
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

            if (!_activeSandbox.IsServer) return;

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

            int connectedClients = _activeSandbox.ConnectedClients.Count;
            float avgFrameTime = _counter.GetAvgFrameTime();

            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("Average FrameTime: {0}ms. Connected Clients: {1}\n", avgFrameTime, connectedClients);

            File.AppendAllText(_filePath, _stringBuilder.ToString());

            _timerServerLog = SimulationTimer.SimulationTimer.CreateFromSeconds(_intervalServerLog);
        }
    }
}