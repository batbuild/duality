using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if ! __ANDROID__
using System.Drawing;
#else
using Android.Graphics;
#endif
namespace Duality.Cloning.Surrogates
{
	public class BitmapSurrogate : Surrogate<Bitmap>
	{
#if ! __ANDROID__
		public override Bitmap CreateTargetObject(CloneProvider provider)
		{
			return new Bitmap(this.RealObject.Width, this.RealObject.Height);
		}
		public override void CopyDataTo(Bitmap targetObj, CloneProvider provider)
		{
			Bitmap target = targetObj as Bitmap;
			target.SetPixelDataIntArgb(this.RealObject.GetPixelDataIntArgb());
		}
#endif

#if __ANDROID__
		public override void CopyDataTo(Bitmap targetObj, CloneProvider provider)
		{
			throw new NotImplementedException();
		}
#endif
	}
}
