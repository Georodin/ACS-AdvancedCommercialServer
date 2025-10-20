using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class SetupDialog : Window
    {
        private ServerRack rack;
        private Vector2 leftScrollPosition;

        public SetupDialog(ServerRack rack)
        {
            this.rack = rack;
            closeOnCancel = true;  // Ensures that the standard window close operation is linked to key events.
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (rack == null || rack.List == null || rack.Production == null)
            {
                string reason = "Unknown";

                if (rack == null)
                    reason = "rack == null";
                else if (rack.List == null)
                    reason = "rack.List == null";
                else if (rack.Production == null)
                    reason = "rack.Production == null";

                Widgets.Label(inRect, $"[SetupDialog] Data not loaded: {reason}");
                return;
            }


            float topPadding = 15f;
            float bottomPadding = 50f;
            float iconSize = 24f;  // Define icon size, ensuring it's square
            float scrollViewHeight = inRect.height - topPadding - bottomPadding - 30f; // Space for OK button and gap

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Text.Font = GameFont.Medium;
            listingStandard.Label("Payout Resources");
            listingStandard.Gap(10f);  // Adds a small gap after the title

            Rect scrollViewRect = new Rect(0, listingStandard.CurHeight, inRect.width, scrollViewHeight);
            Rect scrollContentRect = new Rect(0, 0, scrollViewRect.width - 16, rack.List.Count * (32 + 4) + 10);
            Widgets.BeginScrollView(scrollViewRect, ref leftScrollPosition, scrollContentRect, true);

            var itemListing = new Listing_Standard();
            itemListing.Begin(scrollContentRect);
            foreach (var item in rack.List.Keys.ToList().OrderBy(dev => dev.label))
            {
                bool state = rack.List[item];
                //Log.Message($"itemDiag: {item} is {state}");
                Texture2D itemIcon = item.uiIcon;
                GUIContent itemContent = new GUIContent(" " + item.label + " x" + Mathf.Max(1f, Mathf.Floor(DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue / item.BaseMarketValue)), itemIcon);

                Rect rowRect = itemListing.GetRect(30);

                // Adjusting the icon to be square
                Rect iconRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);
                if (Widgets.ButtonImage(iconRect, itemIcon))
                {
                    rack.List[item] = !state; // Toggle state when icon is clicked
                }

                // Adjust label position
                Widgets.Label(new Rect(iconRect.xMax + 5, rowRect.y, rowRect.width - iconSize - 35, rowRect.height), itemContent.text);

                // Checkbox positioned to the right of the label
                Rect checkboxRect = new Rect(rowRect.x + rowRect.width - 30, rowRect.y, 24, 24);
                Widgets.Checkbox(checkboxRect.x, checkboxRect.y, ref state);

                rack.List[item] = state;  // Update the item state after the checkbox is used

                itemListing.Gap(4f); // Adds a small gap between items
            }
            itemListing.End();
            Widgets.EndScrollView();

            Rect okButtonRect = new Rect(0, inRect.height - 45f, inRect.width, 30f);
            if (Widgets.ButtonText(okButtonRect, "OK"))
            {
                CloseWindow();
            }

            listingStandard.End();
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            CloseWindow();
        }

        private void CloseWindow()
        {
            rack.Production.UpdateActivatedItems(rack.List);
            Close();
        }
    }
}