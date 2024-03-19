using FishNet.Managing;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using UnityEngine;

namespace StinkySteak.FishnetBenchmark
{
    public class GUIGame : BaseGUIGame, INetcodeBenchmarkRunner
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        private NetworkManager _networkManager;
        [SerializeField] private FrametimeCounter _counter;
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

        protected override void MonoUpdate()
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
            _frametimeLogger.MonoUpdate();
        }

        public int GetConnectedClients()
        {
            return _networkManager.ServerManager.Clients.Count;
        }

        public float GetAverageFrameTime()
        {
            return _counter.GetAvgFrameTime();
        }
    }
}