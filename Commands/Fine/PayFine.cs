using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace SAGPoliceUtilities.Commands.Fine
{
    public class PayFine : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;

            if (command.Length < 1)
            {
                ChatManager.serverSendMessage($"Incorrect usage of command.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (!Decimal.TryParse(command[0], out var caseId) || !SAGPoliceUtilities.Instance.FinesDatabase.Collection.Exists(x => x.CaseID == caseId))
            {
                ChatManager.serverSendMessage($"Invalid case ID.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            Models.Fine selectedFine = SAGPoliceUtilities.Instance.FinesDatabase.Collection.FindOne(x => x.Active && x.CaseID == caseId);

            if (selectedFine == null)
            {
                ChatManager.serverSendMessage($"Fine with the Case ID {caseId} is not active.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            var playerExperience = unturnedPlayer.Experience; // Fetch the player's current experience points
            uint finedAmount = (uint)selectedFine.FinedAmount; // Convert finedAmount to uint

            if (playerExperience < finedAmount)
            {
                ChatManager.serverSendMessage($"You do not have enough experience points to pay this fine.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            // Deduct experience points
            unturnedPlayer.Experience -= finedAmount;

            SAGPoliceUtilities.Instance.FinesDatabase.DeactivateFine(selectedFine);
            ChatManager.serverSendMessage($"Paid fine with the Case ID of {selectedFine.CaseID}.", Color.blue, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "payfine";
        public string Help => "Pays off a fine.";
        public string Syntax => "/payfine <FineID>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "payfine" };
    }
}
