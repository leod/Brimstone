using System.IO;
using System.Collections.Generic;

namespace Brimstone.Importers
{
	public interface IPowerHistoryImporter
	{
		PowerHistory ImportFirst(StreamReader stream);
		IEnumerable<PowerHistory> Import(StreamReader stream);
	}
}
