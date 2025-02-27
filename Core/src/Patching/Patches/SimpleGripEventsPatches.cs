﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using SLZ;
using SLZ.Interaction;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SimpleGripEvents))]
    public static class SimpleGripEventsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedDelegate))]
        public static bool OnAttachedDelegatePrefix(SimpleGripEvents __instance, Hand hand)
        {
            if (IsPlayerRep(__instance, hand))
                return false;
            else if (GetExtender(__instance, hand, out var syncable, out var extender))
            {
                // Decompiled code from CPP2IL
                if (__instance.doNotRetriggerOnMultiGirp)
                {
                    if (!__instance.leftHand && !__instance.rightHand)
                    {
                        SendGripEvent(syncable.GetId(), extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
                    }
                }
                else
                {
                    SendGripEvent(syncable.GetId(), extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnDetachedDelegate))]
        public static bool OnDetachedDelegatePrefix(SimpleGripEvents __instance, Hand hand)
        {
            if (IsPlayerRep(__instance, hand))
                return false;
            else if (GetExtender(__instance, hand, out var syncable, out var extender))
            {
                // Decompiled code from CPP2IL
                if (__instance.doNotRetriggerOnMultiGirp)
                {
                    bool rightHand = __instance.rightHand;
                    bool leftHand = __instance.leftHand;

                    if (hand.handedness != Handedness.LEFT)
                    {
                        rightHand = false;
                    }
                    leftHand = false; // This probably isn't how the logic is supposed to be but it's what the game does /shrug
                    if (leftHand || rightHand)
                    {
                        return true;
                    }
                }
                SendGripEvent(syncable.GetId(), extender.GetIndex(__instance).Value, SimpleGripEventType.DETACH);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
        public static bool OnAttachedUpdateDelegatePrefix(SimpleGripEvents __instance, Hand hand)
        {
            return !IsPlayerRep(__instance, hand);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
        public static void OnAttachedUpdateDelegatePostfix(SimpleGripEvents __instance, Hand hand)
        {
            if (GetExtender(__instance, hand, out var syncable, out var extender))
            {
                if (hand._indexButtonDown)
                {
                    SendGripEvent(syncable.Id, extender.GetIndex(__instance).Value, SimpleGripEventType.TRIGGER_DOWN);
                }

                if (hand.Controller.GetMenuTap())
                {
                    SendGripEvent(syncable.Id, extender.GetIndex(__instance).Value, SimpleGripEventType.MENU_TAP);
                }
            }
        }

        private static bool IsPlayerRep(SimpleGripEvents __instance, Hand hand)
        {
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager) && SimpleGripEventsExtender.Cache.ContainsSource(__instance))
            {
                return true;
            }

            return false;
        }

        private static bool GetExtender(SimpleGripEvents __instance, Hand hand, out PropSyncable syncable, out SimpleGripEventsExtender extender)
        {
            if (NetworkInfo.HasServer && hand.manager == RigData.RigReferences.RigManager && SimpleGripEventsExtender.Cache.TryGet(__instance, out syncable) && syncable.TryGetExtender(out extender))
                return true;
            else
            {
                syncable = null;
                extender = null;
                return false;
            }
        }

        private static void SendGripEvent(ushort syncId, byte gripEventIndex, SimpleGripEventType type)
        {
            using var writer = FusionWriter.Create(SimpleGripEventData.Size);
            using var data = SimpleGripEventData.Create(PlayerIdManager.LocalSmallId, syncId, gripEventIndex, type);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SimpleGripEvent, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}
