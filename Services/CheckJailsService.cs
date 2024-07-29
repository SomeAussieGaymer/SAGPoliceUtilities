using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SAGPoliceUtilities.Services
{
    public class CheckJailsService : MonoBehaviour
    {
        public Dictionary<string, DateTime> JailTimes { get; private set; }
        
        private void Start()
        {
            StartCoroutine(CheckJail());
        }
        
        private void OnDestroy()
        {
            
        }

        private static IEnumerator CheckJail()
        {
            while (SAGPoliceUtilities.Instance.IsPluginLoaded)
            {
                //Who the fuck still uses coroutines?
                yield return new WaitForSeconds((float) Convert.ToDouble(SAGPoliceUtilities.Instance.Configuration.Instance.CheckInterval));

                //Logger.Log("Checking jails database...");
                //Logger.Log($"{SAGPoliceUtilities.Instance.JailTimesDatabase.Data.Count} player(s) found in jail.");

                foreach (var jailedPlayer in SAGPoliceUtilities.Instance.JailTimesDatabase.Data.ToList().Where(jailedPlayer => jailedPlayer.ExpireDate <= DateTime.Now))
                {
                    SAGPoliceUtilities.Instance.JailTimeService.RemoveJailedUser(jailedPlayer.PlayerId);
                    ChatManager.serverSendMessage($"{UnturnedPlayer.FromCSteamID((CSteamID) Convert.ToUInt64(jailedPlayer.PlayerId)).CharacterName} was automatically released from {jailedPlayer.JailName}.", Color.blue, null, null, EChatMode.GLOBAL, null, true);
                    UnturnedPlayer.FromCSteamID((CSteamID) Convert.ToUInt64(jailedPlayer.PlayerId)).Teleport(new Vector3(SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.x, SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.x, SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.z), 0);
                }
            }
        }
    }
}