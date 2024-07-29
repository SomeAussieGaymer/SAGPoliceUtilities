using System.Collections.Generic;
using SAGPoliceUtilities.Models;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace SAGPoliceUtilities.Commands.JailCommands
{
    public class Free : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;

            var jailedPlayer = UnturnedPlayer.FromName(command[0]);

            if (command.Length < 1)
            {
                ChatManager.serverSendMessage($"Incorrect usage of command.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (jailedPlayer == null)
            {
                ChatManager.serverSendMessage($"Player does not exist.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            JailTime jailedTime = new JailTime();
            if (!SAGPoliceUtilities.Instance.JailTimeService.IsPlayerJailed(jailedPlayer.CSteamID.ToString(), out jailedTime)) return;
            if (jailedTime == null)
            {
                ChatManager.serverSendMessage($"{jailedPlayer} is not in jail.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            ChatManager.serverSendMessage($"{unturnedPlayer.CharacterName} freed {jailedPlayer.CharacterName} from {jailedTime.JailName}", Color.blue, null, null, EChatMode.GLOBAL, null, true);

            jailedPlayer.Teleport(new Vector3(SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.x, SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.x, SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.z), 0);
            SAGPoliceUtilities.Instance.JailTimeService.RemoveJailedUser(jailedPlayer.CSteamID.ToString());
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "free";
        public string Help => "Free a player from jail.";
        public string Syntax => "/free <player>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "JailRelease" };
    }
}