using UnityEngine;
namespace PhumFarm.Data { [CreateAssetMenu(menuName="Phum Farm/Definitions/Expansion", fileName="ExpansionDefinition")] public sealed class ExpansionDefinitionAsset : ScriptableObject { public string id=string.Empty; public string displayNameKey=string.Empty; public int requiredLevel=1; public int coinCost; public Bounds buildBounds; } }
