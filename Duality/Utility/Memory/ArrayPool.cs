using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Duality.Utility.Memory
{
	public static class ArrayPool<T>
	{
		private static List<int> _poolSizes = new List<int> { 256, 1024, 16384, 32768, 65536, 1048576, 4194304, 16777216, 33554432, 67108864 };

		private static readonly Dictionary<int, Stack<T[]>> _pool = new Dictionary<int, Stack<T[]>>();
		private static readonly Dictionary<int, int> _poolCounts = new Dictionary<int, int>();
		private static readonly Dictionary<int, int> _averageWastageByPool = new Dictionary<int, int>();
		private static readonly Dictionary<int, int> _numAllocationsByPool = new Dictionary<int, int>();
		private static readonly Dictionary<string, int> _allocationsByMethod = new Dictionary<string, int>();

		private static int _totalMemoryAllocated;
		private static int _totalMemoryInAllocatedPools;

		public static readonly T[] Empty = new T[0];

		public static DisposableValue<T[]> AllocateDisposable(int size)
		{
			var array = Allocate(size);
			return DisposableValue<T[]>.Create(array, () => Free(array), size);
		}

		public static DisposableValue<T[]> Resize(DisposableValue<T[]> source, int size)
		{
			if (size < 0) throw new ArgumentOutOfRangeException("size", "Must be positive.");

			var dest = AllocateDisposable(size);
			Array.Copy(source.Value, dest.Value, size < source.Value.Length ? size : source.Value.Length);
			source.Dispose();
			return dest;
		}

		public static void Clear()
		{
			_pool.Clear();
			_poolCounts.Clear();
			_totalMemoryAllocated = 0;
			_totalMemoryInAllocatedPools = 0;
		}

		public static void Report()
		{
			Log.Core.Write("ArrayPool<{0}>: Using '{1:N0}'B", typeof(T).Name, _totalMemoryAllocated);
			Log.Core.PushIndent();
			Log.Core.Write("{0:N0} bytes is unreleased", _totalMemoryInAllocatedPools);

			foreach (var pool in _poolCounts)
			{
				Log.Core.Write("{0} pool(s) of size '{1}' allocated", pool.Value, pool.Key);
			}

			Log.Core.Write("=========================================================== Wastage ===========================================================");
			foreach (var keyValuePair in _averageWastageByPool)
			{
				Log.Core.Write("{0} bytes wasted on average in pool {1}", keyValuePair.Value, keyValuePair.Key);
			}

			ReportAllocationsByMethod();

			Log.Core.PopIndent();
		}

		private static T[] Allocate(int size)
		{
			if (size < 0) throw new ArgumentOutOfRangeException("size", "Must be positive.");

			if (size == 0) return Empty;

			var poolSize = GetClosestPoolSize(size);

			UpdateAllocationsCount(poolSize);
			CalculateWastage(size, poolSize);

			Stack<T[]> candidates;
			if (_pool.TryGetValue(poolSize, out candidates) && candidates.Count > 0)
			{
				_totalMemoryInAllocatedPools += GetAllocationSizeInBytes(poolSize);
				return candidates.Pop();
			}

			if (_poolCounts.ContainsKey(poolSize))
				_poolCounts[poolSize]++;
			else
				_poolCounts.Add(poolSize, 1);

			_totalMemoryAllocated += GetAllocationSizeInBytes(poolSize);
			_totalMemoryInAllocatedPools += GetAllocationSizeInBytes(poolSize);

			UpdateAllocationMap(poolSize);

			return new T[poolSize];
		}

		private static void UpdateAllocationsCount(int poolSize)
		{
			if (_numAllocationsByPool.ContainsKey(poolSize))
				_numAllocationsByPool[poolSize]++;
			else
				_numAllocationsByPool.Add(poolSize, 1);
		}

		private static void CalculateWastage(int size, int poolSize)
		{
			if (_averageWastageByPool.ContainsKey(poolSize))
				_averageWastageByPool[poolSize] = (_averageWastageByPool[poolSize] + (poolSize - size)) / _numAllocationsByPool[poolSize];
			else
				_averageWastageByPool.Add(poolSize, (poolSize - size));
		}

		private static int GetClosestPoolSize(int size)
		{
			var poolSize = _poolSizes[_poolSizes.Count - 1];
			var index = _poolSizes.Count - 1;
			while (poolSize >= size && index > 0)
				poolSize = _poolSizes[--index];

			if (poolSize < size && index != _poolSizes.Count - 1)
				poolSize = _poolSizes[index + 1];

			return poolSize;
		}

		private static void Free(T[] array)
		{
			if (array == null) throw new ArgumentNullException("array");

			if (array.Length == 0) return;

			Stack<T[]> candidates;
			if (!_pool.TryGetValue(array.Length, out candidates))
				_pool.Add(array.Length, candidates = new Stack<T[]>());

			_totalMemoryInAllocatedPools -= GetAllocationSizeInBytes(array.Length);
			candidates.Push(array);
		}

		/// <summary>
		/// Keeps a tally of accumulated memory allocated by all methods. This would be far more useful if it was hierarchical!
		/// </summary>
		/// <param name="size"></param>
		[Conditional("DEBUG")]
		private static void UpdateAllocationMap(int size)
		{
			var stackTrace = new StackTrace(true);
			foreach (var stackFrame in stackTrace.GetFrames().Skip(2))
			{
				var name = stackFrame.GetMethod().DeclaringType.FullName + "." + stackFrame.GetMethod().Name;
				if (_allocationsByMethod.ContainsKey(name))
					_allocationsByMethod[name] += GetAllocationSizeInBytes(size);
				else
					_allocationsByMethod.Add(name, GetAllocationSizeInBytes(size));
			}
		}

		private static int GetAllocationSizeInBytes(int size)
		{
			return size * Marshal.SizeOf(typeof(T));
		}

		[Conditional("DEBUG")]
		private static void ReportAllocationsByMethod()
		{
			var allocationsByMethod = _allocationsByMethod.OrderByDescending(a => a.Value);
			Log.Core.Write(
				"=========================================================== Allocations by method ==============================================================");
			Log.Core.Write(
				"Method name                                                                                                                    Allocated (bytes)");
			foreach (var keyValuePair in allocationsByMethod)
			{
				Log.Core.Write("{0}{1}{2:N0} bytes", keyValuePair.Key,
					string.Join("", Enumerable.Repeat(".", 128 - keyValuePair.Key.Length)), keyValuePair.Value);
			}
		}
	}

	public class DisposableValue<T> : IDisposable
	{
		private readonly Action _dispose;

		public DisposableValue(T value, Action dispose)
		{
			_dispose = dispose;
			Value = value;
		}

		public T Value { get; private set; }
		public int RequestedSize { get; set; }

		public void Dispose()
		{
			_dispose();
		}

		public static DisposableValue<T> Create(T value, Action dispose, int size)
		{
			return new DisposableValue<T>(value, dispose){RequestedSize = size};
		}
	}
}