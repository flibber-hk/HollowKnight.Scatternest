using ItemChanger;
using UnityEngine.SceneManagement;
using UnityEngine;
using ItemChanger.Extensions;
using UObject = UnityEngine.Object;

namespace Scatternest
{
    internal class StartChangesProxyModule : ItemChanger.Modules.Module
    {
        public override void Initialize()
        {
            ToggleSceneHooks(true);
        }

        public override void Unload()
        {
            ToggleSceneHooks(false);
        }

        // Changes taken from RandomizerMod/IC/RandomizerModule.cs
        // https://github.com/homothetyhk/RandomizerMod/blob/e061177fd31332572b5162fc9a767271e71f9291/RandomizerMod/IC/RandomizerModule.cs#L89
        private static void ToggleSceneHooks(bool toggle)
        {
            RandomizerMod.Settings.GenerationSettings gs = RandomizerMod.RandomizerMod.RS.GenerationSettings;
            string startLocation = gs.StartLocationSettings.StartLocation;

            if (startLocation.Contains("|Ancestral Mound|"))
            {
                if (gs.NoveltySettings.RandomizeNail)
                {
                    if (toggle) Events.AddSceneChangeEdit(SceneNames.Crossroads_ShamanTemple, DestroyPlanksForAncestralMoundStart);
                    else Events.RemoveSceneChangeEdit(SceneNames.Crossroads_ShamanTemple, DestroyPlanksForAncestralMoundStart);
                }
            }

            if (startLocation.Contains("|Fungal Core|"))
            {
                if (toggle) Events.AddSceneChangeEdit(SceneNames.Fungus2_30, CreateBounceShroomsForFungalCoreStart);
                else Events.RemoveSceneChangeEdit(SceneNames.Fungus2_30, CreateBounceShroomsForFungalCoreStart);
            }

            if (startLocation.Contains("|West Crossroads|"))
            {
                if (toggle) Events.AddSceneChangeEdit(SceneNames.Crossroads_36, MoveShadeMarkerForWestCrossroadsStart);
                else Events.RemoveSceneChangeEdit(SceneNames.Crossroads_36, MoveShadeMarkerForWestCrossroadsStart);
            }
        }

        // Destroy planks in cursed nail mode because we can't slash them
        private static void DestroyPlanksForAncestralMoundStart(Scene to)
        {
            foreach ((_, GameObject go) in to.Traverse())
            {
                if (go.name.StartsWith("Plank")) UObject.Destroy(go);
            }
        }

        private static void CreateBounceShroomsForFungalCoreStart(Scene to)
        {
            GameObject bounceShroom = to.FindGameObjectByName("Bounce Shroom C");

            GameObject s0 = UObject.Instantiate(bounceShroom);
            s0.transform.SetPosition3D(12.5f, 26f, 0f);
            s0.SetActive(true);

            GameObject s1 = UObject.Instantiate(bounceShroom);
            s1.transform.SetPosition3D(12.5f, 54f, 0f);
            s1.SetActive(true);

            GameObject s2 = UObject.Instantiate(bounceShroom);
            s2.transform.SetPosition3D(21.7f, 133f, 0f);
            s2.SetActive(true);
        }

        private static void MoveShadeMarkerForWestCrossroadsStart(Scene to)
        {
            GameObject marker = to.FindGameObject("_Props/Hollow_Shade Marker 1");
            marker.transform.position = new(46.2f, 28f);
        }
    }
}
