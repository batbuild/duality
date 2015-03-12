using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using OpenTK.Input;

namespace Duality
{
	/// <summary>
	/// Provides access to a set of <see cref="GamepadInput">GamepadInputs</see>.
	/// </summary>
	public sealed class GamepadInputCollection : UserInputCollection<GamepadInput,IGamepadInputSource>
	{
		private const int MaxSupportedDevices = 4;

		protected override GamepadInput CreateInput(IGamepadInputSource source)
		{
			GamepadInput input = new GamepadInput();
			input.Source = source;
			return input;
		}
		protected override GamepadInput CreateDummyInput()
		{
			return new GamepadInput(true);
		}

		public void AddGlobalDevices()
		{
			for (var i = 0; i < MaxSupportedDevices; i++)
			{
				GlobalGamepadInputSource gamepad = new GlobalGamepadInputSource(i);
				gamepad.UpdateState();
				if (!gamepad.IsAvailable) continue;

				this.AddSource(gamepad);
			}
		}
	}
}
