using System.IO;
using System.Collections.Generic;

namespace Brimstone.Importers
{
	public interface IPowerHistoryImporter
	{
		PowerHistory ImportFirst(StreamReader stream);
		IEnumerable<PowerHistory> Import(StreamReader stream);
	}

	public abstract class PowerHistoryImporter<T> : IPowerHistoryImporter where T : PowerHistoryImporter<T>, new()
	{
		public abstract PowerHistory ImportFirst(StreamReader stream);
		public abstract IEnumerable<PowerHistory> Import(StreamReader stream);

		public PowerHistory ImportFirst(string filename) {
			using (StreamReader stream = new StreamReader(filename))
				return ImportFirst(stream);
		}

		public IEnumerable<PowerHistory> Import(string filename) {
			using (StreamReader stream = new StreamReader(filename))
				return Import(stream);
		}

		public static PowerHistory ImportFirstFrom(StreamReader stream) {
			return new T().ImportFirst(stream);
		}

		public static PowerHistory ImportFirstFrom(string filename) {
			return new T().ImportFirst(filename);
		}

		public static IEnumerable<PowerHistory> ImportFrom(StreamReader stream) {
			return new T().Import(stream);
		}

		public static IEnumerable<PowerHistory> ImportFrom(string filename) {
			return new T().Import(filename);
		}
	}
}
