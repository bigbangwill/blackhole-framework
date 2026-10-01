using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Blackhole.GlobalDialogue.Presentation
{
    public class EffectAnimations : MonoBehaviour
    {
        public void FadeOutAnimation(VisualElement targetVE, int duration)
        {
            targetVE.schedule.Execute(() =>
            {
                targetVE.experimental.animation.Start(1f, 0f, duration, (element, value) =>
                {
                    element.style.opacity = value;
                }).Ease(Easing.OutCubic);

            }).ExecuteLater(10);
        }

        public void ScaleFadeOut(VisualElement targetVE, int duration)
        {
            targetVE.schedule.Execute(() =>
            {
                //Scale Out
                targetVE.experimental.animation.Start(new Vector2(1.2f, 1.2f), Vector2.zero, duration, (elemetn, value) =>
                {
                    elemetn.style.scale = new StyleScale(value);
                }).Ease(Easing.InQuad);

                //Fade out
                targetVE.experimental.animation.Start(1f, 0f, duration, (element, value) =>
                {
                    element.style.opacity = value;
                }).Ease(Easing.OutCubic);

            }).ExecuteLater(10);
        }

        public void UpwardFadeOut(VisualElement targetVE, int duration)
        {
            targetVE.schedule.Execute(() =>
            {

                //Moving UpWard 
                Vector2 start_pos = new Vector2(targetVE.resolvedStyle.translate.x, targetVE.resolvedStyle.translate.y);

                targetVE.experimental.animation.Start(start_pos, new Vector2(start_pos.x, start_pos.y - 100), duration, (elemetn, value) =>
                {
                    elemetn.style.translate = new Translate(value.x, value.y);
                }).Ease(Easing.OutQuad);

                //Fade out
                targetVE.experimental.animation.Start(1f, 0f, duration, (element, value) =>
                {
                    element.style.opacity = value;
                }).Ease(Easing.OutQuad);

            }).ExecuteLater(10);
        }
    }
}