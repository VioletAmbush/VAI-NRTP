﻿using EFT.Ballistics;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static TarkovRPG.ConfigRepository;
using BepInEx.Logging;
using System.Text;
using JsonType;
using UnityEngine;
using EFT.HealthSystem;
using EFT.InventoryLogic;

namespace TarkovRPG
{
    public static class Patches
    {
        private static Plugin Instance => Plugin.Instance!;
        private static ManualLogSource Logger => Plugin.Instance!.Logger;
        private static ConfigRepository ConfigRepository => Plugin.Instance!.ConfigRepository!;

        [HarmonyPatch(typeof(BallisticsCalculator), "CreateShot")]
        [HarmonyPrefix]
        private static bool CreateShot(
            BallisticsCalculator __instance,
            ref EftBulletClass __result,
			AmmoItemClass __0,
            UnityEngine.Vector3 __1,
            UnityEngine.Vector3 __2,
            int __3,
            string __4,
            Item __5,
            float __6 = 1f,
            int __7 = 0)
		{
			if (!ConfigRepository[Section.DamageSettings] || Plugin.Instance == null || !(__5 is Weapon weapon))
            {
                return true;
            }

            float damage = __0.Damage;
            float damageMult = ConfigRepository[DamageStatKey.Default];

            if (weapon.BoltAction &&
                weapon.WeapClass.Equals("sniperRifle"))
            {
                damageMult = ConfigRepository[DamageStatKey.BoltAction];
            }
            else if (
                weapon.Template._id == "59f9cabd86f7743a10721f46" ||
                weapon.Template._id == "60339954d62c9b14ed777c06")
            {
                damageMult = ConfigRepository[DamageStatKey.SingleSMGs];
            }
            else if (weapon.Template._id == "61f7c9e189e6fb1a5e3ea78d") // Break-action single-fire rifle
            {
                damageMult = ConfigRepository[DamageStatKey.BoltAction];
            }
            // TODO: Refine
            else if ((!weapon.BoltAction &&
                weapon.WeapClass.Equals("sniperRifle") &&
                (weapon.WeapFireType.Contains(Weapon.EFireMode.single) ||
                weapon.WeapFireType.Contains(Weapon.EFireMode.semiauto)) &&
                !weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto)) ||

                weapon.WeapClass.Equals("assaultCarbine") ||

                weapon.WeapClass.Equals("marksmanRifle") ||

                (weapon.WeapClass.Equals("assaultRifle") &&
                (weapon.WeapFireType.Contains(Weapon.EFireMode.single) ||
                weapon.WeapFireType.Contains(Weapon.EFireMode.semiauto)) &&
                !weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto)) ||

                weapon.Template._id == "59e6687d86f77411d949b251") // One of VPOs is a shotgun for some reason
            {
                damageMult = ConfigRepository[DamageStatKey.Marksman];
            }
            else if (weapon.WeapClass.Equals("shotgun"))
            {
                damageMult = ConfigRepository[DamageStatKey.Shotgun];
            }
            else if (weapon.WeapClass.Equals("pistol"))
            {
                if (weapon.WeapFireType.Contains(Weapon.EFireMode.doubleaction))
                {
                    damageMult = ConfigRepository[DamageStatKey.Revolver];
                }
                else
                {
                    damageMult = ConfigRepository[DamageStatKey.Pistol];
                }
            }

            int num1 = UnityEngine.Random.Range(0, 512);
            float num2 = __0.InitialSpeed * __6;

            if (damageMult == default)
                damageMult = 1f;

            damage *= damageMult;

            __result = EftBulletClass.Create(
                __0,
                __7,
                num1,
                __1,
                __2,
                num2,
                num2,
                __0.BulletMassGram,
                __0.BulletDiameterMilimeters,
                (float)damage,
                BallisticsCalculator.GetAmmoPenetrationPower(__0, num1, __instance.Randoms),
                __0.PenetrationChanceObstacle,
                __0.RicochetChance,
                __0.FragmentationChance,
                1f,
                __0.MinFragmentsCount,
                __0.MaxFragmentsCount,
                BallisticsCalculator.DefaultHitBody,
                __instance.Randoms,
                __0.BallisticCoeficient,
                __4,
                __5,
                __3,
                null);

#if DEBUG
            Logger.LogInfo($"{weapon.Template.Name} {weapon.WeapClass} Bang! Damage (x{damageMult:F1}): {damage:F0}");
#endif
			return false;
        }

        [HarmonyPatch(typeof(Player), "ProceedDamageThroughArmor")]
        [HarmonyPrefix]
        public static bool ProceedDamageThroughArmor(
            Player? __instance,
            ref List<ArmorComponent>? __result,
            ref DamageInfoStruct __0,
			EBodyPartColliderType __1,
			EArmorPlateCollider __2,
            bool __3)
        {
            if (!ConfigRepository[Section.ArmorSettings] || Instance == null || __instance == null)
                return true;

            var damageInfoIsLocal = __3;

            var _preAllocArmorComps = __instance.GetPrivateFieldValue<List<ArmorComponent>>("_preAllocatedArmorComponents");
            _preAllocArmorComps.Clear();
            __instance.Inventory.GetPutOnArmorsNonAlloc(_preAllocArmorComps);

            List<ArmorComponent> armorComponentList = new List<ArmorComponent>();

            var armorClass = 0;
            bool flag3 = _preAllocArmorComps.Any(comp => comp.Item.Template._id == GClass3178.InvincibleBalaclava);

            foreach (ArmorComponent allocatedArmorComponent in _preAllocArmorComps)
            {
                float armorDamage = 0.0f;

                if (allocatedArmorComponent.ShotMatches(__1, __2))
                {
                    armorClass = Math.Max(armorClass, allocatedArmorComponent.ArmorClass);

                    armorComponentList.Add(allocatedArmorComponent);

                    if (__instance.GetPrivateFieldValue<IHealthController>("_healthController").IsAlive)
                    {
                        var damage = __0.Damage;
                        armorDamage = allocatedArmorComponent.ApplyDamage(
                            ref __0, 
                            __1,
                            __2,
                            damageInfoIsLocal,
                            _preAllocArmorComps,
                            __instance.Skills.LightVestMeleeWeaponDamageReduction, 
                            __instance.Skills.HeavyVestBluntThroughputDamageReduction);

                        __0.Damage = damage;

                        if (allocatedArmorComponent.Buff.IsActive && allocatedArmorComponent.Buff.BuffType == ERepairBuffType.DamageReduction)
                        {
                            __0.Damage *= (float)allocatedArmorComponent.Buff.DamageReduction;
                        }

                        if (armorDamage > 0.1f && !allocatedArmorComponent.IsDestroyed)
                        {
                            if (allocatedArmorComponent.ArmorType == EArmorType.Light)
                            {
                                __instance.Skills.LightArmorDamageTakenAction.Complete(armorDamage);
                            }
                            else if (allocatedArmorComponent.ArmorType == EArmorType.Heavy)
                            {
                                __instance.Skills.HeavyArmorDamageTakenAction.Complete(armorDamage);
                            }
                        }
                    }
                }

                if ((double)armorDamage > 0.10000000149011612)
                    __instance.OnArmorPointsChanged(allocatedArmorComponent);
            }

            if (flag3)
                __0.Damage = 0.0f;
            else
            {
                if (__0.DamageType == EDamageType.Bullet ||
                    __0.DamageType == EDamageType.GrenadeFragment ||
                    __0.DamageType == EDamageType.Sniper)
                {
                    var damageMultiplier = ConfigRepository[FromClassToEnum(armorClass)];
                    damageMultiplier += (__0.PenetrationPower / 100f);
                    damageMultiplier = Math.Min(1f, damageMultiplier);
                    damageMultiplier = damageMultiplier < 0f ? 0f : damageMultiplier;

#if DEBUG
                    if (armorClass != 0)
                        Logger.LogInfo($"Armor hit! Class {armorClass} Pen:{__0.PenetrationPower:F1} Mult:x{damageMultiplier:F2} Dam:{__0.Damage:F0}->{__0.Damage * damageMultiplier:F0}");
#endif

                    __0.Damage *= damageMultiplier;
                }
            }

            __result = armorComponentList;
            return false;
        }

        private static ArmorStatKey FromClassToEnum(int armorClass)
        {
            switch (armorClass)
            {
                case 0: return ArmorStatKey.Unarmored;
                case 1: return ArmorStatKey.Class1;
                case 2: return ArmorStatKey.Class2;
                case 3: return ArmorStatKey.Class3;
                case 4: return ArmorStatKey.Class4;
                case 5: return ArmorStatKey.Class5;
                case 6: return ArmorStatKey.Class6;
                default: return ArmorStatKey.Default;
            }
        }

        [HarmonyPatch(typeof(Player), "ApplyShot")]
        [HarmonyPostfix]
        private static void ApplyShot(
            Player __instance,
			ShotInfoClass? __result,
            DamageInfoStruct __0,
            EBodyPart __1,
			EBodyPartColliderType __2,
			EArmorPlateCollider __3,
			ShotIdStruct __4)
        {
            if (!ConfigRepository[Section.ArmorSettings])
                return;

            var damageInfo = __0;
            var bodyPartType = __1;

            damageInfo.BleedBlock = true;
            damageInfo.DamageType = EDamageType.Impact;

            var bodyPart = __instance.HealthController.GetBodyPartHealth(bodyPartType);

            var massDamage = damageInfo.Damage * ConfigRepository[DamageStatKey.Mass];

#if DEBUG
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{__instance.Profile.Nickname} hit for {damageInfo.Damage:F0}, {massDamage:F0}! Health:");
            stringBuilder.AppendLine($"{bodyPartType}:{bodyPart.Current:F0}/{bodyPart.Maximum:F0}");
#endif

            foreach (EBodyPart part in Enum.GetValues(typeof(EBodyPart)))
            {
                if (part == bodyPartType || part == EBodyPart.Common)
                    continue;

                __instance.ActiveHealthController.ApplyDamage(
                    part,
                    massDamage,
                    damageInfo);

#if DEBUG
				bodyPart = __instance.HealthController.GetBodyPartHealth(part);

                stringBuilder.AppendLine($"{part}:{bodyPart.Current:F0}/{bodyPart.Maximum:F0}");
#endif
            }

#if DEBUG
            Logger.LogInfo(stringBuilder.ToString());
#endif
        }

        [HarmonyPatch(typeof(GClass1338), "ToColor")]
        [HarmonyPostfix]
        private static void ToColor(
            ref UnityEngine.Color __result,
            TaxonomyColor __0)
        {
			if (!ConfigRepository[Section.ColorSettings])
                return; 

            if (__0 == TaxonomyColor.green)
            {
                __result = ColorFromRGB(82, 145, 57);
            }
            else if (__0 == TaxonomyColor.orange)
            {
                __result = ColorFromRGB(135, 73, 26);
            }
            else
            {
                __result.r *= 1.2f;
                __result.g *= 1.2f;
                __result.b *= 1.2f;
            }
        }

        private static Color ColorFromRGB(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            return new Color32(r, g, b, a);
        }
    }
}