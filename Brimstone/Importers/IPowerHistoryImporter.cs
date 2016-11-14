using System.IO;
using System.Collections.Generic;

namespace Brimstone.Importers
{
	public interface IPowerHistoryImporter
	{
		PowerHistory Import(StreamReader stream);
	}
}
