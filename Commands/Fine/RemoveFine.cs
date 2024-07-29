using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SAGPoliceUtilities.Commands.Fine
{
    public class RemoveFine : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;

            if (command.Length < 1)
            {
                ChatManager.serverSendMessage("Incorrect usage of command.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            // Validate and parse the Case ID from the command
            if (!Decimal.TryParse(command[0], out var caseId) || !SAGPoliceUtilities.Instance.FinesDatabase.Collection.Exists(x => x.CaseID == caseId))
            {
                ChatManager.serverSendMessage("Invalid case ID.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            // Retrieve the fine using the Case ID
            Models.Fine selectedFine = SAGPoliceUtilities.Instance.FinesDatabase.Collection.FindOne(x => x.Active && x.CaseID == caseId);

            if (selectedFine == null)
            {
                ChatManager.serverSendMessage($"Fine with the Case ID {caseId} is not active.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            // Deactivate the fine
            SAGPoliceUtilities.Instance.FinesDatabase.DeactivateFine(selectedFine);

            // Notify the fined player
            var finedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)Convert.ToUInt64(selectedFine.PlayerId));
            if (finedPlayer != null)
            {
                ChatManager.serverSendMessage($"Your fine with the Case ID of {selectedFine.CaseID} has been revoked.", Color.blue, null, finedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
            }

            // Notify the command issuer
            ChatManager.serverSendMessage($"Deactivated fine with the Case ID of {selectedFine.CaseID}.", Color.blue, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "removefine";
        public string Help => "Deactivates a fine.";
        public string Syntax => "/removefine <FineID>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "removefine" };
    }
}
