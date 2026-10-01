using Blackhole.GlobalDialogue.Application;
using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using Blackhole.SharedKernel.Presentaion;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Blackhole.GlobalDialogue.Presentation
{
    public class EffectPresentation : Orientation, IEffectOutput
    {
        private VisualElement _root;
        private VisualElement _effectVE;
        private Dictionary<EffectType, EffectData> _effectData = new Dictionary<EffectType, EffectData>();

        private EffectAnimations _effectAnimations;

        [Inject] private EffectSettingSO _setting;

        private IScopedHandlerRegistery<IEffectOutput> _handler;

        [Inject]
        public void Construct(IScopedHandlerRegistery<IEffectOutput> handler)
        {
            _handler = handler;
        }

        protected override void Awake()
        {
            base.Awake();
            _handler.Register(this);
            //_effectAnimations = new EffectAnimations();
            _effectAnimations = new GameObject("Effect Animation").AddComponent<EffectAnimations>();
        }

        protected override void OnUIReloaded(PanelRenderer panelRenderer, VisualElement root)
        {
            base.OnUIReloaded(panelRenderer, root);
            _root = DocumentRoot;
            _effectVE = _root.Q<VisualElement>("EffectVE");
            Initialize();
        }

        private void Initialize()
        {
            foreach (var item in _setting.EffectDatas)
            {

                if (Enum.TryParse<EffectType>(item.EffectName.Replace(" ", ""), out var effect))
                {
                    _effectData[effect] = item;
                }
            }
        }

        public void PresentEffect(Vector2 position, EffectSetting setting)
        {
            Vector2 panel_pos = UI_Position(position+setting.Offset);

            Image img = new Image();

            SetUpImage(img, setting.Type, setting.AnchorPosition, panel_pos);

            _effectAnimations.UpwardFadeOut(img, setting.ShowTime);

            WaitToDestroy(img, setting.ShowTime).Forget();
        }

        private async UniTask WaitToDestroy(VisualElement target, int delay)
        {
            await UniTask.Delay(delay);

            _effectVE.Remove(target);
        }

        private Vector2 UI_Position(Vector2 worldPos)
        {
            Camera camera = Camera.main;

            Vector2 uiCenter = new(Screen.width / 2, Screen.height / 2);

            float worldHeight = camera.orthographicSize * 2;
            float worldWidth = worldHeight * camera.aspect;

            float widthDiff = _effectVE.resolvedStyle.width / worldWidth;
            float heightDiff = _effectVE.resolvedStyle.height / worldHeight;

            Vector2 panelPosition = new Vector2(worldPos.x * widthDiff, -worldPos.y * heightDiff);
            panelPosition += uiCenter;

            return panelPosition;
        }

        private void SetUpImage(Image img, EffectType type, ImageAnchor anchor, Vector2 panel_pos)
        {
            img.sprite = _effectData[type].Effect_Sprite;
            img.style.position = Position.Absolute;

            Vector2 imgSize;
            img.pickingMode = PickingMode.Ignore;
            img.style.opacity = 0;
            _effectVE.Add(img);

            float x;
            float y;
            img.schedule.Execute(() =>
            {
                if (anchor == ImageAnchor.CenterToCenter)
                {
                    x = img.resolvedStyle.width;
                    y = img.resolvedStyle.height;
                    imgSize = new Vector2(-x / 2, -y / 2);

                    panel_pos += imgSize;
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);
                }

                else if (anchor == ImageAnchor.BottomLeftToCenter)
                {
                    y = img.resolvedStyle.height;
                    imgSize = new Vector2(0, -y);

                    panel_pos += imgSize;
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);
                }

                else if (anchor == ImageAnchor.TopLeftToCenter)
                {
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);
                }

                else if (anchor == ImageAnchor.BottomRightToCenter)
                {
                    x = img.resolvedStyle.width;
                    y = img.resolvedStyle.height;
                    imgSize = new Vector2(-x, -y);

                    panel_pos += imgSize;
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);

                }

                else if (anchor == ImageAnchor.TopRightToCenter)
                {
                    x = img.resolvedStyle.width;
                    imgSize = new Vector2(-x, 0);

                    panel_pos += imgSize;
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);

                }

                else if (anchor == ImageAnchor.MidBottomToCenter)
                {
                    x = img.resolvedStyle.width;
                    y = img.resolvedStyle.height;
                    imgSize = new Vector2(-x/2, -y);

                    panel_pos += imgSize;
                    img.style.translate = new Translate(panel_pos.x, panel_pos.y);

                }

                img.style.opacity = 1;
                img.style.display = DisplayStyle.Flex;
            }).ExecuteLater(10);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _handler.Unregister(this);
        }
    }
}