

// TimeMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// TimeMaster.MelonMain
using Harmony;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.SimulationBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Display;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Audio;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.AbilitiesMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Unity.UI_New.Main.MapSelect;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Unity.UI_New.Upgrade;
using Il2CppAssets.Scripts.Unity.UI_New.Utils;
using Il2CppAssets.Scripts.Utils;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TimeMaster;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.U2D;
using static Il2CppAssets.Scripts.Simulation.Simulation;
using static Il2CppAssets.Scripts.Unity.UI_New.Popups.Popup;
using static Il2CppAssets.Scripts.Unity.UI_New.Popups.PopupScreen;
using ModHelperData = TimeMaster.ModHelperData;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using HarmonyPatch = HarmonyLib.HarmonyPatch;
using HarmonyPrefix = HarmonyLib.HarmonyPrefix;
using HarmonyPostfix = HarmonyLib.HarmonyPostfix;

[assembly: MelonInfo(typeof(TimeMaster.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace TimeMaster 
{ 
public class Main : MelonMod
{
        [HarmonyPatch(typeof(TowerManager), "UpgradeTower")]
        public sealed class DisplayFactory
    {
        //[HarmonyPrefix]
        //private static bool Prefix(__c__DisplayClass21_0 __instance, ref UnityDisplayNode prototype)
        //{
        //    //IL_00cd: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_00e6: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_00eb: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_00f4: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_00f6: Unknown result type (might be due to invalid IL or missing references)
        //    Factory _4__this = __instance.__4__this;
        //    PrefabReference objectId = __instance.objectId;
        //    string guidRef = objectId.guidRef;
        //    Action<UnityDisplayNode> onComplete = __instance.onComplete;
        //    _ = Game.instance.prototypeObjects.transform;
        //    ResourceManager resourceManager = Addressables.Instance.ResourceManager;
        //    if ((Object)(object)prototype == (Object)null && guidRef.Equals("TimeMonkey"))
        //    {
        //        GameObject obj = ((Il2CppObjectBase)Assets.LoadAsset(guidRef)).Cast<GameObject>();
        //        ((Object)obj).name = guidRef;
        //        UnityDisplayNode val = Object.Instantiate<GameObject>(obj, _4__this.DisplayRoot).AddComponent<UnityDisplayNode>();
        //        val.Active = false;
        //        ((Component)val).gameObject.AddComponent<TransformTimeMaster>();
        //        ((Object)val).name = guidRef + " (Clone)";
        //        val.RecalculateGenericRenderers();
        //        _4__this.prototypeHandles[objectId] = resourceManager.CreateCompletedOperation<GameObject>(((Component)val).gameObject, "");
        //        Vector3 val2 = default(Vector3);
        //        ((Vector3)(ref val2))..ctor(Factory.kOffscreenPosition.x, 0f, 0f);
        //        Quaternion identity = Quaternion.identity;
        //        GameObject obj2 = Object.Instantiate<GameObject>(((Component)val).gameObject, val2, identity, _4__this.DisplayRoot);
        //        obj2.SetActive(true);
        //        UnityDisplayNode component = obj2.GetComponent<UnityDisplayNode>();
        //        component.Create();
        //        component.cloneOf = objectId;
        //        _4__this.active.Add(component);
        //        onComplete.Invoke(component);
        //        return false;
        //    }
        //    return true;
        //}
    }



    [HarmonyPatch(typeof(AbilityMenu), "RebuildAbilities")]
    public static class AbilityMenu_ReturnAllAbilityButtons
    {
        [HarmonyPostfix]
        internal static void RebuildAbilities(ref AbilityMenu __instance)
        {
            InGame.instance.hotkeys.ClearAbilityButtonHotkeys();
            var enumerator = __instance.activeButtons.GetEnumerator();
            while (enumerator.MoveNext())
            {
                StackedAbilityButton current = enumerator.Current;
                if (((AbilityButton)current).currentAbilityIcon.guidRef.Contains("pocket_watch"))
                {
                    ((Component)((AbilityButton)current).Button).gameObject.SetActive(false);
                }
                else
                {
                    InGame.instance.hotkeys.AddAbilityButton(current);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Ability), "Activate")]
    public static class AbilityPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Ability __instance)
        {
            object obj;
            if (__instance == null)
            {
                obj = null;
            }
            else
            {
                Tower tower = ((TowerBehavior)__instance).tower;
                obj = ((tower != null) ? tower.towerModel : null);
            }
            if (obj == null || ((__instance != null) ? __instance.abilityModel : null) == null || !((TowerBehavior)__instance).tower.towerModel.baseId.Contains("TimeMaster"))
            {
                return;
            }
            if (((Model)__instance.abilityModel).name.Contains("FFOne"))
            {
                int currentRound = InGame.instance.bridge.GetCurrentRound();
                BonusCashPerRoundModel val = ((Il2CppObjectBase)(InGame.instance.bridge.Model.behaviors).First((SimulationBehaviorModel a) => ((object)a).Is<BonusCashPerRoundModel>())).Cast<BonusCashPerRoundModel>();
                AddCash(val.baseCash + val.roundMultiple * (float)currentRound);
                InGame.instance.bridge.simulation.DistributeXp(currentRound);
                InGame.instance.RoundEnd(currentRound, System.Math.Max(currentRound, 999));
                InGame.instance.bridge.StartRound();
                InGame.instance.bridge.SetRound(currentRound + 1, false);
                PlayBokudan();
            }
            if (((Model)__instance.abilityModel).name.Contains("FFTen"))
            {
                InGame.instance.bridge.GetCurrentRound();
                for (int num = 9; num >= 0; num--)
                {
                    int currentRound2 = InGame.instance.bridge.GetCurrentRound();
                    BonusCashPerRoundModel val2 = ((Il2CppObjectBase)(InGame.instance.bridge.Model.behaviors).First((SimulationBehaviorModel a) => ((Object)(object)a).Is<BonusCashPerRoundModel>())).Cast<BonusCashPerRoundModel>();
                    AddCash(val2.baseCash + val2.roundMultiple * (float)currentRound2);
                    InGame.instance.bridge.simulation.DistributeXp(currentRound2);
                }
                rounds += 10;
                PlayBokudan();
            }
            if (((Model)__instance.abilityModel).name.Contains("FFHundred"))
            {
                InGame.instance.bridge.GetCurrentRound();
                for (int num2 = 99; num2 >= 0; num2--)
                {
                    int currentRound3 = InGame.instance.bridge.GetCurrentRound();
                    BonusCashPerRoundModel val3 = ((Il2CppObjectBase)(InGame.instance.bridge.Model.behaviors).First((SimulationBehaviorModel a) => ((Object)(object)a).Is<BonusCashPerRoundModel>())).Cast<BonusCashPerRoundModel>();
                    AddCash(val3.baseCash + val3.roundMultiple * (float)currentRound3);
                    InGame.instance.bridge.simulation.DistributeXp(currentRound3);
                }
                rounds += 100;
                PlayBokudan();
            }
            if (((Model)__instance.abilityModel).name.Contains("FFReverse"))
            {
                int currentRound4 = InGame.instance.bridge.GetCurrentRound();
                InGame.instance.RoundEnd(currentRound4, System.Math.Max(currentRound4, 999));
                if (currentRound4 > 0)
                {
                    InGame.instance.bridge.StartRound();
                    InGame.instance.bridge.SetRound(currentRound4 - 1, false);
                    PlayBokudan();
                }
            }
            if (((Model)__instance.abilityModel).name.Contains("FFSelect") && ((TowerBehavior)__instance).tower.owner == InGame.Bridge.MyPlayerNumber)
            {
                int round = InGame.instance.bridge.GetCurrentRound();
                PopupScreen instance = PopupScreen.instance;
                var action = delegate (int i)
                {
                    if (i > 0)
                    {
                        int num3 = i - 1;
                        if (num3 > round)
                        {
                            BonusCashPerRoundModel val4 = ((Il2CppObjectBase)(InGame.instance.bridge.Model.behaviors).First((SimulationBehaviorModel a) => ((Object)(object)a).Is<BonusCashPerRoundModel>())).Cast<BonusCashPerRoundModel>();
                            AddCash(val4.baseCash + val4.roundMultiple * (float)i);
                            InGame.instance.bridge.simulation.DistributeXp(i);
                        }
                        InGame.instance.RoundEnd(round, System.Math.Max(round, 999));
                        InGame.instance.bridge.StartRound();
                        InGame.instance.bridge.SetRound(num3, false);
                    }
                };
                instance.ShowSetValuePopup("Enter a round to jump to", "Enter a round and the time master will make the jump to that time.", op_Implicit(action), round + 1);
            }
            if (((Model)__instance.abilityModel).name.Contains("FF"))
            {
                __instance.tower.Node.graphic.GetComponentInParent<Animator>().CrossFade("Idle", 0.001f);
                __instance.tower.Node.graphic.GetComponentInParent<Animator>().Play("Ability");
            }
        }

        private static void AddCash(double cash)
        {
            if (InGame.instance.bridge.Is<NetworkedUnityToSimulation>(out NetworkedUnityToSimulation _))
            {
                var enumerator = InGame.Bridge.simulation.cashManagers.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    InGame.Bridge.simulation.cashManagers[current.Key].cash.Add(cash);
                }
            }
            else
            {
                InGame.Bridge.AddCash(cash, (CashSource)1);
            }
        }
    }

    [HarmonyPatch(typeof(Tower), "UnHighlight")]
    internal static class Tower_UnHighlight
    {
        [HarmonyPostfix]
        public static void Postfix(Tower __instance)
        {
            object obj;
            if (__instance == null)
            {
                obj = null;
            }
            else
            {
                DisplayNode node = __instance.Node;
                if (node == null)
                {
                    obj = null;
                }
                else
                {
                    UnityDisplayNode graphic = node.graphic;
                    obj = ((graphic != null) ? graphic.genericRenderers : null);
                }
            }
            if (obj != null)
            {
                for (int i = 0; i < __instance.Node.graphic.genericRenderers.Count; i++)
                {
                    __instance.Node.graphic.genericRenderers[i].material.SetFloat("_Highlighted", 0f);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Tower), "Hilight")]
    internal static class Tower_Highlight
    {
        [HarmonyPostfix]
        public static void Postfix(Tower __instance)
        {
            object obj;
            if (__instance == null)
            {
                obj = null;
            }
            else
            {
                DisplayNode node = __instance.Node;
                if (node == null)
                {
                    obj = null;
                }
                else
                {
                    UnityDisplayNode graphic = node.graphic;
                    obj = ((graphic != null) ? graphic.genericRenderers : null);
                }
            }
            if (obj != null)
            {
                for (int i = 0; i < __instance.Node.graphic.genericRenderers.Count; i++)
                {
                    __instance.Node.graphic.genericRenderers[i].material.SetFloat("_Highlighted", 1f);
                }
            }
        }
    }

    [HarmonyPatch(typeof(SpriteAtlas), "GetSprite")]
    internal static class SpriteAtlas_GetSprite
    {
        [HarmonyPrefix]
        private static bool Prefix(SpriteAtlas __instance, string name, ref Sprite __result)
        {
            //IL_0046: Unknown result type (might be due to invalid IL or missing references)
            //IL_004d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0053: Unknown result type (might be due to invalid IL or missing references)
            if (__instance.name == "Ui")
            {
                byte[] embeddedResource = name.Trim().GetEmbeddedResource();
                if (embeddedResource != null && embeddedResource.Length != 0)
                {
                    Texture2D val = embeddedResource.ToTexture();
                    __result = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), default(Vector2), 10.2f);
                    ((Texture)__result.texture).mipMapBias = -1f;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DebugValueScreen), "Init", new Type[]
    {
        typeof(Action<int>),
        typeof(int),
        typeof(BackGround),
        typeof(TransitionAnim)
    })]
    public static class DebugValueScreen_Init
    {
        [HarmonyPostfix]
        public static void Postfix(ref DebugValueScreen __instance)
        {
            __instance.input.characterLimit = int.MaxValue.ToString().Length;
        }
    }

    private static int rounds;

    private static AssetBundle Assets;

    public override void OnInitializeMelon()
    {
        //IL_002e: Unknown result type (might be due to invalid IL or missing references)
        //IL_003c: Expected O, but got Unknown
        //IL_006b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0079: Expected O, but got Unknown
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b6: Expected O, but got Unknown
        //IL_00e5: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f3: Expected O, but got Unknown
        //IL_0123: Unknown result type (might be due to invalid IL or missing references)
        //IL_0130: Expected O, but got Unknown
        //IL_0160: Unknown result type (might be due to invalid IL or missing references)
        //IL_016d: Expected O, but got Unknown
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(Btd6Player), "CheckForNewParagonPipEvent", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "Pip", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(UpgradeScreen), "UpdateUi", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "UpdateScreenUI", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(GameModelLoader), "Load", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "GameModelLoaded", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(MonkeyTeamsIcon), "Init", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "MonkeyTeams", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(ProfileModel), "Validate", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "ValidateProfile", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        ((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(InGame), "Update", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(((object)this).GetType(), "UpdateInGame", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
        Assets = AssetBundle.LoadFromMemory(Il2CppStructArray<byte>.op_Implicit("TimeMaster.timemonkey".GetEmbeddedResource()));
    }

    public static void UpdateInGame(ref InGame __instance)
    {
        InGame obj = __instance;
        if (((obj != null) ? obj.bridge : null) != null)
        {
            int currentRound = __instance.bridge.GetCurrentRound();
            if (rounds > 0)
            {
                __instance.RoundEnd(currentRound + 1, Math.Max(currentRound + 1, 999));
                __instance.bridge.StartRound();
                __instance.bridge.SetRound(currentRound + 1, false);
                rounds--;
            }
        }
    }

    public static void GameModelLoaded(ref GameModel __result)
    {
        //IL_00f1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f6: Unknown result type (might be due to invalid IL or missing references)
        //IL_0106: Expected O, but got Unknown
        //IL_01df: Unknown result type (might be due to invalid IL or missing references)
        //IL_01e4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f4: Expected O, but got Unknown
        //IL_02cd: Unknown result type (might be due to invalid IL or missing references)
        //IL_02d2: Unknown result type (might be due to invalid IL or missing references)
        //IL_02e2: Expected O, but got Unknown
        //IL_03c0: Unknown result type (might be due to invalid IL or missing references)
        //IL_03c5: Unknown result type (might be due to invalid IL or missing references)
        //IL_03d5: Expected O, but got Unknown
        //IL_04b7: Unknown result type (might be due to invalid IL or missing references)
        //IL_04bc: Unknown result type (might be due to invalid IL or missing references)
        //IL_04cc: Expected O, but got Unknown
        TowerModel val = __result.GetTower("MonkeyVillage", 0, 0, 0).CloneCast<TowerModel>();
        val.towerSet = (TowerSet)8;
        val.range = 10f;
        val.cost = 0f;
        ((Model)val).name = "TimeMaster";
        val.baseId = "TimeMaster";
        val.SetIcons("TimeMaster.time_master.png");
        val.SetDisplay("TimeMonkey");
        val.upgrades = Il2CppReferenceArray<UpgradePathModel>.op_Implicit(Array.Empty<UpgradePathModel>());
        val.dontDisplayUpgrades = true;
        val.mods = Il2CppReferenceArray<ApplyModModel>.op_Implicit(Array.Empty<ApplyModModel>());
        val.radius = 10f;
        AbilityModel val2 = ((IEnumerable<Model>)__result.GetTower("DartlingGunner", 0, 4, 0).behaviors).First((Model a) => ((Object)(object)a).Is<AbilityModel>()).CloneCast<AbilityModel>();
        string text2 = (((Model)val2)._name = "FFOne");
        string name = (val2.displayName = text2);
        ((Model)val2).name = name;
        val2.icon = new SpriteReference
        {
            guidRef = "Ui[TimeMaster.pocket_watch_icon.png]"
        };
        val2.behaviors = val2.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<ActivateAttackModel>()));
        foreach (Model item in (Il2CppArrayBase<Model>)(object)val2.behaviors)
        {
            if (((Object)(object)item).Is<CreateSoundOnAbilityModel>(out CreateSoundOnAbilityModel ex))
            {
                ex.sound = null;
            }
        }
        val2.cooldown = 0.05f;
        AbilityModel val3 = ((IEnumerable<Model>)__result.GetTower("DartlingGunner", 0, 4, 0).behaviors).First((Model a) => ((Object)(object)a).Is<AbilityModel>()).CloneCast<AbilityModel>();
        text2 = (((Model)val3)._name = "FFTen");
        name = (val3.displayName = text2);
        ((Model)val3).name = name;
        val3.icon = new SpriteReference
        {
            guidRef = "Ui[TimeMaster.pocket_watch_icon_2.png]"
        };
        val3.behaviors = val3.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<ActivateAttackModel>()));
        foreach (Model item2 in (Il2CppArrayBase<Model>)(object)val3.behaviors)
        {
            if (((Object)(object)item2).Is<CreateSoundOnAbilityModel>(out CreateSoundOnAbilityModel ex2))
            {
                ex2.sound = null;
            }
        }
        val3.cooldown = 0.5f;
        AbilityModel val4 = ((IEnumerable<Model>)__result.GetTower("DartlingGunner", 0, 4, 0).behaviors).First((Model a) => ((Object)(object)a).Is<AbilityModel>()).CloneCast<AbilityModel>();
        text2 = (((Model)val4)._name = "FFHundred");
        name = (val4.displayName = text2);
        ((Model)val4).name = name;
        val4.icon = new SpriteReference
        {
            guidRef = "Ui[TimeMaster.pocket_watch_icon_3.png]"
        };
        val4.behaviors = val4.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<ActivateAttackModel>()));
        foreach (Model item3 in (Il2CppArrayBase<Model>)(object)val4.behaviors)
        {
            if (((Object)(object)item3).Is<CreateSoundOnAbilityModel>(out CreateSoundOnAbilityModel ex3))
            {
                ex3.sound = null;
            }
        }
        val4.cooldown = 1.5f;
        AbilityModel val5 = ((IEnumerable<Model>)__result.GetTower("DartlingGunner", 0, 4, 0).behaviors).First((Model a) => ((Object)(object)a).Is<AbilityModel>()).CloneCast<AbilityModel>();
        text2 = (((Model)val5)._name = "FFReverse");
        name = (val5.displayName = text2);
        ((Model)val5).name = name;
        val5.icon = new SpriteReference
        {
            guidRef = "Ui[TimeMaster.pocket_watch_icon_4.png]"
        };
        val5.behaviors = val5.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<ActivateAttackModel>()));
        foreach (Model item4 in (Il2CppArrayBase<Model>)(object)val5.behaviors)
        {
            if (((Object)(object)item4).Is<CreateSoundOnAbilityModel>(out CreateSoundOnAbilityModel ex4))
            {
                ex4.sound = null;
            }
        }
        val5.cooldown = 0.05f;
        AbilityModel val6 = ((IEnumerable<Model>)__result.GetTower("DartlingGunner", 0, 4, 0).behaviors).First((Model a) => ((Object)(object)a).Is<AbilityModel>()).CloneCast<AbilityModel>();
        text2 = (((Model)val6)._name = "FFSelect");
        name = (val6.displayName = text2);
        ((Model)val6).name = name;
        val6.icon = new SpriteReference
        {
            guidRef = "Ui[TimeMaster.pocket_watch_icon_5.png]"
        };
        val6.behaviors = val6.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<ActivateAttackModel>()));
        foreach (Model item5 in (Il2CppArrayBase<Model>)(object)val6.behaviors)
        {
            if (((Object)(object)item5).Is<CreateSoundOnAbilityModel>(out CreateSoundOnAbilityModel ex5))
            {
                ex5.sound = null;
            }
        }
        val6.cooldown = 1.5f;
        val.behaviors = val.behaviors.Remove<Model>((Func<Model, bool>)((Model a) => ((Object)(object)a).Is<RangeSupportModel>())).Add<Model>((Model[])(object)new Model[5]
        {
            (Model)val2,
            (Model)val3,
            (Model)val4,
            (Model)val5,
            (Model)val6
        });
        ShopTowerDetailsModel val7 = ((Model)(object)((Il2CppArrayBase<TowerDetailsModel>)(object)__result.towerSet)[0]).CloneCast<ShopTowerDetailsModel>();
        ((TowerDetailsModel)val7).towerId = "TimeMaster";
        ((TowerDetailsModel)val7).towerIndex = 294503994;
        __result.towers = __result.towers.Add<TowerModel>((TowerModel[])(object)new TowerModel[1] { val });
        __result.towerSet = __result.towerSet.Add<TowerDetailsModel>((TowerDetailsModel[])(object)new TowerDetailsModel[1] { (TowerDetailsModel)val7 });
    }

    public static void PlayBokudan()
    {
            var factory = new AudioFactory();
            factory.CreateStartingSources();
            factory.PlaySoundFromUnity(null, "bokudan", 5, 0.5f, 0f, false, "", false, false, true, Il2CppAssets.Scripts.Unity.Bridge.AudioType.FX);
    }

    public static void ValidateProfile(ref ProfileModel __instance)
    {
        __instance.unlockedTowers.AddIfNotPresent("TimeMaster");
    }

    public static bool MonkeyTeams(ref MonkeyTeamsIcon __instance, ref bool show, ref Il2CppStringArray monkeyTeams)
    {
        show = false;
        return true;
    }

    public static bool UpdateScreenUI(ref string towerId)
    {
        if (towerId.StartsWith("TimeMaster"))
        {
            towerId = "DartMonkey";
        }
        return true;
    }

    public static bool Pip(ref bool __result)
    {
        return __result = false;
    }
}
}