using System;
using System.Collections;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core.Commands;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Core;
using UnityEngine;
using SAGPoliceUtilities;

namespace SAGPoliceUtilities.Commands.Duty
{
    public class Duty : RocketPlugin<SAGPoliceUtilitiesConfiguration>
    {
        public static Duty Instance;
        private readonly Dictionary<ulong, DateTime> dutyStartTimes = new Dictionary<ulong, DateTime>();
        private readonly Dictionary<ulong, int> accumulatedXP = new Dictionary<ulong, int>();
        private readonly Dictionary<ulong, int> backPayXP = new Dictionary<ulong, int>();
        private int officersOnDuty = 0;
        private const string RequiredPermission = "PoliceDuty";

        protected override void Load()
        {
            Instance = this;

            U.Events.OnPlayerDisconnected += PlayerDisconnected;
            U.Events.OnPlayerConnected += PlayerConnected;

            Rocket.Core.Logging.Logger.LogWarning("PoliceStuffPlugin has been successfully loaded!");
        }

        protected override void Unload()
        {
            Instance = null;

            U.Events.OnPlayerDisconnected -= PlayerDisconnected;
            U.Events.OnPlayerConnected -= PlayerConnected;

            Rocket.Core.Logging.Logger.LogWarning("PoliceStuffPlugin has been unloaded!");
        }

        [RocketCommand("pduty", "Toggle police duty status", "", AllowedCaller.Player)]
        public void PDutyCommand(IRocketPlayer caller, string[] _)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (!player.HasPermission(RequiredPermission))
            {
                UnturnedChat.Say(player, "You do not have permission to use this command.", Color.red);
                return;
            }

            StartCoroutine(ToggleDuty(player));
        }

        private IEnumerator ToggleDuty(UnturnedPlayer player)
        {
            ulong playerID = player.CSteamID.m_SteamID;
            bool onDuty = dutyStartTimes.ContainsKey(playerID);

            if (onDuty)
            {
                // Off Duty
                DateTime dutyStartTime = dutyStartTimes[playerID];
                TimeSpan timeOnDuty = DateTime.UtcNow - dutyStartTime;

                // Calculate total XP to be paid (accumulated XP plus remaining time since last interval)
                int totalSecondsOnDuty = (int)timeOnDuty.TotalSeconds;
                int remainingSeconds = totalSecondsOnDuty % (int)Configuration.Instance.PayInterval;
                accumulatedXP[playerID] += remainingSeconds * Configuration.Instance.PayAmount;

                int totalXP = accumulatedXP[playerID];

                // Remove player from duty group
                if (R.Permissions != null)
                {
                    var result = R.Permissions.RemovePlayerFromGroup(Configuration.Instance.OnDutyPermissionGroup, player);
                    if (result != RocketPermissionsProviderResult.Success)
                    {
                        Rocket.Core.Logging.Logger.LogError($"Failed to remove {player.CharacterName} from group {Configuration.Instance.OnDutyPermissionGroup}: {result}");
                    }
                }
                else
                {
                    Rocket.Core.Logging.Logger.LogError("Permissions API is not available.");
                }

                // Pay the player
                player.Experience += (uint)totalXP;
                officersOnDuty--;

                // Announcement
                UnturnedChat.Say($"An officer has gone off duty. (Officers on duty: {officersOnDuty})", Color.white);
                UnturnedChat.Say(player, $"You have been paid {totalXP} experience for your total duty time. (Total time on duty: {timeOnDuty.TotalMinutes:F2} minutes)", Color.blue);

                dutyStartTimes.Remove(playerID);
                accumulatedXP.Remove(playerID);
            }
            else
            {
                // On Duty
                dutyStartTimes[playerID] = DateTime.UtcNow;
                accumulatedXP[playerID] = 0;

                if (R.Permissions != null)
                {
                    var result = R.Permissions.AddPlayerToGroup(Configuration.Instance.OnDutyPermissionGroup, player);
                    if (result != RocketPermissionsProviderResult.Success)
                    {
                        Rocket.Core.Logging.Logger.LogError($"Failed to add {player.CharacterName} to group {Configuration.Instance.OnDutyPermissionGroup}: {result}");
                    }
                }
                else
                {
                    Rocket.Core.Logging.Logger.LogError("Permissions API is not available.");
                }

                officersOnDuty++;
                StartCoroutine(AccumulateXP(player));

                // Announcement
                UnturnedChat.Say($"An officer has gone on duty. (Officers on duty: {officersOnDuty})", Color.white);
            }

            yield return null;
        }

        private IEnumerator AccumulateXP(UnturnedPlayer player)
        {
            ulong playerID = player.CSteamID.m_SteamID;
            while (dutyStartTimes.ContainsKey(playerID))
            {
                yield return new WaitForSeconds(Configuration.Instance.PayInterval);

                if (dutyStartTimes.ContainsKey(playerID))
                {
                    accumulatedXP[playerID] += Configuration.Instance.PayAmount;
                }
            }
        }

        private void PlayerDisconnected(UnturnedPlayer player)
        {
            ulong playerID = player.CSteamID.m_SteamID;

            // Check if the player was on duty
            if (dutyStartTimes.ContainsKey(playerID))
            {
                // Calculate accumulated XP up to the moment of disconnection
                DateTime dutyStartTime = dutyStartTimes[playerID];
                TimeSpan timeOnDuty = DateTime.UtcNow - dutyStartTime;
                int totalSecondsOnDuty = (int)timeOnDuty.TotalSeconds;
                int remainingSeconds = totalSecondsOnDuty % (int)Configuration.Instance.PayInterval;
                accumulatedXP[playerID] += remainingSeconds * Configuration.Instance.PayAmount;

                if (backPayXP.ContainsKey(playerID))
                {
                    backPayXP[playerID] += accumulatedXP[playerID];
                }
                else
                {
                    backPayXP[playerID] = accumulatedXP[playerID];
                }

                // Remove from dictionaries
                dutyStartTimes.Remove(playerID);
                accumulatedXP.Remove(playerID);

                // Handle payout when the player disconnects
                StopCoroutine(AccumulateXP(player));
            }
        }

        private void PlayerConnected(UnturnedPlayer player)
        {
            ulong playerID = player.CSteamID.m_SteamID;

            // Pay back pay if any
            if (backPayXP.ContainsKey(playerID) && backPayXP[playerID] > 0)
            {
                player.Experience += (uint)backPayXP[playerID];
                UnturnedChat.Say(player, $"You've been paid {backPayXP[playerID]} XP in back pay.", Color.blue);
                backPayXP.Remove(playerID);
            }
        }
    }
}
