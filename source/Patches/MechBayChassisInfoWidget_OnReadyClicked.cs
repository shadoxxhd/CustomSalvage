﻿using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using HBS;
using UnityEngine;

namespace CustomSalvage;

[HarmonyPatch(typeof(MechBayChassisInfoWidget))]
[HarmonyPatch("OnReadyClicked")]
public static class MechBayChassisInfoWidget_OnReadyClicked
{

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ChassisDef ___selectedChassis, MechBayPanel ___mechBay
        , MechBayChassisUnitElement ___chassisElement, MechBayChassisInfoWidget __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (!Control.Instance.Settings.AssemblyVariants)
        {
            return;
        }

        if (___selectedChassis == null)
        {
            __runOriginal = false;
            return;
        }



        if (___mechBay.Sim.GetFirstFreeMechBay() < 0)
        {
            GenericPopupBuilder.Create("Cannot Ready 'Mech", "There are no available slots in the 'Mech Bay. You must move an active 'Mech into storage before readying this chassis.").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0f, true).Render();
            __runOriginal = false;
            return;
        }

        ChassisHandler.PreparePopup(___selectedChassis, ___mechBay, __instance, ___chassisElement);

        int chassisQuantity = ___mechBay.Sim.GetItemCount(___selectedChassis.Description.Id, typeof(MechDef), SimGameState.ItemCountType.UNDAMAGED_ONLY);
        if (___selectedChassis.MechPartCount == 0)
        {
            if(chassisQuantity <= 0)
            {
                GenericPopupBuilder.Create("Absent parts","You does not have parts or complete chassis for this unit in storage")
                    .AddButton("Cancel", null, true, null)
                    .AddButton("Ready", ChassisHandler.OnChassisReady, true, null)
                    .AddFader(
                        new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants
                            .PopupBackfill), 0f, true)
                    .CancelOnEscape()
                    .Render();
                __runOriginal = false;
                return;
            }
            int num2 = Mathf.CeilToInt((float)___mechBay.Sim.Constants.Story.MechReadyTime /
                                       (float)___mechBay.Sim.MechTechSkill);

            GenericPopupBuilder.Create("Ready 'Unit?",
                    $"It will take {num2} day(s) to ready this BattleMech chassis for combat.")
                .AddButton("Cancel", null, true, null)
                .AddButton("Ready", ChassisHandler.OnChassisReady, true, null)
                .AddFader(
                    new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants
                        .PopupBackfill), 0f, true)
                .CancelOnEscape()
                .Render();
            __runOriginal = false;
            return;
        }
        //int mechPartCount = ChassisHandler.GetCount(ChassisHandler.GetMech(___selectedChassis.Description.Id).Description.Id);
        if (___selectedChassis.MechPartCount >= ___selectedChassis.MechPartMax)
        {
            if (Control.Instance.Settings.MechBrokeType == BrokeType.Normalized)
            {
                ChassisHandler.StartDialog();
            }
            else
            {
                GenericPopupBuilder.Create("Assembly 'Unit?",
                        $"It will take {___selectedChassis.MechPartMax} parts from storage.")
                    .AddButton("Cancel", null, true, null)
                    .AddButton("Ready", ChassisHandler.OnPartsAssembly, true, null)
                    .AddFader(
                        new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants
                            .PopupBackfill), 0f, true)
                    .CancelOnEscape()
                    .Render();
            }

            __runOriginal = false;
            return;
        }

        if (!Control.Instance.Settings.AssemblyVariants)
        {
            return;
        }

        ChassisHandler.StartDialog();

        __runOriginal = false;
    }

}