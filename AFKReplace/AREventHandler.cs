using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using UnityEngine;
using PlayableScps;

namespace AFKReplace
{
	internal class AREventHandler : IEventHandlerWaitingForPlayers, IEventHandlerPlayerJoin
	{
		private readonly Plugin plugin;
		public AREventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}
		public string WillBeReplaced;
		public string HaveBeenReplaced;
		public float AFK_Time;
		public float AFK_Distance;
		public bool ShouldBroadcast;

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			AddAFKManager(ReferenceHub.GetHub((GameObject)ev.Player.GetGameObject()));
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (plugin.GetConfigBool(plugin.Details.configPrefix + "_disable"))
			{
				plugin.PluginManager.DisablePlugin(plugin);
			}
			AFK_Time = plugin.GetConfigFloat(plugin.Details.configPrefix + "_afk_time");
			if(AFK_Time < 15)
			{
				AFK_Time = 15;
			}
			WillBeReplaced = plugin.GetConfigString(plugin.Details.configPrefix + "_will_replace_string");
			HaveBeenReplaced = plugin.GetConfigString(plugin.Details.configPrefix + "_replaced_string");
			ShouldBroadcast = plugin.GetConfigBool(plugin.Details.configPrefix + "_broadcast");
			AFK_Distance = plugin.GetConfigFloat(plugin.Details.configPrefix + "_afk_distance");

			foreach (var ply in ReferenceHub.GetAllHubs().Values)
			{
				AddAFKManager(ply);
			}
		}

		public void AddAFKManager(ReferenceHub Ply)
		{
			if (Ply.TryGetComponent(out AFKManager AFKManager))
				return;

			if (Ply == ReferenceHub.HostHub /*|| PermissionsHandler.IsPermitted(Ply.serverRoles.Permissions, PlayerPermissions.AFKImmunity)*/) return;

			AFKManager AFKManagerComp = Ply.gameObject.AddComponent<AFKManager>();

			AFKManagerComp.AFKDistance = AFK_Distance;
			AFKManagerComp.AFK_Time = AFK_Time;
			AFKManagerComp.ShouldBroadcast = ShouldBroadcast;
			AFKManagerComp.WillBeReplaced = WillBeReplaced;
			AFKManagerComp.HaveBeenReplaced = HaveBeenReplaced;
		}
	}
}