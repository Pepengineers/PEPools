using System;
using System.Collections.Generic;
using NUnit.Framework;
using PEPEngineers.PEPools.Interfaces;
using PEPEngineers.PEPools.Runtime;

namespace PEPEngineers.PEPools.Tests
{
	internal sealed class PoolsTest
	{
		private StringFactory factory;
		private Pool<string> pool;

		[SetUp]
		public void SetUp()
		{
			factory = new StringFactory();
			pool = new Pool<string>(factory, factory);
		}

		[TearDown]
		public void Teardown()
		{
			pool.Clear();
		}

		[Test]
		public void CreatePool()
		{
			Assert.NotNull(pool);
		}

		[Test]
		public void CheckRented()
		{
			for (var i = 0; i < pool.BucketSize; i++) pool.Get();

			Assert.NotNull(pool);
			Assert.AreEqual(pool.BucketSize, factory.RentedCount);
			Assert.AreEqual(pool.BucketSize, factory.CreatedCount);
		}

		[Test]
		public void CheckReleased()
		{
			for (var i = 0; i < pool.BucketSize; i++) pool.Release(pool.Get());

			Assert.NotNull(pool);
			Assert.AreEqual(pool.BucketSize, factory.RentedCount);
			Assert.AreEqual(pool.BucketSize, factory.CreatedCount);
			Assert.AreEqual(pool.BucketSize, factory.ReleasedCount);
		}

		[Test]
		public void CheckClear()
		{
			pool.Release(pool.Get());
			pool.Clear();

			Assert.NotNull(pool);
			Assert.AreEqual(pool.BucketSize, factory.CreatedCount);
			Assert.AreEqual(pool.BucketSize, factory.DestroyedCount);
		}


		[Test]
		public void CheckDeletedWhenIncreasedMaxSize()
		{
			var list = new List<string>();
			for (var i = 0; i < pool.MaxCount * 2; i++) list.Add(pool.Get());

			foreach (var str in list) pool.Release(str);

			Assert.NotNull(pool);
			Assert.AreEqual(pool.MaxCount * 2, factory.RentedCount);
			Assert.AreEqual(pool.MaxCount * 2, factory.CreatedCount);
			Assert.AreEqual(pool.MaxCount * 2, factory.ReleasedCount);
			Assert.AreEqual(pool.MaxCount, factory.DestroyedCount);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void FailedCallbackDestroysItemThatWasNotReturned(bool failCreation)
		{
			var failingFactory = new FailingFactory { FailCreation = failCreation, FailRental = !failCreation };
			var failingPool = new Pool<object>(failingFactory, failingFactory, bucketSize: 1);

			Assert.Throws<InvalidOperationException>(() => failingPool.Get());
			Assert.AreEqual(1, failingFactory.DestroyedCount);
			Assert.AreEqual(1, failingFactory.DestroyedCallbackCount);
			Assert.AreEqual(0, failingPool.FreeCount);
			Assert.AreEqual(0, failingPool.RentedCount);

			failingFactory.FailCreation = failingFactory.FailRental = false;
			var item = failingPool.Get();
			Assert.NotNull(item);
			Assert.AreEqual(1, failingPool.RentedCount);
			failingPool.Release(item);
			failingPool.Clear();
			Assert.AreEqual(2, failingFactory.DestroyedCount);
			Assert.AreEqual(2, failingFactory.DestroyedCallbackCount);
			Assert.AreEqual(0, failingPool.RentedCount);
		}

		private sealed class FailingFactory : IPoolFactory<object>, IPoolCallback<object>
		{
			public bool FailCreation;
			public bool FailRental;
			public int DestroyedCount;
			public int DestroyedCallbackCount;

			public object Create() => new object();
			public void Destroy(object item) => DestroyedCount++;
			public void OnItemCreated(in object item)
			{
				if (FailCreation) throw new InvalidOperationException("Creation callback failed.");
			}
			public void OnItemRented(in object item)
			{
				if (FailRental) throw new InvalidOperationException("Rental callback failed.");
			}
			public void OnItemReleased(in object item) { }
			public void OnItemDestroyed(in object item) => DestroyedCallbackCount++;
		}
	}
}
