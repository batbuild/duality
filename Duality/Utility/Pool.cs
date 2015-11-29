using System;
using System.Collections.Generic;

namespace Duality.Utility
{
	internal class Pool<T>
	{
		private Queue<T> _objects = new Queue<T>();
		private List<T> _inUse = new List<T>();

		public T Acquire(Func<T> create)
		{
			T obj;
			if (_objects.Count > 0)
			{
				obj = _objects.Dequeue();
				_inUse.Add(obj);
				return obj;
			}

			obj = create();
			_inUse.Add(obj);
			return obj;
		}

		public void ReleaseAll()
		{
			foreach (var obj in _inUse)
			{
				_objects.Enqueue(obj);
			}

			_inUse.Clear();
		}
	}
}