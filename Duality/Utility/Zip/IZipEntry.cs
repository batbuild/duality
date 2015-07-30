using System.IO;

namespace Duality.Android.Utility.Zip
{
	public interface IZipEntry
	{
		long Length { get; }
		string FullName { get; }
		
		Stream Open();
	}
}