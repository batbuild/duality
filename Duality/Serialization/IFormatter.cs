using System;

namespace Duality.Serialization
{
	public interface IFormatter : IDisposable
	{
		/// <summary>
		/// [GET] Can this serializer read data?
		/// </summary>
		bool CanRead { get; }

		/// <summary>
		/// [GET] Can this serializer write data?
		/// </summary>
		bool CanWrite { get; }

		/// <summary>
		/// Reads an object including all referenced objects.
		/// </summary>
		/// <returns>The object that has been read.</returns>
		T ReadObject<T>();

		/// <summary>
		/// Writes the specified object including all referenced objects.
		/// </summary>
		/// <param name="obj">The object to write.</param>
		void WriteObject<T>(T t);
	}
}