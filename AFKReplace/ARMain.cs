using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.Piping;

namespace AFKReplace
{
	[PluginDetails(
		author = "Mith",
		name = "AFKReplace",
		description = "When someone is AFK for a configured amount of time they get replaced by someone spectating.",
		id = "mith.AFKReplace",
		version = "1.0.0",
		configPrefix = "ar",
		SmodMajor = 3,
		SmodMinor = 9,
		SmodRevision = 2
		)]
	class DRMain : Plugin
	{
		[ConfigOption]
		public readonly bool disable = false;

		[ConfigOption]
		public readonly bool broadcast = true;

		[ConfigOption]
		public readonly float afk_time = 120f;

		[ConfigOption]
		public readonly float afk_distance = 0.11f;

		[ConfigOption]
		public readonly string will_replace_string = "You will be AFK replaced in {0} seconds if you don't move!";

		[ConfigOption]
		public readonly string replaced_string = "You have been replaced because you were AFK!";
		public override void OnDisable()
		{
			this.Info($"{this.Details.name}(Version:{this.Details.version}) has been disabled.");
		}

		public override void OnEnable()
		{
			this.Info($"{this.Details.name}(Version:{this.Details.version}) has been enabled.");
		}

		public override void Register()
		{
			this.AddCommand(this.Details.configPrefix + "_version", new ARVersion(this));
			this.AddCommand(this.Details.configPrefix + "_disable", new ARDisable(this));
			this.AddEventHandlers(new AREventHandler(this));
		}
	}
}