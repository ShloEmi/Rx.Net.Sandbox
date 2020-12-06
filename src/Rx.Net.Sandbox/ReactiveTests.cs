using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Reactive.Testing;

using NUnit.Framework;

namespace Rx.Net.Sandbox
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(1,1)]
        [TestCase(11,100)]
        [TestCase(11,1_000)]
        public void Observable_Range__Range__Expected_creates_an_observable_sequence(int start, int count)
        {
            List<int> received = new List<int>(count);

            //act
            IObservable<int> source = Observable.Range(start, count);

            Exception lastException = null;
            var isCompleted = false;
            using IDisposable subscription = source.Subscribe(
                i => received.Add(i),
                ex => lastException = ex,
                () => isCompleted = true);


            lastException.Should().BeNull();
            isCompleted.Should().BeTrue();
            received.Should()
                .HaveCount(count).And
                .ContainInOrder(Enumerable.Range(start, count));
        }

        [Test]
        public async Task Observable_OnNext__WithTwoSubscribers__ExpectedEventsReceived()
        {
            TimeSpan waitTimeSpan = TimeSpan.FromMilliseconds(10);
            List<long> lastReported = new List<long>();

            lastReported.Should().HaveCount(0);

            //act
            Observable.Interval(waitTimeSpan).Subscribe(l => lastReported.Add(l));

            await Task.Delay(waitTimeSpan * 10);
            lastReported.Should().HaveCountGreaterThan(0);
        }

        [Ignore("Shlomi, TBC - unexpected behaviour!")]
        [Test]
        public void ToObservable__SomeCollection__SubscriberGotSameCollection()
        {
            var scheduler = new TestScheduler();
            var collection = new List<int> {1, 2, 3};
            var actual = new List<int>();

            //act
            IObservable<int> collectionObservable = collection.ToObservable(scheduler);

            using IDisposable subscriber1 = collectionObservable.Subscribe(i => actual.Add(i));
            scheduler.Start();
            actual.Should().ContainInOrder(collection);

            Assert.Fail("Shlomi, TBC - unexpected behaviour!");
            actual.Clear();
            using IDisposable subscriber2 = collectionObservable.Subscribe(i => actual.Add(i));
            scheduler.Start();
            actual.Should().ContainInOrder(collection);

        }

        [Ignore("Shlomi, TBC - unexpected behaviour!")]
        [Test]
        public void Defer__ObservableStartedBeforeSubscriber__SubscriberGotSameCollectionWhenSubscribed()
        {
            var scheduler = new TestScheduler();
            var collection = new List<int> {1, 2, 3};
            var actual = new List<int>();


            //act
            var collectionObservable = Observable.Defer(() => collection.ToObservable(scheduler));

            using IDisposable subscriber1 = collectionObservable.Subscribe(i => actual.Add(i));
            scheduler.Start();
            actual.Should()
                .ContainInOrder(collection).And
                .HaveCount(3);

            Assert.Fail("Shlomi, TBC - unexpected behaviour!");
            actual.Clear();
            using IDisposable subscriber2 = collectionObservable.Subscribe(i => actual.Add(i));
            scheduler.Start();
            actual.Should()
                .ContainInOrder(collection).And
                .HaveCount(3);

        }
    }
}