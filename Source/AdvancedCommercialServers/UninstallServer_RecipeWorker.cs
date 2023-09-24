using System.Collections.Generic;
using Verse;

namespace AdvancedCommercialServers
{
    public class UninstallServer_RecipeWorker : RecipeWorker
    {
        
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {

            if (thing is ServerRack rack)
            {
                Log.Message("Unistall: "+ rack.IsUninstallAvailableNow(recipe.label));
                return rack.IsUninstallAvailableNow(recipe.label);
            }
            return false;
        }

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            //base.ConsumeIngredient(ingredient, recipe, map);
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {

            Thing targetThing = billDoer.CurJob.GetTarget(Verse.AI.TargetIndex.A).Thing;

            if (targetThing is ServerRack serverRack)
            {
                
                serverRack.UninstallServer(billDoer, recipe.label);
            }

        }

    }
}
