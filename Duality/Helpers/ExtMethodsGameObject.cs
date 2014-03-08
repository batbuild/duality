using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Resources;

namespace Duality.Helpers
{
	public static class ExtMethodsGameObject
	{
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

			IEnumerable<ICmpHandlesMessages> receivers;

			if (target != null)
			{
				if (!target.Active)
					return;

				receivers = target.GetComponents<ICmpHandlesMessages>().ToList();
			}
			else
			{
				receivers = Scene.Current.FindComponents<ICmpHandlesMessages>();
			}

			foreach (var receiver in receivers)
			{
				if (((Component)receiver).Active == false)
					continue;

				receiver.HandleMessage(sender, msg);

				if (msg.Handled)
					break;
			}
		}
	}
}
