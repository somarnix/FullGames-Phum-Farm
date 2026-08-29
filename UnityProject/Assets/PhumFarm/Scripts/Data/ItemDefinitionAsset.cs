using UnityEngine;
namespace PhumFarm.Data { [CreateAssetMenu(menuName="Phum Farm/Definitions/Item", fileName="ItemDefinition")] public sealed class ItemDefinitionAsset : ScriptableObject { public string id=string.Empty; public string displayNameKey=string.Empty; public ItemCategory category; public Sprite icon; public int sellPrice; } }
