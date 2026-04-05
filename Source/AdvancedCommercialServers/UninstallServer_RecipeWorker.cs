using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AdvancedCommercialServers
{
    public class UninstallServer_RecipeWorker : RecipeWorker
    {
        private ThingDef ResolveServerDef()
        {
            if (recipe?.ProducedThingDef != null)
                return recipe.ProducedThingDef;

            if (recipe?.products != null && recipe.products.Count > 0)
                return recipe.products[0].thingDef;

            return null;
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            var rack = thing as ServerRack;
            if (rack == null)
                return false;

            ThingDef serverDef = ResolveServerDef();
            if (serverDef == null)
            {
                Log.Warning($"[ACS] Uninstall recipe '{recipe?.defName ?? "<null>"}' could not resolve a server def.");
                return false;
            }

            return rack.Core != null && rack.Core.IsUninstallAvailable(serverDef);
        }

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            // Intentionally do nothing: uninstall recipes should not consume extra ingredients.
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Thing targetThing = billDoer?.CurJob?.GetTarget(Verse.AI.TargetIndex.A).Thing;
            if (targetThing is ServerRack serverRack)
            {
                ThingDef serverDef = ResolveServerDef();
                if (serverDef == null)
                {
                    Log.Warning($"[ACS] Uninstall recipe '{recipe?.defName ?? "<null>"}' could not resolve a server def during completion.");
                }
                else
                {
                    serverRack.Core?.UninstallServer(billDoer, serverDef);

                    // After uninstalling, remove this bill if no more of that server type remain.
                    if (serverRack.Core == null || !serverRack.Core.IsUninstallAvailable(serverDef))
                    {
                        Bill billToRemove = null;

                        if (serverRack.BillStack != null)
                        {
                            foreach (Bill bill in serverRack.BillStack.Bills)
                            {
                                if (bill?.recipe == recipe)
                                {
                                    billToRemove = bill;
                                    break;
                                }
                            }
                        }

                        if (billToRemove != null)
                        {
                            serverRack.BillStack.Delete(billToRemove);
                        }
                    }
                }
            }

            base.Notify_IterationCompleted(billDoer, ingredients);
        }
    }
}