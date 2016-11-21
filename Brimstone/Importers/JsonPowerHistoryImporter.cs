using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Brimstone.PowerActions;

namespace Brimstone.Importers
{
	public class JsonPowerHistoryImporter : PowerHistoryImporter<JsonPowerHistoryImporter>
	{
		public override PowerHistory ImportFirst(StreamReader stream) {
			return Import(stream).First();
		}

		public override IEnumerable<PowerHistory> Import(StreamReader stream) {
			using (JsonTextReader reader = new JsonTextReader(stream)) {
				var logs = JArray.Load(reader);

				return logs.Select(x => importPowerHistory((JArray)x));
			}
		}

		private PowerHistory importPowerHistory(JArray log) {
			var history = new PowerHistory();
			foreach (JToken entry in log) {
				var powerAction = importPowerAction((JObject)entry);
				if (powerAction != null)
					history.Add(powerAction);
			}

			return history;
		}

		private PowerAction importPowerAction(JObject action) {
			var type = (string)action["Type"];

			if (type == "TAG_CHANGE")
				return importTagChange(action);
			else if (type == "BLOCK_START")
				return importBlockStart(action);
			else if (type == "BLOCK_END")
				return importBlockEnd(action);

			// All other types of log entries are ignored
			return null;
		}

		private PowerAction importTagChange(JObject action) {
			var entityId = (int)action["Entity"];

			// Get tag change name and value pair
			var change = ((IEnumerable<KeyValuePair<string, JToken>>)action["Change"]).First();
			var tagString = (string)change.Key;

			// Unknown tag?
			if (tagString.Length == 0)
				return null;

			var tagName = (GameTag)Enum.Parse(typeof(GameTag), tagString);

			int tagValue;
			if (change.Value.Type == JTokenType.String) {
				// Convert string values to int using the tag's enum type
				Type tagType = Tag.TypedTags[tagName];
				tagValue = (int)Enum.Parse(tagType, (string)change.Value);
			} else {
				// Otherwise, use the given int	directly
				tagValue = (int)change.Value;
			}

			return new TagChange(entityId, tagName, tagValue);
		}

		private PowerAction importBlockStart(JObject action) {
			var type = (BlockType)(int)action["BlockType"];
			var source = (int)action["Entity"];
			var target = (int)action["Target"];
			var index = (int)action["EffectIndex"];

			return new BlockStart(type, source, target, index);
		}

		private PowerAction importBlockEnd(JObject action) {
			return new BlockEnd();
		}
	}
}
