using System;
using System.Collections.Generic;
using System.Linq;
using SAGPoliceUtilities.Database;
using SAGPoliceUtilities.Models;
using UnityEngine;

namespace SAGPoliceUtilities.Services
{
    public class FinesService : MonoBehaviour
    {
        public Dictionary<string, DateTime> Fines { get; private set; }

        private FinesDatabase Database => SAGPoliceUtilities.Instance.FinesDatabase;

        void Awake()
        {
            Fines = new Dictionary<string, DateTime>();
        }

        void Start()
        {
        }

        private void OnDestroy()
        {
        }

        public void RegisterFine(string playerId, int amount, string reason)
        {
            Database.FinePlayer(playerId, amount, reason);
        }

        public void RemoveFine(string playerId)
        {
            var fines = Database.FindActiveFines(playerId);
            if (fines.Count > 0)
            {
                foreach (var fine in fines)
                {
                    Database.DeactivateFine(fine);
                }
            }
        }

        public bool ActiveFine(string playerId, out Fine fine)
        {
            var fines = Database.FindActiveFines(playerId);
            fine = fines.FirstOrDefault();
            return fine != null;
        }

        public int AmountFines(string playerId, out int amount)
        {
            var fines = Database.FindActiveFines(playerId);
            amount = fines.Count;
            return amount;
        }
    }
}
