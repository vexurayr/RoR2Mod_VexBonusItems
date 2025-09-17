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
    internal class SwordOfLootingItem : ItemBase<SwordOfLootingItem>
    {
        public List<CharacterBody> playerBodies = new List<CharacterBody>();
        public bool isNewStage = true;

        public ConfigEntry<int> additionalItems;
        public ConfigEntry<int> additionalItemsPerItem;

        public override string ItemName => "Sword of Looting";

        public override string ItemLangTokenName => "SWORD_OF_LOOTING";

        public override string ItemPickupDesc => "Bosses drop additional items per player.";

        public override string ItemFullDescription => CorrectItemFullDescription();

        public override string ItemLore => "Legends speak of a man who strived so hard to break his limits he became a god. His imagination was his only limitation.\r\n\nOne of his proudest feats was crafting a functional sword purely out of diamond. His second proudest moment was learning how to enchant it.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override bool CanRemove => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("SwordOfLootingPickup.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("SwordOfLootingIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            SetLogbookAppearance(2.8f, 4.8f);
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            additionalItems = config.Bind<int>("Item: " + ItemName, "Additional Items Dropped", 1, "How many more items should a teleporter boss give per player?");
            additionalItemsPerItem = config.Bind<int>("Item: " + ItemName, "Additional Items Dropped Per Item", 1, "How many more items should be given per item?");
        }

        public string CorrectItemFullDescription()
        {
            // Check configs to update ItemFullDescription
            if (additionalItems.Value > 1 && additionalItemsPerItem.Value <= 1)
            {
                // additionalItems is plural
                return $"When a <style=cHumanObjective>teleporter boss</style> would drop an item, drop <style=cIsUtility>{additionalItems.Value} more items</style> per player <style=cStack>(+{additionalItemsPerItem.Value} item per item)</style>.";
            }
            else if (additionalItems.Value <= 1 && additionalItemsPerItem.Value > 1)
            {
                // additionalItemsPerItem is plural
                return $"When a <style=cHumanObjective>teleporter boss</style> would drop an item, drop <style=cIsUtility>{additionalItems.Value} more item</style> per player <style=cStack>(+{additionalItemsPerItem.Value} items per item)</style>.";
            }
            else if (additionalItems.Value > 1 && additionalItemsPerItem.Value > 1)
            {
                // Both are plural
                return $"When a <style=cHumanObjective>teleporter boss</style> would drop an item, drop <style=cIsUtility>{additionalItems.Value} more items</style> per player <style=cStack>(+{additionalItemsPerItem.Value} items per item)</style>.";
            }
            else
            {
                // Neither are plural
                return $"When a <style=cHumanObjective>teleporter boss</style> would drop an item, drop <style=cIsUtility>{additionalItems.Value} more item</style> per player <style=cStack>(+{additionalItemsPerItem.Value} item per item)</style>.";
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            GameObject ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("SwordOfLootingDisplay.prefab");
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
                    childName = "Chest",
                    localPos = new Vector3(0.00403F, 0.34192F, -0.19375F),
                    localAngles = new Vector3(342.6127F, 180.0179F, 179.8824F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05362F, 0.05742F, -0.12749F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00442F, 0.17429F, -0.17939F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.38361F, 1.50016F, -2.3852F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(2.8F, 2.8F, 2.8F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.04852F, 0.19196F, -0.31372F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("EngiTurretBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02861F, 0.42224F, -1.50942F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            rules.Add("EngiWalkerTurretBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00711F, 0.77825F, -1.68756F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.02414F, 0.09417F, -0.39658F),
                    localAngles = new Vector3(352.3033F, 180.0791F, 181.8248F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01744F, 0.11645F, -0.3126F),
                    localAngles = new Vector3(353.2321F, 179.9853F, 180.4361F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(0.02243F, 0.31684F, -0.88185F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(1.12F, 1.12F, 1.12F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.03341F, 0.08339F, -0.37888F),
                    localAngles = new Vector3(358.2817F, 180.0001F, 179.9887F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.04407F, -0.33761F, 4.26154F),
                    localAngles = new Vector3(28.54525F, 0.04101F, 180.2169F),
                    localScale = new Vector3(2.8F, 2.8F, 2.8F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.0305F, 0.15621F, -0.2226F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0689F, -0.52025F, -0.07424F),
                    localAngles = new Vector3(60.94838F, 97.99738F, 22.47078F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.06541F, -0.0483F, -0.13284F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02711F, -0.02416F, -0.2669F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pack",
                    localPos = new Vector3(-0.01219F, -0.10683F, -0.38861F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00898F, 0.17587F, -0.42282F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.03293F, -0.50478F, 0.13087F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });

            return rules;
        }

        public override void Hooks()
        {
            // Determines when effects are applied
            On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChange;
            On.RoR2.Run.OnServerBossAdded += OnBossAdded;
            On.RoR2.Run.BeginStage += OnBeginStage;
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

        // This function is called for every boss spawned for the teleporter event
        // so in order to keep the code that adds to bossGroup.bonusRewardCount rather than hard set it
        // I need to make sure code in this method only runs once per stage.
        private void OnBossAdded(On.RoR2.Run.orig_OnServerBossAdded orig, Run self, BossGroup bossGroup, CharacterMaster characterMaster)
        {
            if (isNewStage)
            {
                isNewStage = false;
            }
            else
            {
                return;
            }

            // Get the number of players in the run
            int playerCount = self.participatingPlayerCount;

            // Get the total additional items to spawn across all player inventories
            int totalItems = 0;

            foreach (CharacterBody body in playerBodies)
            {
                // Items in this inventory
                int inventoryCount = GetCount(body);

                if (inventoryCount <= 0)
                {
                    // Do nothing
                }
                else if (inventoryCount == 1)
                {
                    totalItems += additionalItems.Value;
                }
                else
                {
                    totalItems += additionalItems.Value + (inventoryCount - 1) * additionalItemsPerItem.Value;
                }
            }
            
            bossGroup.bonusRewardCount += playerCount * totalItems;

            ModLogger.LogInfo("Player Count: " + playerCount + ", Items Per Player: " + totalItems);

            orig(self, bossGroup, characterMaster);
        }

        private void OnBeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            isNewStage = true;
            orig(self);
        }
    }
}
