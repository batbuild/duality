using System.Collections.Generic;
using System.IO;

namespace Duality.Editor
{
	/// <summary>
	/// Certain applications, Photoshop and Excel to name two, have specific, and weird, behaviours when saving files, 
	/// and can create and rename several temporary files for one save operation. This results in multiple file events 
	/// being raised, and Duality doesn't  have any way to recognize these events as all coming from a single user
	/// action. This interface can be implemented in plugins to manually recognize known sequences of file events to
	/// help Duality do the right thing. Implementations will be automatically plugged in to the FileEventManager, so all
	/// you have to do is create your classes. No need to manually register them.
	/// </summary>
	public interface IFileSystemEventPattern
	{
		/// <summary>
		/// Tries to match a list of file system events to a recognized pattern. Implementations should remove matched
		/// events from the dirEventList collection so that they are not matched subsequently by Duality.
		/// </summary>
		/// <param name="dirEventList">The list of file events</param>
		/// <param name="basePath">The relative base path from Game where the file events took place. Will usually be
		/// Data or Source\Media.</param>
		/// <param name="fileSystemEventArgs">A FileSystemEventArgs instance that represents the users' actual intention.
		/// Will probably in most cases represent a Changed event, as saving is the most common culprit of strange file 
		/// event patterns.</param>
		/// <returns>True if a match was made, otherwise false.</returns>
		bool Match(List<FileSystemEventArgs> dirEventList, string basePath, out FileSystemEventArgs fileSystemEventArgs);
	}
}