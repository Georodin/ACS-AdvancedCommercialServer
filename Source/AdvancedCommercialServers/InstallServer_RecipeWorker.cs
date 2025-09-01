using System.Collections.Generic;
using Verse;

namespace AdvancedCommercialServers
{
    public class InstallServer_RecipeWorker : RecipeWorker
    {
        
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {

            if (thing is ServerRack rack)
            {
                return rack.Core.IsInstallAvailableNow();
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

            if (targetThing is ServerRack rack)
            {
                rack.Core.InstallServer(ingredients[0]);
            }

            base.Notify_IterationCompleted(billDoer, ingredients);
        }
    }
}
