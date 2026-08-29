using System;

namespace PhumFarm.Data
{
    public enum ItemCategory { Crop, AnimalProduct, CraftedGood, Material, ExpansionItem, Special }

    [Serializable]
    public sealed class OrderRequirement
    {
        public string itemId = string.Empty;
        public int minimum = 1;
        public int maximum = 1;
    }

}
