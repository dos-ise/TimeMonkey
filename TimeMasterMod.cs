using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.SimulationBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.AbilitiesMenu;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Unity.UI_New.Upgrade;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using TimeMaster;

[assembly: MelonInfo(typeof(TimeMaster.TimeMasterMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace TimeMaster
{
    public class TimeMasterMod : BloonsTD6Mod
    {
        // How many rounds are still queued to skip (used by the +10 / +100 abilities)
        private static int roundsToSkip;

        // ─────────────────────────────────────────────────────────────────────────
        //  Game-model patch – create the TimeMaster tower
        // ─────────────────────────────────────────────────────────────────────────
        public override void OnGameModelLoaded(GameModel model)
        {
            // Clone MonkeyVillage as base (no attacks, reasonable cost)
            TowerModel tower = model.GetTower("MonkeyVillage", 0, 0, 0).CloneCast<TowerModel>();
            tower.towerSet          = (TowerSet)8;
            tower.range             = 10f;
            tower.radius            = 10f;
            tower.cost              = 0f;
            tower.name              = "TimeMaster";
            tower.baseId            = "TimeMaster";
            tower.dontDisplayUpgrades = true;

            // Use embedded icon / display assets
            tower.SetIcons("TimeMaster.timemaster_icon.png");
            // If you have a custom 3-D asset bundle set the GUID here; otherwise it
            // will fall back to the village model which looks fine in-game:
            // tower.SetDisplay("TimeMonkey");

            tower.upgrades = Il2CppReferenceArray<UpgradePathModel>.op_Implicit(Array.Empty<UpgradePathModel>());
            tower.mods     = Il2CppReferenceArray<ApplyModModel>.op_Implicit(Array.Empty<ApplyModModel>());

            // Remove the village range-support behaviour – this tower does nothing offensively
            tower.behaviors = tower.behaviors.Remove<Il2CppAssets.Scripts.Models.Model>(
                m => m.IsType<RangeSupportModel>());

            // ── Build the five ability models ─────────────────────────────────────
            AbilityModel MakeAbility(string abilityName, string iconGuid, float cooldown)
            {
                AbilityModel ability = model
                    .GetTower("DartlingGunner", 0, 4, 0).behaviors
                    .OfType<AbilityModel>()
                    .First()
                    .CloneCast<AbilityModel>();

                ability.name        = abilityName;
                ability.displayName = abilityName;
                ability.cooldown    = cooldown;
                ability.icon        = new SpriteReference { guidRef = $"Ui[TimeMaster.{iconGuid}]" };

                // Remove the built-in attack activation – we drive everything from patches
                ability.behaviors = ability.behaviors.Remove<Il2CppAssets.Scripts.Models.Model>(
                    m => m.IsType<ActivateAttackModel>());

                // Silence the default sound
                foreach (var b in ability.behaviors.OfType<CreateSoundOnAbilityModel>())
                    b.sound = null;

                return ability;
            }

            var ffOne     = MakeAbility("FFOne",     "pocket_watch_icon.png",   0.05f);   // +1 round
            var ffTen     = MakeAbility("FFTen",     "pocket_watch_icon_2.png", 0.5f);    // +10 rounds
            var ffHundred = MakeAbility("FFHundred", "pocket_watch_icon_3.png", 1.5f);    // +100 rounds
            var ffReverse = MakeAbility("FFReverse", "pocket_watch_icon_4.png", 0.05f);   // -1 round
            var ffSelect  = MakeAbility("FFSelect",  "pocket_watch_icon_5.png", 1.5f);    // jump to round

            tower.behaviors = tower.behaviors.Add<Il2CppAssets.Scripts.Models.Model>(
                ffOne, ffTen, ffHundred, ffReverse, ffSelect);

            // Register in game model
            ShopTowerDetailsModel shopEntry =
                ((Il2CppAssets.Scripts.Models.Model)(object)model.towerSet[0])
                    .CloneCast<ShopTowerDetailsModel>();
            shopEntry.towerId    = "TimeMaster";
            shopEntry.towerIndex = 294503994;

            model.towers  = model.towers.Add<TowerModel>(tower);
            model.towerSet = model.towerSet.Add<TowerDetailsModel>(shopEntry);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Profile patch – auto-unlock the tower
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
        private static class ProfileModel_Validate
        {
            [HarmonyPostfix]
            private static void Postfix(ProfileModel __instance) =>
                __instance.unlockedTowers.AddIfNotPresent("TimeMaster");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Ability activation
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(Ability), nameof(Ability.Activate))]
        private static class Ability_Activate
        {
            [HarmonyPostfix]
            private static void Postfix(Ability __instance)
            {
                if (__instance?.tower?.towerModel == null || __instance.abilityModel == null)
                    return;
                if (!__instance.tower.towerModel.baseId.Contains("TimeMaster"))
                    return;

                string abilityName = __instance.abilityModel.name;
                var bridge = InGame.instance.bridge;

                if (abilityName.Contains("FFOne"))
                {
                    SkipRoundsAndCollect(1);
                    AdvanceRound(bridge.GetCurrentRound(), 1);
                }
                else if (abilityName.Contains("FFTen"))
                {
                    SkipRoundsAndCollect(10);       // cash/xp collected incrementally via roundsToSkip
                    roundsToSkip += 10;
                }
                else if (abilityName.Contains("FFHundred"))
                {
                    SkipRoundsAndCollect(100);
                    roundsToSkip += 100;
                }
                else if (abilityName.Contains("FFReverse"))
                {
                    int current = bridge.GetCurrentRound();
                    if (current > 0)
                    {
                        bridge.simulation.EndRound(current, Math.Max(current, 999));
                        bridge.StartRound();
                        bridge.SetRound(current - 1, false);
                    }
                }
                else if (abilityName.Contains("FFSelect") &&
                         __instance.tower.owner == InGame.Bridge.MyPlayerNumber)
                {
                    int current = bridge.GetCurrentRound();
                    PopupScreen.instance.ShowSetValuePopup(
                        "Jump to round",
                        "Enter the round number and the Time Master will leap there.",
                        new Action<int>(targetRound =>
                        {
                            if (targetRound <= 0) return;
                            int dest = targetRound - 1;
                            if (dest > current)
                            {
                                CollectCashAndXp(dest, current);
                            }
                            bridge.simulation.EndRound(current, Math.Max(current, 999));
                            bridge.StartRound();
                            bridge.SetRound(dest, false);
                        }),
                        current + 1);
                }

                // Trigger the tower animation
                if (abilityName.Contains("FF"))
                {
                    var animator = __instance.tower.Node?.graphic?
                        .GetComponentInParent<Animator>();
                    if (animator != null)
                    {
                        animator.CrossFade("Idle", 0.001f);
                        animator.Play("Ability");
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  InGame.Update – drain the roundsToSkip queue one per frame
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(InGame), nameof(InGame.Update))]
        private static class InGame_Update
        {
            [HarmonyPostfix]
            private static void Postfix(InGame __instance)
            {
                if (__instance?.bridge == null || roundsToSkip <= 0) return;

                int current = __instance.bridge.GetCurrentRound();
                __instance.bridge.simulation.EndRound(current + 1, Math.Max(current + 1, 999));
                __instance.bridge.StartRound();
                __instance.bridge.SetRound(current + 1, false);
                roundsToSkip--;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Ability button – hide the pocket-watch button from the ability bar
        //  so it doesn't clutter the normal hotkey row
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(AbilityMenu), nameof(AbilityMenu.RebuildAbilities))]
        private static class AbilityMenu_RebuildAbilities
        {
            [HarmonyPostfix]
            private static void Postfix(AbilityMenu __instance)
            {
                InGame.instance.hotkeys.ClearAbilityButtonHotkeys();

                foreach (StackedAbilityButton btn in __instance.activeButtons)
                {
                    if (((AbilityButton)btn).currentAbilityIcon.guidRef.Contains("pocket_watch"))
                        ((Component)((AbilityButton)btn).Button).gameObject.SetActive(false);
                    else
                        InGame.instance.hotkeys.AddAbilityButton(btn);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Upgrade screen – redirect TimeMaster to DartMonkey so the game
        //  doesn't crash on the upgrade panel
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(UpgradeScreen), nameof(UpgradeScreen.UpdateUi))]
        private static class UpgradeScreen_UpdateUi
        {
            [HarmonyPrefix]
            private static bool Prefix(ref string towerId)
            {
                if (towerId.StartsWith("TimeMaster"))
                    towerId = "DartMonkey";
                return true;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Highlight / UnHighlight – manual material update for custom display
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(Tower), nameof(Tower.Hilight))]
        private static class Tower_Highlight
        {
            [HarmonyPostfix]
            private static void Postfix(Tower __instance) =>
                SetHighlight(__instance, 1f);
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.UnHighlight))]
        private static class Tower_UnHighlight
        {
            [HarmonyPostfix]
            private static void Postfix(Tower __instance) =>
                SetHighlight(__instance, 0f);
        }

        private static void SetHighlight(Tower tower, float value)
        {
            var renderers = tower?.Node?.graphic?.genericRenderers;
            if (renderers == null) return;
            for (int i = 0; i < renderers.Count; i++)
                renderers[i].material.SetFloat("_Highlighted", value);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Embedded-resource sprite loader for custom UI icons
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(SpriteAtlas), nameof(SpriteAtlas.GetSprite))]
        private static class SpriteAtlas_GetSprite
        {
            [HarmonyPrefix]
            private static bool Prefix(SpriteAtlas __instance, string name, ref Sprite __result)
            {
                if (__instance.name != "Ui") return true;

                byte[] bytes = name.Trim().GetEmbeddedResource();
                if (bytes == null || bytes.Length == 0) return true;

                Texture2D tex = bytes.ToTexture();
                __result = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    default(Vector2),
                    10.2f);
                __result.texture.mipMapBias = -1f;
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Suppress paragon pip events so the tower doesn't cause UI glitches
        // ─────────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(Btd6Player), nameof(Btd6Player.CheckForNewParagonPipEvent))]
        private static class Btd6Player_ParagonPip
        {
            [HarmonyPrefix]
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Collect the cash + XP that would have been earned over <paramref name="rounds"/> rounds,
        /// starting at the current round.
        /// </summary>
        private static void SkipRoundsAndCollect(int rounds)
        {
            var bridge = InGame.instance.bridge;
            for (int i = 0; i < rounds; i++)
            {
                int r = bridge.GetCurrentRound() + i;
                CollectCashAndXp(r, r);
            }
        }

        private static void CollectCashAndXp(int targetRound, int fromRound)
        {
            var bridge = InGame.instance?.bridge;
            if (bridge == null) return;

            var bonusCash = bridge.Model.behaviors
                .OfType<BonusCashPerRoundModel>()
                .FirstOrDefault();
            if (bonusCash != null)
                AddCash(bonusCash.baseCash + bonusCash.roundMultiple * targetRound);

            bridge.simulation.DistributeXp(targetRound);
        }

        private static void AddCash(double cash)
        {
            if (InGame.Bridge.Is<NetworkedUnityToSimulation>(out _))
            {
                foreach (var kv in InGame.Bridge.simulation.cashManagers)
                    InGame.Bridge.simulation.cashManagers[kv.Key].cash.Add(cash);
            }
            else
            {
                InGame.Bridge.AddCash(cash, (Il2CppAssets.Scripts.Simulation.CashSource)1);
            }
        }
    }
}
