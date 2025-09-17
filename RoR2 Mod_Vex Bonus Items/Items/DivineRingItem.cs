using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2_Mod_Vex_Bonus_Items.Utils;
using static RoR2_Mod_Vex_Bonus_Items.Main;

namespace RoR2_Mod_Vex_Bonus_Items.Items
{
    public class DivineRingItem : ItemBase<DivineRingItem>
    {
        public ConfigEntry<float> healthIncreasePercentPerLevel;
        public ConfigEntry<float> regenerationIncreasePercentPerLevel;
        public ConfigEntry<float> damageIncreasePercentPerLevel;
        public ConfigEntry<float> experienceIncreasePercentPerLevel;

        public override string ItemName => "Divine Ring";

        public override string ItemLangTokenName => "DIVINE_RING";

        public override string ItemPickupDesc => "Increase base stats and experience earned. Scales with character level.";

        public override string ItemFullDescription => $"Increase <style=cIsHealth>health</style> per level by " +
            $"<style=cIsHealth>{healthIncreasePercentPerLevel.Value * 100}%</style> " +
            $"<style=cStack>(+{healthIncreasePercentPerLevel.Value * 100}% per item)</style>, " +
            $"increase <style=cIsHealing>regeneration</style> per level by " +
            $"<style=cIsHealing>{regenerationIncreasePercentPerLevel.Value * 100}%</style> " +
            $"<style=cStack>(+{regenerationIncreasePercentPerLevel.Value * 100}% per item)</style>, " +
            $"increase <style=cIsDamage>damage</style> per level by " +
            $"<style=cIsDamage>{damageIncreasePercentPerLevel.Value * 100}%</style> " +
            $"<style=cStack>(+{damageIncreasePercentPerLevel.Value * 100}% per item)</style>, and " +
            $"increase <style=cIsUtility>experience gain</style> per level by " +
            $"<style=cIsUtility>{experienceIncreasePercentPerLevel.Value * 100}%</style> " +
            $"<style=cStack>(+{experienceIncreasePercentPerLevel.Value * 100}% per item)</style>.";

        public override string ItemLore => "<style=cEvent>//--TRANSCRIPT FROM UNIDENTIFIED ARTIFACT STORAGE--//</style>\r\n\n\"What do you think it is, Bob?\"\r\n\n\"Hard to say. Frank just filed a new identification form, and the only note he left is... it's a ring.\"\r\n\n\"Are his eyes going bad? That can't be right, it's clearly the size of a crown.\"\r\n\n<style=cEvent>//--END TRANSCRIPT--//</style>";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override bool CanRemove => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("DivineRingPickup.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("DivineRingIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            SetLogbookAppearance(.8f, 1.8f);
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            healthIncreasePercentPerLevel = config.Bind<float>("Item: " + ItemName, "Health Increase Percent Per Level", 0.05f, "What should the player's health be multiplied by?");
            regenerationIncreasePercentPerLevel = config.Bind<float>("Item: " + ItemName, "Regeneration Increase Percent Per Level", 0.05f, "What should the player's regeneration be multiplied by?");
            damageIncreasePercentPerLevel = config.Bind<float>("Item: " + ItemName, "Damage Increase Percent Per Level", 0.05f, "What should the player's damage be multiplied by?");
            experienceIncreasePercentPerLevel = config.Bind<float>("Item: " + ItemName, "Experience Gain Increase Percent Per Level", 5f, "What should the player's experience gain be multiplied by?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            GameObject ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DivineRingDisplay.prefab");
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
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.35f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.15217f, 4.40077f, 2.42042f),
                    localAngles = new Vector3(56.99331f, 358.0688f, 0.74397f),
                    localScale = new Vector3(7, 7, 7)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("EngiTurretBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
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
                    localPos = new Vector3(0F, 1.71F, -0.52957F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.4f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.05f, 2.6f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(2.8f, 2.8f, 2.8f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.13884f, 0.00953f, 3.81179f),
                    localAngles = new Vector3(291.4786f, 0.37527f, 358.321f),
                    localScale = new Vector3(7, 7, 7)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.59379f, 0.05884f, 0.02507f),
                    localAngles = new Vector3(357.9198f, 9.64556f, 111.2866f),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 0.45f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03208f, 0.84755f, -0.15072f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.14977f, 0.01038f, -0.03227f),
                    localAngles = new Vector3(0, 0, 90f),
                    localScale = new Vector3(1, 1, 1)
                }
            });

            return rules;
        }

        public override void Hooks()
        {
            // Determines when effects are applied
            //On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChange;
            //On.RoR2.CharacterBody.OnLevelUp += OnLevelUp;
            RecalculateStatsAPI.GetStatCoefficients += DivineRingRecalculateStats;
            On.RoR2.ExperienceManager.AwardExperience += OnAwardExperience;
            //On.RoR2.CharacterMaster.GiveMoney += OnCharacterGainMoney;
            //On.RoR2.CharacterMaster.GiveExperience += OnCharacterGainExperience;
        }

        private void OnAwardExperience(On.RoR2.ExperienceManager.orig_AwardExperience orig, ExperienceManager self, Vector3 origin, CharacterBody body, ulong amount)
        {
            int inventoryCount = GetCount(body);

            if (body.level <= 0 || inventoryCount <= 0)
            {
                orig(self, origin, body, amount);
                return;
            }

            ulong newAmount = (ulong)(inventoryCount * experienceIncreasePercentPerLevel.Value * body.level);
            
            orig(self, origin, body, newAmount);
        }

        private void DivineRingRecalculateStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.isPlayerControlled)
            {
                int inventoryCount = GetCount(sender);

                if (sender.level <= 0 || inventoryCount <= 0)
                {
                    return;
                }
                
                float newHealth = inventoryCount * healthIncreasePercentPerLevel.Value * sender.level;
                args.healthMultAdd = newHealth;

                float newRegen = inventoryCount * regenerationIncreasePercentPerLevel.Value * sender.level;
                args.regenMultAdd = newRegen;

                float newDamage = inventoryCount * damageIncreasePercentPerLevel.Value * sender.level;
                args.damageMultAdd = newDamage;
            }
        }
    }
}
