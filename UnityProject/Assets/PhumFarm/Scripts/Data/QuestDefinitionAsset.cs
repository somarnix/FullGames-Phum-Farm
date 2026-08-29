using UnityEngine;
namespace PhumFarm.Data { [CreateAssetMenu(menuName="Phum Farm/Definitions/Quest", fileName="QuestDefinition")] public sealed class QuestDefinitionAsset : ScriptableObject { public string id=string.Empty; public string displayNameKey=string.Empty; public string eventId=string.Empty; public int target=1; public int coinReward; public int xpReward; } }
