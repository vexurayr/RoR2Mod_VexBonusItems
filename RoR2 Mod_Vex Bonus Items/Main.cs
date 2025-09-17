using BepInEx;
using BepInEx.Configuration;
using On.RoR2.Items;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2_Mod_Vex_Bonus_Items.Artifact;
using RoR2_Mod_Vex_Bonus_Items.Equipment;
using RoR2_Mod_Vex_Bonus_Items.Equipment.EliteEquipment;
using RoR2_Mod_Vex_Bonus_Items.Items;
using RoR2_Mod_Vex_Bonus_Items.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace RoR2_Mod_Vex_Bonus_Items
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(EliteAPI), nameof(PrefabAPI), nameof(DamageAPI))]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.Vexurayr.VexBonusItems";
        public const string ModName = "Vex Bonus Items";
        public const string ModVer = "1.0.0";
        
        public static AssetBundle MainAssets;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbed hopoo games/deferred/standard", "shaders/deferred/hgstandard"},
            {"stubbed hopoo games/fx/cloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"stubbed hopoo games/fx/cloud remap", "shaders/fx/hgcloudremap" },
            {"stubbed hopoo games/fx/distortion", "shaders/fx/hgdistortion" },
            {"stubbed hopoo games/deferred/snow topped", "shaders/deferred/hgsnowtopped" },
            {"stubbed hopoo games/fx/solid parallax", "shaders/fx/hgsolidparallax" }
        };

        //Provides a direct access to this plugin's logger for use in any of your other classes.
        public static BepInEx.Logging.ManualLogSource ModLogger;

        public static ExpansionDef sotvDLC;

        public static ConfigEntry<bool> lockVoidsBehindPair;
        public static ConfigEntry<bool> doVoidPickupBorders;
        public static ConfigEntry<bool> doVoidCommandVFX;
        public static ConfigEntry<bool> doSaleCradle;

        GameObject tier1Clone;
        GameObject tier2Clone;
        GameObject tier3Clone;
        GameObject tier4Clone;
        bool hasAdjustedTiers;
        bool hasAddedCommand;

        private void Awake()
        {
            lockVoidsBehindPair = Config.Bind<bool>("Tweaks: Void Items", "Require Original Item Unlocked", true, "If enabled, makes it so void items are locked until the non-void pair is unlocked. Ex. Pluripotent is locked until the profile has unlocked Dios. Only applies to void items which do not already have unlocks, in the event a mod adds special unlocks for a void item.");
            doVoidPickupBorders = Config.Bind<bool>("Tweaks: Void Items", "Improved Pickup Highlights", true, "If enabled, picking up a void item will show tier-appropriate item highlights rather the the default white highlights.");
            doVoidCommandVFX = Config.Bind<bool>("Tweaks: Void Items", "Improved Command VFX", true, "If enabled, void command cubes will have appropriate void vfx in the style of typical command VFX based on the actual void item VFX.");
            doSaleCradle = Config.Bind<bool>("Tweaks: Void Cradles", "Sale Star Functionality", true, "If enabled, Sale Star will work on Void Cradles.");

            ModLogger = Logger;

            sotvDLC = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            // Don't know how to create/use an asset bundle, or don't have a unity project set up?
            // Look here for info on how to set these up: https://github.com/KomradeSpectre/AetheriumMod/blob/rewrite-master/Tutorials/Item%20Mod%20Creation.md#unity-project
            // (This is a bit old now, but the information on setting the unity asset bundle should be the same.)

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoR2_Mod_Vex_Bonus_Items.vexbonusitemsassets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            ShaderConversion(MainAssets);

            On.RoR2.Items.ContagiousItemManager.Init += AddVoidItemsToDict;
            On.RoR2.ItemCatalog.Init += AddUnlocksToVoidItems;

            On.RoR2.Language.GetLocalizedStringByToken += (orig, self, token) => {
                if (ItemBase.TokenToVoidPair.ContainsKey(token))
                {
                    ItemIndex idx = ItemCatalog.FindItemIndex(ItemBase.TokenToVoidPair[token]);
                    if (idx != ItemIndex.None) return orig(self, token).Replace("{CORRUPTION}", MiscUtils.GetPlural(orig(self, ItemCatalog.GetItemDef(idx).nameToken)));
                }
                return orig(self, token);
            };

            SetupVoidTierHighlights();

            if (doSaleCradle.Value)
            {
                var cradle = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidChest/VoidChest.prefab").WaitForCompletion();
                if (cradle)
                {
                    var pi = cradle.GetComponent<PurchaseInteraction>();
                    if (pi)
                    {
                        pi.saleStarCompatible = true;
                        On.RoR2.PurchaseInteraction.OnInteractionBegin += ImproveStarCradle;
                    }
                }
            }

            //This section automatically scans the project for all artifacts
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }

            //This section automatically scans the project for all items
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>();

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);

                    var tags = item.ItemTags;
                    bool aiValid = true;
                    bool aiBlacklist = false;
                    if (item.ItemDef.deprecatedTier == ItemTier.NoTier)
                    {
                        aiBlacklist = true;
                        aiValid = false;
                    }
                    string name = item.ItemName;
                    name = name.Replace("'", "");

                    foreach (var tag in tags)
                    {
                        if (tag == ItemTag.AIBlacklist)
                        {
                            aiBlacklist = true;
                            aiValid = false;
                            break;
                        }
                    }
                    if (aiValid)
                    {
                        aiBlacklist = Config.Bind<bool>("Item: " + name, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
                    }
                    else
                    {
                        aiBlacklist = true;
                    }

                    if (aiBlacklist)
                    {
                        item.AIBlacklisted = true;
                    }
                }
            }

            //this section automatically scans the project for all equipment
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }

            //this section automatically scans the project for all elite equipment
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);

                }
            }


        }

        private void ImproveStarCradle(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.displayNameToken == "VOID_CHEST_NAME")
            {
                var count = activator.GetComponent<CharacterBody>().inventory.GetItemCount(DLC2Content.Items.LowerPricedChests);
                var chestb = self.GetComponent<ChestBehavior>();
                if (chestb && count > 0)
                {
                    chestb.dropForwardVelocityStrength = 6;
                }
            }

            orig(self, activator);
        }

        public static void ShaderConversion(AssetBundle assets)
        {
            var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.StartsWith("Stubbed"));

            foreach (Material material in materialAssets)
            {
                var replacementShader = LegacyResourcesAPI.Load<Shader>(ShaderLookup[material.shader.name.ToLowerInvariant()]);
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }
        }

        private void AddUnlocksToVoidItems(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            if (lockVoidsBehindPair.Value)
            {
                foreach (var voidpair in ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem])
                {
                    if (voidpair.itemDef1.unlockableDef != null && voidpair.itemDef2.unlockableDef == null)
                    {
                        Debug.Log("Updating unlock condition for " + voidpair.itemDef2.nameToken + " to " + voidpair.itemDef1.nameToken + "'s.");
                        voidpair.itemDef2.unlockableDef = voidpair.itemDef1.unlockableDef;
                    }
                }
            }
        }

        private void AddVoidItemsToDict(ContagiousItemManager.orig_Init orig)
        {
            List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>();
            Debug.Log("Adding VanillaVoid item transformations...");
            foreach (var item in Items)
            {
                if (item.ItemDef.deprecatedTier != ItemTier.NoTier)
                {
                    Debug.Log("Item Name: " + item.ItemName);
                    item.AddVoidPair(newVoidPairs);
                }
                else
                {
                    Debug.Log("Skipping " + item.ItemName);
                }
            }
            var key = DLC1Content.ItemRelationshipTypes.ContagiousItem;
            Debug.Log(key);
            var voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(newVoidPairs).ToArray();
            Debug.Log("Finishing appending VanillaVoid item transformations.");

            orig();
        }

        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class."</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class."</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class."</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        /// <summary>
        /// A helper to easily set up and initialize an elite equipment from your elite equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="eliteEquipment">A new instance of an EliteEquipmentBase class.</param>
        /// <param name="eliteEquipmentList">The list you would like to add this to if it passes the config check.</param>
        /// <returns></returns>
        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public void SetupVoidTierHighlights()
        {
            GameObject tier1prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            tier1prefab.AddComponent<NetworkIdentity>();
            tier1Clone = PrefabAPI.InstantiateClone(tier1prefab, "void1HighlightPrefab");
            var rect1 = tier1Clone.GetComponent<RoR2.UI.HighlightRect>();
            if (rect1)
            {
                rect1.highlightColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem);
                rect1.cornerImage = MainAssets.LoadAsset<Sprite>("texUICornerTier1");
            }

            //GameObject tier2prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            tier2Clone = PrefabAPI.InstantiateClone(tier1prefab, "void2HighlightPrefab");
            var rect2 = tier2Clone.GetComponent<RoR2.UI.HighlightRect>();
            if (rect2)
            {
                rect2.highlightColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem);
                rect2.cornerImage = MainAssets.LoadAsset<Sprite>("texUICornerTier2");
            }

            tier3Clone = PrefabAPI.InstantiateClone(tier1prefab, "void3HighlightPrefab");
            var rect3 = tier3Clone.GetComponent<RoR2.UI.HighlightRect>();
            if (rect3)
            {
                rect3.highlightColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem);
                rect3.cornerImage = MainAssets.LoadAsset<Sprite>("texUICornerTier3");
            }

            tier4Clone = PrefabAPI.InstantiateClone(tier1prefab, "void4HighlightPrefab");
            var rect4 = tier4Clone.GetComponent<RoR2.UI.HighlightRect>();
            if (rect4)
            {
                rect4.highlightColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem);
                rect4.cornerImage = MainAssets.LoadAsset<Sprite>("texUICornerTier1");
            }

            hasAdjustedTiers = false;
            if (!doVoidCommandVFX.Value)
            {
                hasAddedCommand = true;
            }
            else
            {
                hasAddedCommand = false;
            }
        }

        public void ApplyTierHighlights()
        {
            if (!hasAdjustedTiers && doVoidPickupBorders.Value)
            {
                var voidtier1def = ItemTierCatalog.GetItemTierDef(ItemTier.VoidTier1);
                if (voidtier1def)
                {
                    voidtier1def.highlightPrefab = tier1Clone;
                }
                var voidtier2def = ItemTierCatalog.GetItemTierDef(ItemTier.VoidTier2);
                if (voidtier2def)
                {
                    voidtier2def.highlightPrefab = tier2Clone;
                }
                var voidtier3def = ItemTierCatalog.GetItemTierDef(ItemTier.VoidTier3);
                if (voidtier3def)
                {
                    voidtier3def.highlightPrefab = tier3Clone;
                }
                var voidtier4def = ItemTierCatalog.GetItemTierDef(ItemTier.VoidBoss);
                if (voidtier4def)
                {
                    voidtier4def.highlightPrefab = tier4Clone;
                }

                hasAdjustedTiers = true;
            }
        }

        private void Update()
        {
            DebugSpawnItems();
        }

        private void DebugSpawnItems()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // Get the player body to use a position
                var transform = RoR2.PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                ModLogger.LogInfo($"Player pressed F1. Spawning item at coordinates {transform.position}");

                // Drop the item in front of the player
                RoR2.PickupDropletController.CreatePickupDroplet(RoR2.PickupCatalog.FindPickupIndex(ItemCatalog.FindItemIndex("ITEM_ADAPTIVE_GOGGLES")), transform.position, transform.forward * 20f);
                RoR2.PickupDropletController.CreatePickupDroplet(RoR2.PickupCatalog.FindPickupIndex(ItemCatalog.FindItemIndex("Talisman")), transform.position, transform.forward * -20f);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player body to use a position
                var transform = RoR2.PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                ModLogger.LogInfo($"Player pressed F2. Spawning item at coordinates {transform.position}");

                // Drop the item in front of the player
                RoR2.PickupDropletController.CreatePickupDroplet(RoR2.PickupCatalog.FindPickupIndex(ItemCatalog.FindItemIndex("ITEM_DIVINE_RING")), transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                // Get the player body to use a position
                var transform = RoR2.PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                ModLogger.LogInfo($"Player pressed F3. Spawning item at coordinates {transform.position}");

                // Drop the item in front of the player
                RoR2.PickupDropletController.CreatePickupDroplet(RoR2.PickupCatalog.FindPickupIndex(ItemCatalog.FindItemIndex("ITEM_SWORD_OF_LOOTING")), transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                // Get the player body to use a position
                var transform = RoR2.PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                ModLogger.LogInfo($"Player pressed F4. Spawning equipment at coordinates {transform.position}");

                // Drop the equipment in front of the player
                RoR2.PickupDropletController.CreatePickupDroplet(RoR2.PickupCatalog.FindPickupIndex(EquipmentCatalog.FindEquipmentIndex("EQUIPMENT_TWO_TON_VEST")), transform.position, transform.forward * 20f);
            }
        }
    }
}
