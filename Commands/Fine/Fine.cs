using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace SAGPoliceUtilities.Commands.Fine
{
    public class Fine : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;

            if (command.Length <= 2)
            {
                ChatManager.serverSendMessage($"Incorrect usage of command.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            var finedPlayer = UnturnedPlayer.FromName(command[0]);
            var fineReason = string.Join(" ", command.Skip(2));

            if (finedPlayer == null)
            {
                ChatManager.serverSendMessage($"Player does not exist.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (finedPlayer.IsAdmin || finedPlayer.HasPermission("PoliceOnDutyfineimmune"))
            {
                ChatManager.serverSendMessage($"You cannot fine that player!", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (!Decimal.TryParse(command[1], out var fineAmount))
            {
                ChatManager.serverSendMessage($"Invalid amount to fine.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (fineAmount < 0)
            {
                ChatManager.serverSendMessage($"Fine amount cannot be negative.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
                return;
            }

            if (fineReason.Length <= 1)
            {
                fineReason = "N/A";
            }

            SAGPoliceUtilities.Instance.FinesDatabase.FinePlayer(finedPlayer.CSteamID.ToString(), fineAmount, fineReason);
            ChatManager.serverSendMessage($"[Case {SAGPoliceUtilities.Instance.FinesDatabase.Collection.Count()}] {unturnedPlayer.CharacterName} fined {finedPlayer.CharacterName} for {fineAmount} Experience Points.", Color.red, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, null, true);
            ChatManager.serverSendMessage($"{unturnedPlayer.CharacterName} fined {finedPlayer.CharacterName} for {fineReason} for {fineAmount} Experience Points!", Color.yellow, null, null, EChatMode.GLOBAL, null, true);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "fine";
        public string Help => "Fines a player.";
        public string Syntax => "/fine <Player> <Amount> [Reason]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "PoliceOnDutyfine" };
    }
}
