using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Duality.Utility.Jobs
{
	/// <summary>
	/// A simple multi-threaded job queue. All _jobs must be submitted to the list before calling SignalWorkAvailable, otherwise 
	/// all bets are off. The DoWork method signals work complete when no items are left in the work queue, so if you don't do
	/// that, the calling code might continue before any _jobs submitted after that signal have completed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class JobList<T> where T:Job<T>
	{
		private Thread[] _workerThreads;
		private ConcurrentQueue<T> _jobs = new ConcurrentQueue<T>();
		private ManualResetEvent _workAvailableHandle = new ManualResetEvent(false);
		private ManualResetEvent[] _workCompleteHandles;

		public void AddJob(T job)
		{
			if (_workerThreads == null)
			{
				_workerThreads = new Thread[Environment.ProcessorCount];
				_workCompleteHandles = new ManualResetEvent[_workerThreads.Length];
				for (var i = 0; i < _workerThreads.Length; i++)
				{
					_workCompleteHandles[i] = new ManualResetEvent(false);
					_workerThreads[i] = new Thread(DoWork);
					_workerThreads[i].Start(_workCompleteHandles[i]);
				}
			}

			_jobs.Enqueue(job);
		}

		public void SignalWorkAvailable()
		{
			_workAvailableHandle.Set();
		}

		public void WaitForWorkToComplete()
		{
			WaitHandle.WaitAll(_workCompleteHandles);
			_workAvailableHandle.Reset();

			foreach (var workCompleteHandle in _workCompleteHandles)
			{
				workCompleteHandle.Reset();
			}
		}
		
		private void DoWork(object waitEvent)
		{
			while (true)
			{
				_workAvailableHandle.WaitOne();

				T job;
				if (_jobs.TryDequeue(out job) == false)
				{
					((ManualResetEvent)waitEvent).Set();
					continue;
				}

				job.DoWork();
			}
		}
	}
}