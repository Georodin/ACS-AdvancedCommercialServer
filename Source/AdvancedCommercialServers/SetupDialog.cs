using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class SetupDialog : Window
    {
        private Dictionary<ThingDef, bool> items;
        private ServerRack rack;

        public SetupDialog(Dictionary<ThingDef, bool> items, ServerRack rack)
        {
            this.items = items;
            this.rack = rack;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float valueComponent = DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue;

            Text.Font = GameFont.Medium; // Change to your preferred font size
            listingStandard.Label("Payout Resources"); // This will add a label at the current position in the listing
            Text.Font = GameFont.Small; // Reset to the default font size

            foreach (var item in items.Keys.ToList()) // ToList to avoid collection modification issues
            {
                bool state = items[item];

                Texture2D itemIcon = item.uiIcon; // Assuming uiIcon is accessible and contains the icon texture for the ThingDef
                GUIContent itemContent = new GUIContent(itemIcon);

                Texture2D stateIcon = state ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;

                Rect rowRect = listingStandard.GetRect(30); // Adjust the height to match your icons

                string nameAndCount = item.label + " x" + Math.Floor(valueComponent / item.BaseMarketValue);

                Widgets.Label(new Rect(rowRect.x, rowRect.y, 30, 30), itemContent);
                Widgets.Label(new Rect(rowRect.x + 35, rowRect.y, 200, 30), nameAndCount); // Use the label of the ThingDef as the displayed name

                // Draw the toggle button with checkmark or X icon
                if (Widgets.ButtonImage(new Rect(rowRect.x + 240, rowRect.y, 30, 30), stateIcon))
                {
                    items[item] = !state; // Toggle state when button is pressed
                }
            }

            listingStandard.End(); // Don't forget to end the listing

            Rect okButtonRect = listingStandard.GetRect(30); // Replace 30 with the desired height of your button.
            if (Widgets.ButtonText(okButtonRect, "OK"))
            {
                rack.UpdateList();
                Close();
            }
        }
    }
}
