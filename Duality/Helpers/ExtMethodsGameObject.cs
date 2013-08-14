﻿using System.Collections.Generic;
using System.Linq;
using Duality.Resources;

namespace Duality.Helpers
{
	public static class ExtMethodsGameObject
	{
		public static void BroadcastMessage(this Component sender, GameMessage msg, string targetName = null)
		{
			IEnumerable<ICmpHandlesMessages> receivers;
			
			if (targetName != null)
			{
				var targetObject = Scene.Current.FindGameObjects(targetName);
				receivers = targetObject.GetComponents<ICmpHandlesMessages>().ToList();
			}
			else
			{
				receivers = Scene.Current.FindComponents<ICmpHandlesMessages>();
			}

			foreach (var receiver in receivers)
			{
				if (((Component) receiver).Active == false)
					continue;

				receiver.HandleMessage(sender, msg);
			}
		}

		public static void BroadcastMessage(this Component sender, GameMessage msg, GameObject target)
		{
			if (target == null)
				return;

			if (msg == null)
				return;

			if (!target.Active)
				return;

			var receivers = target.GetComponents<ICmpHandlesMessages>();

			foreach(var receiver in receivers)
			{
				if ((receiver as Component).Active == false)
					continue;

				receiver.HandleMessage(sender, msg);
			}
		}
	}
}