using System.Collections.Generic;
using Blackhole.Messaging.Composition;
using Blackhole.Messaging.Contract;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace Blackhole.Development.Tests
{
    public sealed class MessagingTests
    {
        [Test]
        public void InstallerResolvesAdaptersAndDisposingSubscriptionStopsDelivery()
        {
            MessagingInstaller installer = ScriptableObject.CreateInstance<MessagingInstaller>();
            try
            {
                var builder = new ContainerBuilder();
                installer.InstallModule(builder);
                using var container = builder.Build();
                var publisher = container.Resolve<IEventPublisher<int>>();
                var subscriber = container.Resolve<IEventSubscriber<int>>();
                var received = new List<int>();
                using var subscription = subscriber.Subscribe(received.Add);
                publisher.Publish(7);
                subscription.Dispose();
                publisher.Publish(8);
                Assert.That(received, Is.EqualTo(new[] { 7 }));
            }
            finally
            {
                Object.DestroyImmediate(installer);
            }
        }

        [Test]
        public void SeparateContainersDoNotShareEvents()
        {
            MessagingInstaller installer = ScriptableObject.CreateInstance<MessagingInstaller>();
            try
            {
                var firstBuilder = new ContainerBuilder();
                var secondBuilder = new ContainerBuilder();
                installer.InstallModule(firstBuilder);
                installer.InstallModule(secondBuilder);
                using var first = firstBuilder.Build();
                using var second = secondBuilder.Build();
                int received = 0;
                using var subscription = second.Resolve<IEventSubscriber<int>>().Subscribe(_ => received++);
                first.Resolve<IEventPublisher<int>>().Publish(1);
                Assert.That(received, Is.Zero);
                second.Resolve<IEventPublisher<int>>().Publish(2);
                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(installer);
            }
        }
    }
}
