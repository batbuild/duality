using System.Collections.Generic;
using System.Linq;
using Duality.Drawing;

namespace Duality.Utility
{
	public class DefaultRendererVisibilityStrategy : IRendererVisibilityStrategy
	{
		private readonly IEnumerable<Component> _renderers;

		public DefaultRendererVisibilityStrategy(IEnumerable<Component> renderers)
		{
			_renderers = renderers;
		}

		public IEnumerable<ICmpRenderer> QueryVisibleRenderers(IDrawDevice device)
		{
			return _renderers.Where(r => r.Active && (r as ICmpRenderer).IsVisible(device)).OfType<ICmpRenderer>();
		}

		public void Update()
		{
			
		}
	}
}