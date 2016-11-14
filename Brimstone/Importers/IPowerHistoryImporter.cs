using System.IO;
using System.Collections.Generic;

namespace Brimstone.Importers
{
	public interface IPowerHistoryImporter
	{
		PowerHistory ImportFirst(StreamReader stream);
		IEnumerable<PowerHistory> Import(StreamReader stream);
	}

	public static class PowerHistoryImporterExtensions
	{
		public static PowerHistory ImportFirst(this IPowerHistoryImporter importer, string filename) {
			using (StreamReader stream = new StreamReader(filename)) {
				return importer.ImportFirst(stream);
			}
		}

		public static IEnumerable<PowerHistory> Import(this IPowerHistoryImporter importer, string filename) {
			using (StreamReader stream = new StreamReader(filename)) {
				return importer.Import(stream);
			}
		}
	}
}
