using System;
using System.Collections.Generic;
using Duality.Resources;

namespace Duality.Helpers
{
	public static class ExtMethodsGameObject
	{
		private static Stack<List<ICmpHandlesMessages>> _receiverStack = new Stack<List<ICmpHandlesMessages>>();
		private static List<ICmpHandlesMessages> _receivers = new List<ICmpHandlesMessages>();
		private static List<ICmpHandlesMessages> _currentReceivers = _receivers; 

		public static void SendMessage(this Component sender, GameMessage msg, string targetName)
		{
			if (string.IsNullOrEmpty(targetName))
			{
				SendMessage(sender, msg);
			}
			else
			{
				SendMessage(sender, msg, Scene.Current.FindGameObject(targetName));
			}
		}

		public static void SendMessage(this Component sender, GameMessage msg, GameObject target = null)
		{
			SendMessage(sender.GameObj, msg, target);
		}

		public static void SendMessage(this GameObject sender, GameMessage msg, GameObject target)
		{
			if (msg == null)
			{
				Log.Game.WriteWarning("ExtMethodsGameObject: Tried to send a null message.\n{0}", Environment.StackTrace);
				return;
			}

			// if there is already a list on the stack then we have re-entered SendMessage i.e. a HandleMessage method has
			// called SendMessage
			if (_receiverStack.Count > 0)
				_currentReceivers = new List<ICmpHandlesMessages>();

			_receiverStack.Push(_currentReceivers);
			_currentReceivers.Clear();

			if (target != null)
			{
				if (!target.Active)
				{
					_currentReceivers = _receiverStack.Pop();

					Log.Game.WriteWarning("{0}: Message type '{1}' sent to inactive game object '{2}'. Inactive game objects don't respond to messages", Log.CurrentMethod(), msg.GetType().Name, target);
					return;
				}

				target.GetComponents(_currentReceivers);
			}
			else
			{
				Scene.Current.FindComponents(_currentReceivers);
			}


			try
			{
				for (int i = _currentReceivers.Count - 1; i >= 0; i--)
				{
					var receiver = _currentReceivers[i];
				
					if (receiver == null || ((Component)receiver).Disposed || ((Component)receiver).Active == false)
						continue;

					receiver.HandleMessage(sender, msg);

					if (i > _currentReceivers.Count) 
						i = _currentReceivers.Count;

					if (msg.Handled)
						break;
				}
			}
			finally
			{
				_currentReceivers = _receiverStack.Pop();
			}
		}
	}
}
