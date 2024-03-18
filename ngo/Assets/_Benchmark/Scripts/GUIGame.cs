using StinkySteak.NetcodeBenchmark;
using StinkySteak.NetcodeBenchmark.Util;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace StinkySteak.NGOBenchmark
{
    public class GUIGame : BaseGUIGame
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;
        [SerializeField] private FrametimeCounter _counter;

        private NetworkManager _networkManager;
        private FrametimeLogger _frametimeLogger;

        protected override void MonoStart()
        {
            _frametimeLogger = new();
            _frametimeLogger.Initialize();
            _networkManager = Instantiate(_networkManagerPrefab);

            RegisterPrefabs(new StressTestEssential[] { _test_1, _test_2, _test_3, _test_4 });

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
            string serverIp = string.Empty;

            bool isAutoClient = HeadlessUtils.HasArg(HeadlessArguments.AUTO_CLIENT);

            if (!isAutoClient) return;

            if (HeadlessUtils.TryGetArg(HeadlessArguments.SERVER_IP, out string argsServerIp))
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
            LogFrametime();
        }

        private void LogFrametime()
        {
            int connectedClients = _networkManager.ConnectedClients.Count;
            float avgFrameTime = _counter.GetAvgFrameTime();

            _frametimeLogger.MonoUpdate(connectedClients, avgFrameTime);
        }

        private void AutoRunStressTest()
        {
            if (_headlessServerProperty.TimerActivateTest.IsExpired())
            {
                _headlessServerProperty.TimerActivateTest = SimulationTimer.SimulationTimer.None;
                StressTest(_headlessServerProperty.Test);
            }
        }
    }
}