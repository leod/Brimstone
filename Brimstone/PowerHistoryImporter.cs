using System.IO;

namespace Brimstone
{
	public interface IPowerHistoryImporter
	{
		PowerHistory Import(StreamReader stream);
	}
}
