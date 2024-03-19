using Netick.Unity;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using UnityEngine;
using Network = Netick.Unity.Network;

namespace StinkySteak.NetickBenchmark
{
    public class GUIGame : BaseGUIGame, INetcodeBenchmarkRunner
    {
        [SerializeField] private NetworkTransportProvider _transportProvider;
        [SerializeField] private NetworkSandbox _sandboxPrefab;
        [SerializeField] private int _port;
        [SerializeField] private FrametimeCounter _counter;
        private FrametimeLogger _frametimeLogger;

        private NetworkSandbox _activeSandbox;

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
            string serverIp = string.Empty;

            bool isAutoClient = HeadlessUtils.HasArg(HeadlessArguments.AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(HeadlessArguments.CLIENT_COUNT, out string argsClientCount))
            {
                clientCount = int.Parse(argsClientCount);
            }

            if (HeadlessUtils.TryGetArg(HeadlessArguments.SERVER_IP, out string argsServerIp))
            {
                serverIp = argsServerIp;
            }

            Network.LaunchResults sandboxes = Network.StartAsMultiplePeers(_transportProvider, _port, null, false, false, clientCount);

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
            if (!HeadlessUtils.IsHeadlessMode()) return;

            if (_activeSandbox == null) return;

            if (!_activeSandbox.IsServer) return;

            AutoRunStressTest();
            PrintAverageFrameTime();
        }

        private void PrintAverageFrameTime()
        {
            _frametimeLogger.MonoUpdate();
        }

        public int GetConnectedClients()
        {
            return _activeSandbox.ConnectedClients.Count;
        }

        public float GetAverageFrameTime()
        {
            return _counter.GetAvgFrameTime();
        }
    }
}