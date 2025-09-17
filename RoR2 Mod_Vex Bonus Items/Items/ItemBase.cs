using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace RoR2_Mod_Vex_Bonus_Items.Items
{
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to implement instancing for classes.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/
    // Important Colors for Stroke in Photopea- (size 4, size 10)
    // White Item: (Inside) #D4D9DE , (Outside) #DDE9E9
    // Green Item: (Inside) #5CA13C , (Outside) #75BF41
    // Red Item: (Inside) #EE5227 , (Outside) #EE5B47
    // Yellow Item: (Inside) #B2BA2A , (Outside) #C0D52A
    // Blue Item: (Inside) #2F7C92 , (Outside) #43DAF3
    // Purple Item: (Inside) #AC1948 , (Outside) #FA8CE1
    // Equipment: (Inside) #E48D2D , (Outside) #EF9E33

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public static Dictionary<string, string> TokenToVoidPair = new Dictionary<string, string>();
        public ConfigEntry<string> voidPair = null;

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; set; } = false;

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>SetLogbookAppearance();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public abstract void Init(ConfigFile config);

        public virtual void CreateConfig(ConfigFile config) { }

        protected virtual void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
            if (voidPair != null)
            {
                TokenToVoidPair.Add("ITEM_" + ItemLangTokenName + "_PICKUP", voidPair.Value);
                TokenToVoidPair.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", voidPair.Value);
            }
        }

        public virtual string VoidPair()
        {
            if (voidPair != null)
            {
                return voidPair.Value;
            }
            else
            {
                return null;
            }
        }

        public void AddVoidPair(List<ItemDef.Pair> newVoidPairs)
        {
            string pair = VoidPair();
            if (pair != null)
            {
                var pairDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(pair));
                if (pairDef != null)
                {
                    ItemDef.Pair newVoidPair = new ItemDef.Pair()
                    {
                        itemDef1 = pairDef,
                        itemDef2 = ItemDef
                    };
                    newVoidPairs.Add(newVoidPair);
                }
            }
        }

        protected virtual void SetLogbookAppearance(float minDistance = 1f, float maxDistance = 2f)
        {
            if (!ItemModel)
            {
                return;
            }
            
            ModelPanelParameters mpp = ItemModel.AddComponent<ModelPanelParameters>();

            mpp.minDistance = minDistance;
            mpp.maxDistance = maxDistance;
            mpp.modelRotation = Quaternion.identity;

            mpp.focusPointTransform = new GameObject("FocusPoint").transform;
            mpp.focusPointTransform.SetParent(ItemModel.transform);

            mpp.cameraPositionTransform = new GameObject("CameraPosition").transform;
            mpp.cameraPositionTransform.SetParent(ItemModel.transform);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
            ItemDef.deprecatedTier = Tier;
            ItemDef.tier = Tier;

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        public virtual void Hooks() { }

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemDef);
        }
    }
}
