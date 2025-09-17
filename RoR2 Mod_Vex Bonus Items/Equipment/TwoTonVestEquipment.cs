using BepInEx.Configuration;
using EntityStates;
using R2API;
using Rewired.Utils;
using RoR2;
using RoR2_Mod_Vex_Bonus_Items.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2_Mod_Vex_Bonus_Items.Main;

namespace RoR2_Mod_Vex_Bonus_Items.Equipment
{
    public class TwoTonVestEquipment : EquipmentBase<TwoTonVestEquipment>
    {
        public ConfigEntry<float> deathRadius;
        public ConfigEntry<float> healthPercentLostPerDeath;
        public ConfigEntry<float> greenItemPercentChance;
        public ConfigEntry<float> redItemPercentChance;
        public ConfigEntry<float> equipmentCooldown;

        public Transform ringIndicatorInstance;
        public bool isRingUpdating = false;
        public CharacterBody owner;

        public GameObject explosionEffect => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ExplosionVFX.prefab").WaitForCompletion();

        public RoR2.NetworkSoundEventDef explosionSound => Addressables.LoadAssetAsync<RoR2.NetworkSoundEventDef>("RoR2/DLC2/Child/nseTrackingProjectileExplosion.asset").WaitForCompletion();

        public override string EquipmentName => "Two Ton Vest";

        public override string EquipmentLangTokenName => "TWO_TON_VEST";

        public override string EquipmentPickupDesc => "Turn nearby enemies into items at the cost of health.";

        public override string EquipmentFullDescription => $"Instantly kill all non-boss enemies within a <style=cIsUtility>{deathRadius.Value}m radius</style>. Each enemy killed drops a random item. Lose <style=cIsHealth>{healthPercentLostPerDeath.Value * 100}% health</style> for each enemy killed. This disregards armor, barrier, and one shot protection, but will leave you with at least <style=cIsHealth>1 hp</style>.";

        public override string EquipmentLore => "Henry picked up the explosive device and found a delicately handwritten note beneath it.\r\n\n\"Beads of sweat leapt from her face, her hands, but they fell into a pool of resolve.\r\nIt must be done. She tells herself she is worthy, her sacrifice makes beauty.\r\nShe releases a blinding light, and the pillars collapse.\"\r\n\nHenry stares in disbelief at the note. \"Is this meant to be a poem or instructions?\"";

        public override float Cooldown => equipmentCooldown.Value;

        public override bool IsLunar => true;

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("TwoTonVestPickup.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("TwoTonVestIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            SetLogbookAppearance(.5f, 1.5f);
            CreateEquipment();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
            deathRadius = config.Bind<float>("Equipment: " + EquipmentName, "Range at which enemies can be killed", 20f, "How far away should enemies be killed?");
            healthPercentLostPerDeath = config.Bind<float>("Equipment: " + EquipmentName, "Percent of health lost per enemy killed", 0.3f, "How much health should be lost per enemy killed?");
            greenItemPercentChance = config.Bind<float>("Equipment: " + EquipmentName, "Chance for an enemy to drop a green item", 0.395f, "How often should green items drop?");
            redItemPercentChance = config.Bind<float>("Equipment: " + EquipmentName, "Chance for an enemy to drop a red item", 0.01f, "How often should red items drop?");
            equipmentCooldown = config.Bind<float>("Equipment: " + EquipmentName, "Delay before the equipment can be used again", 120f, "How long should the equipment cooldown be?");
            redItemPercentChance.Value = 0.01f;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            GameObject ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("TwoTonVestDisplay.prefab");
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
                    localPos = new Vector3(0.00013F, 0.17418F, 0.24083F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.05505F, 0.14805F, 0.24934F),
                    localAngles = new Vector3(3.49064F, 82.7725F, 345.617F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.10873F, 0.20101F, 0.21099F),
                    localAngles = new Vector3(0F, 70F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.04527F, 1.21698F, 3.78372F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(5.6F, 5.6F, 5.6F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.002F, 0.21774F, 0.30556F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00428F, 0.10987F, 0.16369F),
                    localAngles = new Vector3(0F, 86.99999F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00425F, 0.0947F, 0.25114F),
                    localAngles = new Vector3(0F, 92F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(0F, 0.41166F, 0.89927F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(2.8F, 2.8F, 2.8F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00134F, 0.14757F, 0.21002F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02163F, 1.81186F, -2.18983F),
                    localAngles = new Vector3(0.26926F, 90.02594F, 3.85986F),
                    localScale = new Vector3(5.6F, 5.6F, 5.6F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.03622F, 0.153F, 0.22723F),
                    localAngles = new Vector3(0F, 99.99999F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.41607F, 0.64352F, 0.07227F),
                    localAngles = new Vector3(8.27927F, 8.62889F, 121.0982F),
                    localScale = new Vector3(2.1F, 2.1F, 2.1F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.10221F, 0.184F, 0.19515F),
                    localAngles = new Vector3(5.21899F, 57.91243F, 322.5683F),
                    localScale = new Vector3(1.12F, 1.12F, 1.12F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.02254F, 0.01921F, 0.2043F),
                    localAngles = new Vector3(0F, 80F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00392F, 0.0951F, 0.12247F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.12528F, 0.25199F, 0.33837F),
                    localAngles = new Vector3(0.4305F, 103.4209F, 349.0872F),
                    localScale = new Vector3(1.4f, 1.4f, 1.4f)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.06688F, 0.20803F, 0.0382F),
                    localAngles = new Vector3(0F, 0F, 90F),
                    localScale = new Vector3(1.12F, 1.12F, 1.12F)
                }
            });

            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += OnEquipmentLost;
            On.RoR2.CharacterBody.Update += UpdateVisualAid;
        }

        private void UpdateVisualAid(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self)
        {
            if (!isRingUpdating || !owner)
            {
                return;
            }

            UpdateRingIndicator();

            orig(self);
        }

        private void OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                // Make the ring appear so the player sees the range of the equipment
                if (self.isPlayerControlled)
                {
                    owner = self;
                }
                
                isRingUpdating = true;
            }

            orig(self, equipmentDef);
        }

        private void OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            // Remove the ring effect if it exists
            if (ringIndicatorInstance)
            {
                isRingUpdating = false;
                EntityState.Destroy(ringIndicatorInstance.gameObject);
            }

            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            Vector3 origin = slot.characterBody.corePosition;

            SphereSearch ss = new SphereSearch()
            {
                origin = origin,
                radius = deathRadius.Value,
                mask = LayerIndex.entityPrecise.mask
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities();
            
            HurtBox[] hurtBoxes = ss.GetHurtBoxes();
            ModLogger.LogInfo($"Hurt Boxes Detected: {hurtBoxes.Length}");
            int enemiesKilled = 0;

            foreach (HurtBox hurtBox in hurtBoxes)
            {
                if (hurtBox.healthComponent.body != null)
                {
                    CharacterBody characterBody = hurtBox.healthComponent.body;
                    
                    // Don't do anything if the character body is on team Player or Neutral
                    if (characterBody != null &&
                        characterBody.teamComponent != null &&
                        characterBody.teamComponent.teamIndex == TeamIndex.Player ||
                        characterBody.teamComponent.teamIndex == TeamIndex.Neutral)
                    {
                        ModLogger.LogInfo("This character body won't be affected.");
                    }
                    // Only spawn an item and kill the character if it's an enemy,
                    // not a boss, and the enemy isn't already dead
                    else if (characterBody != null &&
                        characterBody.healthComponent != null &&
                        characterBody.healthComponent.health > 0 &&
                        !characterBody.isBoss &&
                        characterBody.teamComponent != null &&
                        characterBody.teamComponent.teamIndex == TeamIndex.Monster ||
                        characterBody.teamComponent.teamIndex == TeamIndex.Void ||
                        characterBody.teamComponent.teamIndex == TeamIndex.Lunar)
                    {
                        // Try killing the enemy
                        DamageInfo enemyDamageInfo = new DamageInfo
                        {
                            inflictor = null,
                            attacker = null,
                            procCoefficient = 0f,
                            crit = false,
                            position = characterBody.corePosition,
                            force = Vector3.zero,
                            canRejectForce = true,
                            rejected = false,
                            damageColorIndex = DamageColorIndex.Default,
                            damageType = DamageType.BypassArmor,
                            dotIndex = DotController.DotIndex.None,
                            delayedDamageSecondHalf = false,
                            damage = characterBody.maxHealth
                        };

                        // Track only the enemies killed
                        enemiesKilled++;

                        Vector3 heightOffset = new Vector3(0, 2, 0);
                        Vector3 velocity = new Vector3(0, 1, 0);

                        // Spawn an item where that enemy is
                        PickupIndex pickupIndex = GetRandomItem();
                        PickupDropletController.CreatePickupDroplet(pickupIndex, characterBody.transform.position + heightOffset, velocity);

                        // Kill the enemy
                        characterBody.healthComponent.TakeDamage(enemyDamageInfo);
                    }
                }
            }

            if (enemiesKilled <= 0)
            {
                var message = "<color=#E48D2D>It seems to be a dud.</color>";
                RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage { baseToken = message });
                return true;
            }

            // Get the user's health component
            HealthComponent healthComponent = slot.characterBody.healthComponent;

            // Damage the user for each enemy killed
            float healthPercentLost = healthPercentLostPerDeath.Value * (float)enemiesKilled;
            ModLogger.LogInfo("Health Percent Lost: " + healthPercentLost * 100 + "%");

            // Calculate total damage based on user's full health + full shield + any barrier
            float totalDamage = (healthComponent.fullCombinedHealth + healthComponent.barrier) * healthPercentLost;
            ModLogger.LogInfo("Total Self Damage: " + totalDamage);

            // Get the user's combined available health
            float totalHealth = healthComponent.combinedHealth;
            ModLogger.LogInfo("Total Available Health: " + totalHealth);

            // If the damage dealt is more than the user's available health pool, leave them with 1 hp
            if (totalDamage >= totalHealth)
            {
                totalDamage = totalHealth - 0.5f;
            }

            ModLogger.LogInfo("Capped Damage: " + totalDamage);

            // Make the user lose a % of their health
            DamageInfo selfDamageInfo = new DamageInfo
            {
                inflictor = null,
                attacker = null,
                procCoefficient = 0f,
                crit = false,
                position = slot.characterBody.corePosition,
                force = Vector3.zero,
                canRejectForce = true,
                rejected = false,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.BypassArmor,
                dotIndex = DotController.DotIndex.None,
                delayedDamageSecondHalf = false,
                damage = totalDamage
            };
            // Hopefully this makes the damage bypass armor, block, and one shot protection
            selfDamageInfo.damageType |= DamageType.BypassBlock;
            selfDamageInfo.damageType |= DamageType.BypassOneShotProtection;

            healthComponent.TakeDamage(selfDamageInfo);

            // Spawn an explosion visual effect
            EffectData effectData = new EffectData
            {
                color = Color.red,
                origin = origin,
                scale = 4,
                rotation = Quaternion.identity
            };
            
            if (explosionEffect != null)
            {
                EffectManager.SpawnEffect(explosionEffect, effectData, false);
            }
            if (explosionSound != null)
            {
                Util.PlaySound(explosionSound.eventName, slot.gameObject);
            }
            
            return true;
        }
        
        private PickupIndex GetRandomItem()
        {
            // Roll for red item
            if ((float)UnityEngine.Random.Range(0, 101) / 100f <= redItemPercentChance.Value)
            {
                // Get the list of all red items
                List<PickupIndex> dropList = Run.instance.availableTier3DropList;
                // Get a random red item
                int nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                ModLogger.LogInfo($"Number of available red items: {dropList.Count}. Next Item Index: {nextItem}");
                // Return the pickup index of that red item
                return dropList[nextItem];
            }
            // Roll for green item
            else if ((float)UnityEngine.Random.Range(0, 101) / 100f <= greenItemPercentChance.Value)
            {
                // Get the list of all green items
                List<PickupIndex> dropList = Run.instance.availableTier2DropList;
                // Get a random green item
                int nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                ModLogger.LogInfo($"Number of available green items: {dropList.Count}. Next Item Index: {nextItem}");
                // Return the pickup index of that green item
                return dropList[nextItem];
            }
            // Fallback item is white
            else
            {
                // Get the list of all white items
                List<PickupIndex> dropList = Run.instance.availableTier1DropList;
                // Get a random white item
                int nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                ModLogger.LogInfo($"Number of available white items: {dropList.Count}. Next Item Index: {nextItem}");
                // Return the pickup index of that white item
                return dropList[nextItem];
            }
        }

        private void CreateRingIndicator()
        {
            if (!owner)
            {
                return;
            }

            ringIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
            ringIndicatorInstance.localScale = Vector3.one * deathRadius.Value;
            ringIndicatorInstance.transform.position = owner.corePosition;
        }

        private void UpdateRingIndicator()
        {
            if (!ringIndicatorInstance)
            {
                CreateRingIndicator();
            }

            if (!owner)
            {
                return;
            }

            ringIndicatorInstance.transform.position = owner.corePosition;
        }
    }
}
