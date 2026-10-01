using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    [UxmlElement("AnimatedButton")]
    public partial class AnimatedButton : Button
    {
        private static readonly CustomStyleProperty<Color> NormalBackgroundColorStyleProperty = new("--normal-background-color");
        private static readonly CustomStyleProperty<Color> NormalTintColorStyleProperty = new("--normal-tint-color");
        private static readonly CustomStyleProperty<Color> PressedTintColorStyleProperty = new("--pressed-tint-color");

        private float pressedScale = 0.9f;
        private int animationDurationMs = 80;

        private Color normalBackgroundColor = new Color32(0, 0, 0, 0);
        private Color pressedTintColor = new Color32(123, 123, 123, 255);
        private Color normalTintColor = Color.white;
        private Sprite backgroundImage;

        private bool isDown;

        [UxmlAttribute("pressed-scale")]
        public float PressedScale
        {
            get => pressedScale;
            set
            {
                pressedScale = Mathf.Max(0f, value);

                if (isDown)
                    ApplyPressedVisualState();
                else
                    ResetScaleOnly();
            }
        }

        [UxmlAttribute("animation-duration-ms")]
        public int AnimationDurationMs
        {
            get => animationDurationMs;
            set
            {
                animationDurationMs = Mathf.Max(0, value);
                ApplyTransitionStyle();
            }
        }

        [UxmlAttribute("normal-background-color")]
        public Color NormalBackgroundColor
        {
            get => normalBackgroundColor;
            set
            {
                normalBackgroundColor = value;
                ApplyBackgroundColor();
            }
        }

        [UxmlAttribute("pressed-tint-color")]
        public Color PressedTintColor
        {
            get => pressedTintColor;
            set
            {
                pressedTintColor = value;

                if (isDown)
                    ApplyTintColor(pressedTintColor);
            }
        }

        [UxmlAttribute("normal-tint-color")]
        public Color NormalTintColor
        {
            get => normalTintColor;
            set
            {
                normalTintColor = value;

                if (!isDown)
                    ApplyTintColor(normalTintColor);
            }
        }

        [UxmlAttribute("background-image")]
        public Sprite BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;
                ApplyBackgroundImage();
            }
        }

        public AnimatedButton()
        {
            clickable = null;
            ApplyBaseStyle();

            RegisterCallback<AttachToPanelEvent>(_ => ApplyBaseStyle());
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<DetachFromPanelEvent>(_ => ResetVisualState());
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            ICustomStyle customStyle = evt.customStyle;

            if (customStyle.TryGetValue(NormalBackgroundColorStyleProperty, out Color resolvedNormalBackgroundColor))
                normalBackgroundColor = resolvedNormalBackgroundColor;

            if (customStyle.TryGetValue(NormalTintColorStyleProperty, out Color resolvedNormalTintColor))
                normalTintColor = resolvedNormalTintColor;

            if (customStyle.TryGetValue(PressedTintColorStyleProperty, out Color resolvedPressedTintColor))
                pressedTintColor = resolvedPressedTintColor;

            ApplyBaseStyle();
        }

        private void ApplyBaseStyle()
        {
            ApplyBackgroundColor();
            ApplyBackgroundImage();
            ApplyTintColor(isDown ? pressedTintColor : normalTintColor);
            ApplyTransitionStyle();

            style.transformOrigin = new TransformOrigin(
                new Length(50, LengthUnit.Percent),
                new Length(50, LengthUnit.Percent),
                0
            );

            style.scale = isDown
                ? new Scale(new Vector3(pressedScale, pressedScale, 1f))
                : new Scale(Vector3.one);
        }

        private void ApplyBackgroundColor()
        {
            style.backgroundColor = normalBackgroundColor;
        }

        private void ApplyBackgroundImage()
        {
            if (backgroundImage == null)
                return;

            style.backgroundImage = new StyleBackground(backgroundImage);
        }

        private void ApplyTintColor(Color color)
        {
            style.unityBackgroundImageTintColor = color;
        }

        private void ApplyTransitionStyle()
        {
            style.transitionProperty = new List<StylePropertyName>
            {
                new StylePropertyName("scale"),
                new StylePropertyName("background-color"),
                new StylePropertyName("-unity-background-image-tint-color")
            };

            style.transitionDuration = new List<TimeValue>
            {
                new TimeValue(animationDurationMs, TimeUnit.Millisecond),
                new TimeValue(animationDurationMs, TimeUnit.Millisecond),
                new TimeValue(animationDurationMs, TimeUnit.Millisecond)
            };

            style.transitionTimingFunction = new List<EasingFunction>
            {
                new EasingFunction(EasingMode.EaseOut),
                new EasingFunction(EasingMode.EaseOut),
                new EasingFunction(EasingMode.EaseOut)
            };
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (!isDown)
                return;

            if ((evt.pressedButtons & 1) == 0)
            {
                ResetVisualState();
                return;
            }

            ApplyPressedVisualState();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            isDown = true;
            ApplyPressedVisualState();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
                return;

            ResetVisualState();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            ResetVisualState();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            ResetScaleOnly();
        }

        private void ApplyPressedVisualState()
        {
            style.scale = new Scale(new Vector3(pressedScale, pressedScale, 1f));
            ApplyTintColor(pressedTintColor);
        }

        private void ResetVisualState()
        {
            isDown = false;
            style.scale = new Scale(Vector3.one);
            ApplyTintColor(normalTintColor);
            ApplyBackgroundColor();
            ApplyBackgroundImage();
        }

        private void ResetScaleOnly()
        {
            style.scale = new Scale(Vector3.one);
            ApplyTintColor(normalTintColor);
            ApplyBackgroundColor();
            ApplyBackgroundImage();
        }
    }
}
