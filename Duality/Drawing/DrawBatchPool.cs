using System;
using System.Collections.Generic;
using Duality.Resources;

namespace Duality.Drawing
{
	public class DrawBatchPool
	{
		private Dictionary<PoolKey, Queue<IDrawBatch>> _pool = new Dictionary<PoolKey, Queue<IDrawBatch>>();

		public IDrawBatch Get<T>(BatchInfo material, VertexMode vertexMode, float zSortIndex) where T : struct, IVertexData
		{
			var poolKey = new PoolKey {Material = material, VertexMode = vertexMode, ZSortIndex = zSortIndex};
			Queue<IDrawBatch> queue;
			if (_pool.TryGetValue(poolKey, out queue) == false)
			{
				queue = new Queue<IDrawBatch>();
				_pool.Add(poolKey, queue);
			}

			return queue.Count == 0 ? new DrawBatch<T>(material, vertexMode, null, 0, zSortIndex) { Pooled = true } : queue.Dequeue();
		}

		public void Release(IDrawBatch drawBatch)
		{
			var poolKey = new PoolKey { Material = drawBatch.Material, VertexMode = drawBatch.VertexMode, ZSortIndex = drawBatch.ZSortIndex };

			Queue<IDrawBatch> queue;
			if (_pool.TryGetValue(poolKey, out queue) == false)
			{
				queue = new Queue<IDrawBatch>();
				_pool.Add(poolKey, queue);
			}

			queue.Enqueue(drawBatch);
		}

		private struct PoolKey : IEquatable<PoolKey>
		{
			public BatchInfo Material;
			public VertexMode VertexMode;
			public float ZSortIndex;

			private int hashCode;

			public static bool operator ==(PoolKey left, PoolKey right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(PoolKey left, PoolKey right)
			{
				return !left.Equals(right);
			}

			public bool Equals(PoolKey other)
			{
				return Equals(Material, other.Material) && VertexMode == other.VertexMode && ZSortIndex == other.ZSortIndex;
			}

			public override int GetHashCode()
			{
				if (this.hashCode != 0)
					return this.hashCode;

				unchecked
				{
					var hashCode = (Material != null ? Material.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (int)VertexMode;
					hashCode = (hashCode * 397) ^ ZSortIndex.GetHashCode();
					this.hashCode = hashCode;
					return this.hashCode;
				}
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				return obj is PoolKey && Equals((PoolKey)obj);
			}
		}
	}
}