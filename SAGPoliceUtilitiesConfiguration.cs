using System.Collections.Generic;
using SAGPoliceUtilities.Models;
using Rocket.API;
using UnityEngine;

namespace SAGPoliceUtilities
{
    public class SAGPoliceUtilitiesConfiguration : IRocketPluginConfiguration
    {
        public float JailRadius { get; set; }
        public Vector3 RelaseLocation { get; set; }
        public double CheckInterval { get; set; }
        public decimal CreditsPerMinute { get; set; }
        public float ExperiencePerMinute { get; set; } // Added property
        public List<Jail> Jails { get; set; }
        public string OnDutyPermissionGroup { get; set; }
        public int PayInterval { get; set; }
        public int PayAmount { get; set; }

        public void LoadDefaults()
        {
            JailRadius = 5;
            RelaseLocation = new Vector3(0, 0, 0);
            CheckInterval = 15;
            CreditsPerMinute = 5;
            ExperiencePerMinute = 1; // Default value
            Jails = new List<Jail>()
            {
                new Jail()
                {
                    Name = "Default",
                    X = 0,
                    Y = 0,
                    Z = 0
                }
            };
            OnDutyPermissionGroup = "OnDutyPolice";
            PayAmount = 1; // Default XP per interval
            PayInterval = 5; // Time in seconds between payments
        }
    }
}
