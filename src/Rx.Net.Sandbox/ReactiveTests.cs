using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

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


            Assert.AreEqual(count, received.Count);
            Assert.AreEqual(null, lastException);
            Assert.AreEqual(true, isCompleted);

            for (var i = 0; i < count; i++)
                Assert.AreEqual(i+start, received[i]);
        }

        [Test]
        public void Observable_OnNext__WithTwoSubscribers__ExpectedEventsReceived()
        {
            var whatsTheMeaningOfLife = "What's the meaning of life?";

            IObservable<string> source = Observable.Create<string>(
                o =>
                {
                    o.OnNext(whatsTheMeaningOfLife);
                    o.OnNext("42");
                    return o.OnCompleted;
                }
            );
            
            List<string> received = new List<string>();
            source.Subscribe(s => received.Add(s));
            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(whatsTheMeaningOfLife, received[0]);
            Assert.AreEqual("42", received[1]);

            List<string> received2 = new List<string>();
            source.Subscribe(s => received2.Add(s));
            Assert.AreEqual(2, received2.Count);
            Assert.AreEqual(whatsTheMeaningOfLife, received2[0]);
            Assert.AreEqual("42", received2[1]);
        }
    }
}