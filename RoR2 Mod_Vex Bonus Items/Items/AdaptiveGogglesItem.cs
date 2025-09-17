using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2_Mod_Vex_Bonus_Items.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2_Mod_Vex_Bonus_Items.Main;

namespace RoR2_Mod_Vex_Bonus_Items.Items
{
    internal class AdaptiveGogglesItem : ItemBase<AdaptiveGogglesItem>
    {
        public List<CharacterBody> playerBodies = new List<CharacterBody>();

        public ConfigEntry<float> abilityCooldown;
        public ConfigEntry<float> abilityCooldownPercentPerItem;
        public ConfigEntry<float> equipmentCooldown;
        public ConfigEntry<float> equipmentCooldownPercentPerItem;

        public override string ItemName => "Adaptive Goggles";

        public override string ItemLangTokenName => "ADAPTIVE_GOGGLES";

        public override string ItemPickupDesc => "Reduce ability and equipment cooldowns on enemy spawn. <style=cIsVoid>Corrupts all Soulbound Catalysts</style>.";

        public override string ItemFullDescription => $"On enemy spawn, <style=cIsUtility>reduce all ability cooldowns</style> by <style=cIsUtility>{abilityCooldown.Value}s</style> <style=cStack>(+{abilityCooldownPercentPerItem.Value * 100}% per item)</style> and <style=cIsUtility>reduce equipment cooldown</style> by <style=cIsUtility>{equipmentCooldown.Value}s</style> <style=cStack>(+{equipmentCooldownPercentPerItem.Value * 100}% per item)</style>. <style=cIsVoid>Corrupts all Soulbound Catalysts</style>.";

        public override string ItemLore => "Somewhere in the midst of the Universe, on a barren, war-torn desert planet, drifters are finally banding together to revive their creed of order and structure.\r\n\r\nSoldier: \"I spotted the rebels, they're driving straight towards us!\"\r\nLeader: \"How far out were they?\"\r\nSoldier: \"My goggles told me they're eight thousand meters out, should take them another hour.\"\r\nLeader: \"Then we've got plenty of time to prepare. Charge the compound rifles.\"";

        public override ItemTier Tier => ItemTier.VoidTier3;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override bool CanRemove => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AdaptiveGogglesPickup.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AdaptiveGogglesIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            SetLogbookAppearance(.5f, 1f);
            CreateItem();
            ItemDef.requiredExpansion = sotvDLC;
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            abilityCooldown = config.Bind<float>("Item: " + ItemName, "Inital Ability Cooldown Seconds", 0.5f, "How much should cooldowns be reduced per enemy spawned?");
            abilityCooldownPercentPerItem = config.Bind<float>("Item: " + ItemName, "Ability Cooldown Percent Per Item", 0.6f, "How much should cooldowns be reduced per item?");
            equipmentCooldown = config.Bind<float>("Item: " + ItemName, "Initial Equipment Cooldown Seconds", 1f, "How much should the player's damage increase?");
            equipmentCooldownPercentPerItem = config.Bind<float>("Item: " + ItemName, "Equipment Cooldown Percent Per Item", 0.6f, "How much should cooldowns be reduced per item?");

            voidPair = config.Bind<string>("Item: " + ItemName, "Item to Corrupt", "Talisman", "Adjust which item this is the void pair of.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            GameObject ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("AdaptiveGogglesDisplay.prefab");
            ItemDisplay itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            // Press F2 on Main Menu to access ItemDisplayPlacementHelper
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00023F, 0.29662F, 0.19404F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00013F, 0.22F, 0.14775F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00002F, 0.05068F, 0.14588F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00282F, 3.65876F, -0.68874F),
                    localAngles = new Vector3(56.99331F, 358.0688F, 0.74397F),
                    localScale = new Vector3(7F, 7F, 7F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.00108F, 0.02528F, 0.16459F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("EngiTurretBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.75195F, 1.13189F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("EngiWalkerTurretBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.26142F, 0.9694F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2.5F, 2.5F, 2.5F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00005F, 0.05909F, 0.12911F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00002F, 0.12452F, 0.17072F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, -0.78094F, 0.64384F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2.8F, 2.8F, 2.8F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 0.1006F, 0.15895F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00546F, 4.4F, 1.74F),
                    localAngles = new Vector3(54.35565F, 180.2932F, 359.8888F),
                    localScale = new Vector3(7F, 7F, 7F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.08F, 0.18004F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.13305F, 0.41927F, 0.00799F),
                    localAngles = new Vector3(70.00002F, 84.00001F, 174.8418F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00121F, 0.05571F, 0.12564F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.005F, 0.07911F, 0.18065F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.15157F, 0.14195F),
                    localAngles = new Vector3(7.25874F, 180.208F, 359.7437F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00124F, 0.21448F, 0.20936F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.19767F, 0.17975F, -0.00241F),
                    localAngles = new Vector3(90F, 90.00001F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });

            return rules;
        }

        public override void Hooks()
        {
            // Determines when effects are applied
            On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChange;
            On.RoR2.CombatDirector.Spawn += OnEnemySpawn;
        }

        private void OnInventoryChange(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            // Only look at player inventories
            if (!self.isPlayerControlled)
            {
                orig(self);
                return;
            }

            // Add the player if they aren't in the list and have the item
            if (!playerBodies.Contains(self) && GetCount(self) > 0)
            {
                playerBodies.Add(self);
            }
            // Remove the player if they are in the list and don't have the item
            else if (playerBodies.Contains(self) && GetCount(self) <= 0)
            {
                playerBodies.Remove(self);
            }

            orig(self);
        }

        private bool OnEnemySpawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            foreach (CharacterBody body in playerBodies)
            {
                int inventoryCount = GetCount(body);
                
                if (inventoryCount <= 0)
                {
                    // Do nothing
                }
                else if (inventoryCount == 1)
                {
                    // Ability Cooldown Reduction
                    float acr = abilityCooldown.Value;
                    // Equipment Cooldown Reduction
                    float ecr = equipmentCooldown.Value;

                    SkillLocator locator = body.skillLocator;
                    locator.primary.rechargeStopwatch += acr;
                    locator.secondary.rechargeStopwatch += acr;
                    locator.utility.rechargeStopwatch += acr;
                    locator.special.rechargeStopwatch += acr;

                    body.inventory.DeductActiveEquipmentCooldown(ecr);

                    ModLogger.LogInfo("Reduce ability cooldowns by " + acr + "s and reduce equipment cooldown by " + ecr + "s.");
                }
                else
                {
                    // Ability Cooldown Reduction
                    float acr = abilityCooldown.Value + (inventoryCount - 1) * abilityCooldownPercentPerItem.Value * abilityCooldown.Value;
                    // Equipment Cooldown Reduction
                    float ecr = equipmentCooldown.Value + (inventoryCount - 1) * equipmentCooldownPercentPerItem.Value * equipmentCooldown.Value;

                    SkillLocator locator = body.skillLocator;
                    locator.primary.rechargeStopwatch += acr;
                    locator.secondary.rechargeStopwatch += acr;
                    locator.utility.rechargeStopwatch += acr;
                    locator.special.rechargeStopwatch += acr;

                    body.inventory.DeductActiveEquipmentCooldown(ecr);

                    ModLogger.LogInfo("Reduce ability cooldowns by " + acr + "s and reduce equipment cooldown by " + ecr + "s.");
                }
            }
            
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }
    }
}
