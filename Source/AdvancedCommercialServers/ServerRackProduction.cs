using Verse;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdvancedCommercialServers
{
    public class ServerRackProduction : IExposable
    {
        private readonly ServerRack parent;

        private float currentProgress;
        private float normalizedProgress;
        private int currentResourceIndex;

        public List<ThingDef> activatedItems;

        public void ExposeData()
        {
            Scribe_Values.Look(ref currentProgress, "currentProgress", 0f);
            Scribe_Values.Look(ref normalizedProgress, "normalizedProgress", 0f);
            Scribe_Values.Look(ref currentResourceIndex, "currentResourceIndex", 0);
            Scribe_Collections.Look(ref activatedItems, "activatedItems", LookMode.Def);
        }

        public ServerRackProduction(ServerRack parent)
        {
            this.parent = parent;

            if (activatedItems == null)
            {
                activatedItems = new List<ThingDef>();
            }
        }

        public void AdvanceProgress()
        {
            if (activatedItems.Count == 0 || !parent.StateManager.IsOperational)
                return;

            ThingDef currentItem = activatedItems[currentResourceIndex];

            float valueComponent = DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue;
            float itemsToSpawn = Mathf.Max(1, Mathf.Floor(valueComponent / currentItem.BaseMarketValue));
            float totalProgressNeeded = currentItem.BaseMarketValue * itemsToSpawn;

            currentProgress += parent.Core.CurrentGenerationSpeed * 0.008f * ServerModSettings.generationSpeedMultiplier;

            normalizedProgress = (currentProgress / totalProgressNeeded) * 100f;

            if (normalizedProgress >= 100f)
            {
                currentProgress = 0f;
                SpawnItem(currentItem, (int)itemsToSpawn);
                CycleToNextItem();
            }
        }

        public void UpdateActivatedItems(Dictionary<ThingDef, bool> itemSelection)
        {
            activatedItems.Clear();

            foreach (var kvp in itemSelection)
            {
                if (kvp.Value)
                    activatedItems.Add(kvp.Key);
            }

            if (activatedItems.Count > 0)
                currentResourceIndex %= activatedItems.Count;
            else
                currentResourceIndex = 0;
        }

        public string GetProgressString()
        {
            if (activatedItems.Count == 0)
                return "ACS_NoPayoutSelected".Translate();

            ThingDef current = activatedItems[currentResourceIndex];
            float valueComponent = DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue;
            float count = Mathf.Max(1, Mathf.Floor(valueComponent / current.BaseMarketValue));

            return "ACS_PayoutProgress".Translate(current.label, count, normalizedProgress.ToString("F2"));
        }


        private void CycleToNextItem()
        {
            currentResourceIndex = (currentResourceIndex + 1) % activatedItems.Count;
        }

        private void SpawnItem(ThingDef itemDef, int count)
        {
            Thing item = ThingMaker.MakeThing(itemDef);
            item.stackCount = count;
            GenPlace.TryPlaceThing(item, parent.Position, parent.Map, ThingPlaceMode.Near);
        }
    }
}
