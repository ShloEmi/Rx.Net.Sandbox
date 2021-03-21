using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Reactive.Testing;

using NUnit.Framework;


namespace Rx.Net.Sandbox
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void Timer__RegisteredForOnce__Expected_SubscriberCalledOnce()
        {
            List<long> observed = new List<long>();

            var ts = new TestScheduler();
            Observable.Timer(TimeSpan.FromTicks(10), ts).Subscribe(observed.Add);
            ts.AdvanceTo(9);
            observed.Count.Should().Be(0);
            ts.AdvanceTo(11);
            observed.Count.Should().Be(1);
            ts.AdvanceTo(1111);
            observed.Count.Should().Be(1);
        }

        [Test]
        public void Timer__DisposedBeforeTriggered__Expected_SubscriberNotCalled()
        {
            List<long> observed = new List<long>();

            var ts = new TestScheduler();
            IDisposable disposable = Observable.Timer(TimeSpan.FromTicks(100), ts).Subscribe(observed.Add);
            ts.AdvanceTo(90);
            observed.Count.Should().Be(0);

            disposable.Dispose();
            ts.AdvanceTo(111);
            observed.Count.Should().Be(0);
            ts.AdvanceTo(1111);
            observed.Count.Should().Be(0);
        }

        [Test]
        public void Interval__EveryTicks__Expected_SubscriberCalledWhenTimeTriggered()
        {
            List<long> observed = new List<long>();

            var ts = new TestScheduler();
            var period = 100;
            IDisposable disposable = Observable.Interval(TimeSpan.FromTicks(period), ts).Subscribe(observed.Add);

            ts.AdvanceTo(90);
            observed.Count.Should().Be(0);

            ts.AdvanceTo(111);
            observed.Count.Should().Be(1);

            ts.AdvanceTo(222);
            observed.Count.Should().Be(2);

            ts.AdvanceTo(2021);
            observed.Count.Should().Be(2021 / period);
        }

        [Test]
        public void Interval__Disposed__Expected_SubscriberCalledBeforeDisposedAndNotAfter()
        {
            List<long> observed = new List<long>();

            var ts = new TestScheduler();
            var period = 100;
            IDisposable disposable = Observable.Interval(TimeSpan.FromTicks(period), ts).Subscribe(observed.Add);

            ts.AdvanceTo(90);
            observed.Count.Should().Be(0);

            ts.AdvanceTo(111);
            observed.Count.Should().Be(1);

            disposable.Dispose();

            ts.AdvanceTo(222);
            observed.Count.Should().Be(1);

            ts.AdvanceTo(2021);
            observed.Count.Should().Be(1);
        }




        [TestCase(1, 1)]
        [TestCase(11, 100)]
        [TestCase(11, 1_000)]
        public void Observable_Range__Range__Expected_creates_an_observable_sequence__Test(int start, int count)
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
        public async Task Observable_OnNext__WithTwoSubscribers__ExpectedEventsReceived__Test()
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
        public void ToObservable__SomeCollection__SubscriberGotSameCollection__Test()
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
        public void Defer__ObservableStartedBeforeSubscriber__SubscriberGotSameCollectionWhenSubscribed__Test()
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

        [Ignore("Shlomi, TBC - unexpected behaviour!")]
        [Test]
        public void ToObservable__SomeCollection__SubscriberGotSameCollection2__Test()
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

        [Test]
        public void Dispose__TimeTicksAfterDisposedSubscriber__SubscriberShouldNotGetOnNextEvents__Test()
        {
            var scheduler = new TestScheduler();
            var collection = Enumerable.Range(1, 100).ToArray();
            var actual = new List<int>();


            var collectionObservable = collection.ToObservable(scheduler);

            var ticks = 10;
            using IDisposable subscriber = collectionObservable.Subscribe(i => actual.Add(i));
            scheduler.AdvanceTo(ticks);


            //act - disposing subscriber
            subscriber.Dispose();


            actual.Should()
                .ContainInOrder(Enumerable.Range(1, ticks)).And
                .HaveCount(ticks);

            scheduler.AdvanceBy(10);
            actual.Should()
                .ContainInOrder(Enumerable.Range(1, ticks)).And
                .HaveCount(ticks);
        }

        public class IntMessagesObservable
        {
            public IObservable<int> IntMessages(IScheduler scheduler = null)
                => Observable.Range(1, 100, scheduler);
        }

        [Test]
        public void ClassWithObservable__SubscribeUsingClassObservable__ExpectingMessages__Test()
        {
            var intMessagesObservable = new IntMessagesObservable();
            var scheduler = new TestScheduler();

            var actual = new List<int>();

            var ticks = 10;
            //act
            using IDisposable subscriber = intMessagesObservable.IntMessages(scheduler)
                .Subscribe(i => actual.Add(i));
            scheduler.AdvanceTo(ticks);


            actual.Should()
                .ContainInOrder(Enumerable.Range(1, ticks)).And
                .HaveCount(ticks);
        }

        [Test]
        public void Subscribe__UsingTestScheduler_CreateColdObservable__ExpectingMessagesAccordingToTicks__Test()
        {
            var scheduler = new TestScheduler();
            var actual = new List<int>();
            var ticks = 20;

            using var subscriber = scheduler.CreateColdObservable(
                    new Recorded<Notification<int>>(10, Notification.CreateOnNext(1)),
                    new Recorded<Notification<int>>(20, Notification.CreateOnNext(2)),
                    new Recorded<Notification<int>>(30, Notification.CreateOnNext(3)))
                .Subscribe(i => actual.Add(i));

            //act
            scheduler.AdvanceTo(ticks);

            actual.Should()
                .ContainInOrder(Enumerable.Range(1, 2)).And
                .HaveCount(2);
        }

        [Test]
        public void Subscribe__UsingTestScheduler_CreateColdObservable_And_Range__ExpectingMessagesAccordingToTicks__Test()
        {
            var scheduler = new TestScheduler();
            var actual = new List<int>();
            var ticks = 15;

            using var subscriber = scheduler.CreateColdObservable(
                    Enumerable.Range(1, 100)
                        .Select(i => new Recorded<Notification<int>>(i, Notification.CreateOnNext(i)))
                        .ToArray()
                )
                .Subscribe(i => actual.Add(i));

            //act
            scheduler.AdvanceTo(ticks);

            actual.Should()
                .ContainInOrder(Enumerable.Range(1, ticks)).And
                .HaveCount(ticks);
        }

        [Test]
        public void Subscribe__StringAsStreamOfChars__ExpectingMessagesAccordingToTicks__Test()
        {
            var scheduler = new TestScheduler();
            var actual = new List<char>();
            var ticks = 5;

            var stringStream = "gw5tn45ty13g4n9eghwe98t134tr5y";
            using var subscriber = stringStream
                .ToObservable(scheduler)
                .Where(c => c >= 'a' && c <= 'z')
                .Subscribe(c => actual.Add(c));

            //act
            scheduler.AdvanceTo(ticks);

            actual.Should()
                .HaveCountLessOrEqualTo(ticks);

        }
    }
}