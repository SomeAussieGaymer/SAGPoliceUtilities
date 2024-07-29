using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using SAGPoliceUtilities.Models;

namespace SAGPoliceUtilities.Commands.JailCommands
{
    public class Bail : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;

            // Determine the jailed player
            UnturnedPlayer jailedPlayer = command.Length > 0 ? UnturnedPlayer.FromName(command[0]) : unturnedPlayer;

            if (jailedPlayer == null)
            {
                ChatManager.serverSendMessage($"Player does not exist.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            JailTime jailedTime;
            if (!SAGPoliceUtilities.Instance.JailTimeService.IsPlayerJailed(jailedPlayer.CSteamID.ToString(), out jailedTime))
            {
                ChatManager.serverSendMessage($"{jailedPlayer.CharacterName} is not in jail.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            var currentExperience = unturnedPlayer.Experience; // Fetch the player's current experience points
            var minutesInJail = (int)(jailedTime.ExpireDate - DateTime.UtcNow).TotalMinutes;
            var requiredExperience = SAGPoliceUtilities.Instance.Configuration.Instance.ExperiencePerMinute * minutesInJail;

            if (currentExperience < requiredExperience)
            {
                ChatManager.serverSendMessage($"You need {requiredExperience} experience points, but you only have {currentExperience} experience points!", Color.red, null, null, EChatMode.GLOBAL, null, true);
                return;
            }

            // Deduct experience points
            unturnedPlayer.Experience -= (uint)requiredExperience;

            ChatManager.serverSendMessage($"You bailed {jailedPlayer.CharacterName} for {requiredExperience} experience points.", Color.red, null, null, EChatMode.GLOBAL, null, true);
            ChatManager.serverSendMessage($"{unturnedPlayer.CharacterName} bailed {jailedPlayer.CharacterName} from {jailedTime.JailName}", Color.blue, null, null, EChatMode.GLOBAL, null, true);

            // Teleport the jailed player to the release location
            jailedPlayer.Teleport(new Vector3(
                SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.x,
                SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.y,
                SAGPoliceUtilities.Instance.Configuration.Instance.RelaseLocation.z), 0);

            // Remove the jailed time entry
            SAGPoliceUtilities.Instance.JailTimesDatabase.Data.Remove(jailedTime);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "bail";
        public string Help => "Bails a player from jail.";
        public string Syntax => "/bail [player]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "bail" };
    }
}
