using System.Collections.Generic;
using Duality.Drawing;

namespace Duality.Utility
{
	/// <summary>
	/// Implement this interface to plug a different type of spatial subdivision into the engine.
	/// </summary>
	public interface IRendererVisibilityStrategy
	{
		IEnumerable<ICmpRenderer> QueryVisibleRenderers(IDrawDevice device);
		void Update();
	}
}