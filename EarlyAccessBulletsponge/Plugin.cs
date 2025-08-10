using System;
using System.Security.Permissions;
using System.Security;
using BepInEx;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RoR2.Projectile;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace EarlyAccessBulletsponge
{
    [BepInPlugin("com.Moffein.EarlyAccessBulletsponge", "EarlyAccessBulletsponge", "1.0.1")]
    public class EarlyAccessBulletspongePlugin : BaseUnityPlugin
    {
        public static string blacklistedBodyString = "GupBody, GeepBody, GipBody, GrandparentBody, MegaConstructBody, GreaterWispBody, BellBody, ClayBruiserBody," +
            "VoidBarnacleBody, HalcyoniteBody, ScorchlingBody, LunarGolemBody, LunarWispBody, LunarExploderBody, NullifierBody, VoidJailerBody," +
            "MoffeinArchWisp, RobYoungVagrantBody, BrotherBody, BrotherHurtBody, FalseSonBossBody, FalseSonBossBodyLunarShard, FalseSonBossBodyBrokenLunarShard," +
            "MiniVoidRaidCrabBodyBase, MiniVoidRaidCrabBodyPhase1, MiniVoidRaidCrabBodyPhase2, MiniVoidRaidCrabBodyPhase3, ArraignP1Body, ArraignP2Body, RegigigasBody," +
            "EngiTurretBody, EngiWalkerTurretBody, Drone1Body, Drone2Body, BackupDroneBody, EmergencyDroneBody, FlameDroneBody, MegaDroneBody, MissileDroneBody";

        public static float trashTierMultiplier = 1f;
        public static float impTierMultiplier = 200f / 140f;
        public static float golemTierMultiplier = 600f / 480f;
        public static float championTierMultiplier = 1.5f;

        public void Awake()
        {
            ReadConfig();
            RoR2Application.onLoad += RoR2Application_OnLoad;
        }

        private void ReadConfig()
        {
            blacklistedBodyString = Config.Bind<string>("General", "Body Blacklist", blacklistedBodyString, new BepInEx.Configuration.ConfigDescription("List of bodies unaffected by this mod, separated by commas. Anything with Drone in the name is auto-blacklisted.")).Value;
            trashTierMultiplier = Config.Bind<float>("General", "Trash Tier Multiplier", trashTierMultiplier, new BepInEx.Configuration.ConfigDescription("HP multiplier for low-health enemies.")).Value;
            impTierMultiplier = Config.Bind<float>("General", "Imp Tier Multiplier", impTierMultiplier, new BepInEx.Configuration.ConfigDescription("HP multiplier for Imp-tier enemies.")).Value;
            golemTierMultiplier = Config.Bind<float>("General", "Golem Tier Multiplier", golemTierMultiplier, new BepInEx.Configuration.ConfigDescription("HP multiplier for Golem-tier enemies.")).Value;
            championTierMultiplier = Config.Bind<float>("General", "Champion Tier Multiplier", championTierMultiplier, new BepInEx.Configuration.ConfigDescription("HP multiplier for Champion-tier enemies.")).Value;
        }

        private void RoR2Application_OnLoad()
        {
            //Blacklist from config
            List<string> blacklistedBodiesList = blacklistedBodyString.Split(",").Select(str => str.Trim()).ToList();
            HashSet<BodyIndex> indices = new HashSet<BodyIndex>();
            foreach (string str in blacklistedBodiesList)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(str);
                if (index != BodyIndex.None) indices.Add(index);
            }

            //Blacklist survivors
            List<GameObject> survivorBodies = new List<GameObject>();
            foreach (SurvivorDef sur in SurvivorCatalog.allSurvivorDefs)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(sur.bodyPrefab);
                if (index != BodyIndex.None) indices.Add(index);
            }

            foreach (var bodyObject in BodyCatalog.allBodyPrefabs)
            {
                if (bodyObject.GetComponent<ProjectileController>()) continue;
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();
                if (!body || indices.Contains(body.bodyIndex) || body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)) continue;
                if (body.gameObject.name.Contains("Drone")) continue;//Jonk

                if (body.isChampion)
                {
                    if (championTierMultiplier != 1f)
                    {
                        body.baseMaxHealth = Mathf.RoundToInt(body.baseMaxHealth * championTierMultiplier);
                        body.levelMaxHealth = Mathf.RoundToInt(body.levelMaxHealth * championTierMultiplier);
                    }
                }
                else
                {
                    if (body.baseMaxHealth < 140f)
                    {
                        if (trashTierMultiplier != 1f)
                        {
                            body.baseMaxHealth = Mathf.RoundToInt(body.baseMaxHealth * trashTierMultiplier);
                            body.levelMaxHealth = Mathf.RoundToInt(body.levelMaxHealth * trashTierMultiplier);
                        }
                    }
                    if (body.baseMaxHealth >= 140f && body.baseMaxHealth < 480f)
                    {
                        if (impTierMultiplier != 1f)
                        {
                            body.baseMaxHealth = Mathf.RoundToInt(body.baseMaxHealth * impTierMultiplier);
                            body.levelMaxHealth = Mathf.RoundToInt(body.levelMaxHealth * impTierMultiplier);
                        }
                    }
                    else if (body.baseMaxHealth >= 480f)
                    {
                        if (golemTierMultiplier != 1f)
                        {
                            body.baseMaxHealth = Mathf.RoundToInt(body.baseMaxHealth * golemTierMultiplier);
                            body.levelMaxHealth = Mathf.RoundToInt(body.levelMaxHealth * golemTierMultiplier);
                        }
                    }
                }
            }
        }
    }
}
