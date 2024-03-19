using Mirror;
using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using UnityEngine;

namespace StinkySteak.MirrorBenchmark
{
    public class GUIGame : BaseGUIGame, INetcodeBenchmarkRunner
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        [SerializeField] private FrametimeCounter _counter;
        private NetworkManager _networkManager;
        private FrametimeLogger _frametimeLogger;

        protected override void MonoStart()
        {
            _frametimeLogger = new();
            _frametimeLogger.Initialize(this);

            _networkManager = Instantiate(_networkManagerPrefab);
            RegisterPrefabs(new StressTestEssential[] { _test_1, _test_2, _test_3, _test_4 });

            RunAutoServer();
            RunAutoClient();
        }


        private void RunAutoClient()
        {
            string serverIp = string.Empty;

            bool isAutoClient = HeadlessUtils.HasArg(HeadlessArguments.AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(HeadlessArguments.SERVER_IP, out string argsServerIp))
                serverIp = argsServerIp;

            _networkManager.networkAddress = serverIp;
            _networkManager.StartClient();
        }


        private void RunAutoServer()
        {
            bool isAutoServer = HeadlessUtils.HasArg(HeadlessArguments.AUTO_SERVER);

            if (!isAutoServer) return;

            StartServer();
            RefigureHeadlessServerProperty();
        }

        private void RegisterPrefabs(StressTestEssential[] stressTestEssential)
        {
            for (int i = 0; i < stressTestEssential.Length; i++)
            {
                _networkManager.spawnPrefabs.Add(stressTestEssential[i].Prefab);
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
                NetworkServer.Spawn(go);
            }
        }

        protected override void UpdateNetworkStats()
        {
            if (_networkManager == null) return;

            if (!_networkManager.isNetworkActive) return;

            if (_networkManager.mode == NetworkManagerMode.ServerOnly)
            {
                _textLatency.SetText("Latency: 0ms (Server)");
                return;
            }

            _textLatency.SetText("Latency: {0}ms", (float)NetworkTime.rtt * 1_000);
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

            if (_networkManager.mode != NetworkManagerMode.ServerOnly) return;

            AutoRunStressTest();
            PrintAverageFrameTime();
        }

        private void PrintAverageFrameTime()
        {
            _frametimeLogger.MonoUpdate();
        }

        public int GetConnectedClients()
        {
            return _networkManager.numPlayers;
        }

        public float GetAverageFrameTime()
        {
            return _counter.GetAvgFrameTime();
        }
    }
}