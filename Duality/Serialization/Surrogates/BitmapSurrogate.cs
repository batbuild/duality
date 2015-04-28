using System;
#if ! __ANDROID__
using System.Drawing;
#else
using Android.Graphics;
#endif

namespace Duality.Serialization.Surrogates
{
	public class BitmapSurrogate : Surrogate<Bitmap>
	{
		public override void WriteConstructorData(IDataWriter writer)
		{
			writer.WriteValue("width", this.RealObject.Width);
			writer.WriteValue("height", this.RealObject.Height);
		}
#if __ANDROID__
		public override void WriteData(IDataWriter writer)
		{
			throw new NotImplementedException();
		}

		public override void ReadData(IDataReader reader)
		{
			throw new NotImplementedException();
		}
#endif

#if ! __ANDROID__
		public override void WriteData(IDataWriter writer)
		{
			int[] data = this.RealObject.GetPixelDataIntArgb();

			writer.WriteValue("data", data);
		}

		public override object ConstructObject(IDataReader reader, Type objType)
		{
			int width = reader.ReadValue<int>("width");
			int height = reader.ReadValue<int>("height");

			return new Bitmap(width, height);
		}
		public override void ReadData(IDataReader reader)
		{
			int[] data = reader.ReadValue<int[]>("data");

			this.RealObject.SetPixelDataIntArgb(data);
		}
#endif
	}
}
