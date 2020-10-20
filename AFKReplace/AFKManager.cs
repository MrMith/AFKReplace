using Smod2.Commands;
using Smod2;
using System;
using UnityEngine;
using ServerMod2.API;
using System.Collections.Generic;
using Smod2.API;
using System.Linq;
using System.Linq.Expressions;

namespace AFKReplace
{
	class AFKManager : MonoBehaviour
	{
		private PlayerMovementSync _pms;
		private CharacterClassManager _ccm;
		private Vector3 OldPos;

		public bool ShouldBroadcast = true;
		public float AFK_Time = 10000;
		public float TimeAFK = 0; //Time spent AFK
		public float AFKDistance;
		public int LastSecond;
		public string WillBeReplaced;
		public string HaveBeenReplaced;

		public void Awake()
		{
			_pms = GetComponent<PlayerMovementSync>();
			_ccm = GetComponent<CharacterClassManager>();
			OldPos = _pms.GetRealPosition();
			_pms.AFKTime = 0; //Disable the main game AFK system despite the fact that I wrote it :pepesad:
		}

		public void FixedUpdate()
		{
			if (_ccm.CurClass == RoleType.Spectator || _ccm.CurClass == RoleType.None) return;

			if (Vector3.Distance(_pms.GetRealPosition(), OldPos) < AFKDistance)
			{
				TimeAFK += Time.fixedDeltaTime;
			}
			else
			{
				TimeAFK = 0f;
			}
			
			OldPos = _pms.GetRealPosition();
			
			List<Player> Spectators = new List<Player>();
			foreach (var refHub in ReferenceHub.GetAllHubs().Values) //I've done worse for less
			{
				Player SModPlayer = new SmodPlayer(refHub.gameObject);
				if (refHub.characterClassManager.CurClass == RoleType.Spectator && !SModPlayer.OverwatchMode)
				{
					Spectators.Add(SModPlayer);
				}
			}

			if (Spectators.Count == 0) return;

			if ((int)TimeAFK != LastSecond && (AFK_Time - TimeAFK) <= 15)
			{
				LastSecond = Convert.ToInt32(TimeAFK);
				if (ShouldBroadcast)
				{
					var player = new SmodPlayer(ReferenceHub.GetHub(_pms.gameObject).gameObject);
					player.PersonalClearBroadcasts();
					player.PersonalBroadcast(2, WillBeReplaced.Replace("{0}", $"{(int)AFK_Time - (int)TimeAFK}"), true);
				}
			}

			if (TimeAFK >= AFK_Time)
			{
				ServerMod.DebugLog("AFKReplace","Swapping players");
				#region Swap AFK with spectator
				Player CurrentPlayer = new SmodPlayer(ReferenceHub.GetHub(_pms.gameObject).gameObject);
				Player ChosenSpectator = Spectators[UnityEngine.Random.Range(0, Spectators.Count - 1)];
				ServerMod.DebugLog("AFKReplace", $"Swapping {CurrentPlayer.Name}({CurrentPlayer.PlayerId}) for {ChosenSpectator.Name}({ChosenSpectator.PlayerId})");
				ChosenSpectator.ChangeRole(CurrentPlayer.TeamRole.Role, true, false, false, true);
				ChosenSpectator.HP = CurrentPlayer.HP;
				ChosenSpectator.AHP = CurrentPlayer.AHP;
				ChosenSpectator.Stamina = CurrentPlayer.Stamina;
				ChosenSpectator.Teleport(CurrentPlayer.GetPosition());
				ChosenSpectator.ClearInventory();

				foreach(var item in CurrentPlayer.GetInventory())
				{
					var GivenItem = ChosenSpectator.GiveItem(item.ItemType);

					if(item.IsWeapon) //I want to transfer stuff like durability but keep the chosen player's mods
					{
						GivenItem.ToWeapon().AmmoInClip = item.ToWeapon().AmmoInClip;
					}
				}

				CurrentPlayer.ClearInventory();
				ChosenSpectator.SetAmmo(AmmoType.AMMO556, CurrentPlayer.GetAmmo(AmmoType.AMMO556));
				ChosenSpectator.SetAmmo(AmmoType.AMMO762, CurrentPlayer.GetAmmo(AmmoType.AMMO762));
				ChosenSpectator.SetAmmo(AmmoType.AMMO9MM, CurrentPlayer.GetAmmo(AmmoType.AMMO9MM));

				CurrentPlayer.SetAmmo(AmmoType.AMMO556, 0);
				CurrentPlayer.SetAmmo(AmmoType.AMMO762, 0);
				CurrentPlayer.SetAmmo(AmmoType.AMMO9MM, 0); //So they don't drop ammo on the ground.

				CurrentPlayer.ChangeRole(Smod2.API.RoleType.SPECTATOR);

				if (ShouldBroadcast)
				{
					CurrentPlayer.PersonalClearBroadcasts();
					CurrentPlayer.PersonalBroadcast(10, HaveBeenReplaced, true);
				}
				#endregion
			}
		}
	}
}
