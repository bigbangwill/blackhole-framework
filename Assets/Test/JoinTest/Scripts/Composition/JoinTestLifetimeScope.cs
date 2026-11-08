using Blackhole.Networking.Contract;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace Blackhole.JoinTest.Composition
{
    public sealed class JoinTestLifetimeScope : LifetimeScope
    {
        private INetworkClient _client;
        private UnityEngine.UI.Text _status;
        private string _message;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<JoinTestEntryPoint>(Lifetime.Singleton);
        }

        private void Start()
        {
            Container.TryResolve<INetworkClient>(out _client);

            var canvasObject = new GameObject("JoinTestCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(transform, false);
            }

            var buttonObject = new GameObject("ConnectButton", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 80);
            rect.anchoredPosition = new Vector2(0, 40);
            var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
            button.interactable = _client != null;
            button.onClick.AddListener(Connect);
            CreateText(buttonObject.transform, "Connect to server", Vector2.zero, new Vector2(300, 80));
            _status = CreateText(canvasObject.transform, "", new Vector2(0, -40), new Vector2(600, 60));
            _status.color = Color.white;
        }

        private void Update()
        {
            if (_status != null) _status.text = _client == null ? "Network client unavailable" : _message ?? $"Client: {_client.State}";
        }

        private void Connect()
        {
            if (_client.State != NetworkConnectionState.Disconnected)
            {
                _message = null;
                return;
            }

            _message = _client.Connect(new NetworkAddress("127.0.0.1", 7788)).IsSuccess ? null : "Connection attempt could not be started";
        }

        private static UnityEngine.UI.Text CreateText(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(UnityEngine.UI.Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = textObject.GetComponent<UnityEngine.UI.Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 28;
            label.color = Color.black;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }
    }

    public class ServerNetworkHandlerTest : INetworkPayloadHandler
    {
        public void Handle(NetworkPeerId id, NetworkPayload payload)
        {
            Debug.Log($"PeerId: {id}, Payload: {payload}");
        }
    }

    public class ClientNetworkHandlerTest : INetworkClientPayloadHandler
    {
        public void Handle(NetworkPayload payload)
        {
            Debug.Log(payload);
        }
    }
}