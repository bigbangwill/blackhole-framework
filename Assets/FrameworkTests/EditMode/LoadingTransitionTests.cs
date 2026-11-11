using System;
using System.Reflection;
using Blackhole.LoadingScreen.Application;
using Blackhole.LoadingScreen.Contract;
using Blackhole.LoadingScreen.Presentation;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blackhole.Development.Tests
{
    public sealed class LoadingTransitionTests
    {
        [Test]
        public void TransitionStaysOpenForOverlappingOperationsAndClosesOnFailure()
        {
            var presenter = new Presenter();
            var registry = new LoadingScreenPresenterRegistry();
            registry.Attach(presenter);
            var service = new LoadingScreenUseCase(registry);
            var first = new UniTaskCompletionSource();
            var second = new UniTaskCompletionSource();
            UniTask firstTransition = service.ExecuteTransitionAsync(() => first.Task);
            UniTask secondTransition = service.ExecuteTransitionAsync(() => second.Task);
            Assert.That(presenter.Visible, Is.True);

            first.TrySetResult();
            firstTransition.GetAwaiter().GetResult();
            Assert.That(presenter.Visible, Is.True);

            second.TrySetException(new InvalidOperationException("Load failed"));
            Assert.Throws<InvalidOperationException>(() => secondTransition.GetAwaiter().GetResult());
            Assert.That(presenter.Visible, Is.False);
        }

        [Test]
        public void TransitionTemplateRetainsOverlayAcrossUiReload()
        {
            const string path = "Assets/BlackholeFramework/LoadingScreen/Scripts/Presentation/UI/LoadingScreen.uxml";
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var host = new GameObject("Loading Transition Test");
            host.SetActive(false);
            try
            {
                var presenter = host.AddComponent<LoadingScreenPresenter>();
                var reload = typeof(LoadingScreenPresenter).GetMethod("OnLoadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
                TemplateContainer original = template.CloneTree();
                reload.Invoke(presenter, new object[] { null, original });
                presenter.OpenTransitionAsync().GetAwaiter().GetResult();

                TemplateContainer replacement = template.CloneTree();
                reload.Invoke(presenter, new object[] { null, replacement });
                var panel = replacement.Q<VisualElement>("TransitionPanel");
                Assert.That(panel.Q("TransitionSpinner"), Is.Not.Null);
                Assert.That(panel.ClassListContains("loading-transition-panel--visible"), Is.True);

                presenter.CloseTransitionAsync().GetAwaiter().GetResult();
                Assert.That(panel.ClassListContains("loading-transition-panel--visible"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private sealed class Presenter : ILoadingScreenPresenter
        {
            public bool Visible { get; private set; }
            public UniTask FadeIn(LoadingScreenType type) => UniTask.CompletedTask;
            public UniTask FadeOut(LoadingScreenType type) => UniTask.CompletedTask;

            public UniTask OpenTransitionAsync()
            {
                Visible = true;
                return UniTask.CompletedTask;
            }

            public UniTask CloseTransitionAsync()
            {
                Visible = false;
                return UniTask.CompletedTask;
            }
        }
    }
}
