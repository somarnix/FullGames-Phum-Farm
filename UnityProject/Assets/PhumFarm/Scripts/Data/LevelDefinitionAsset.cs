using System; using UnityEngine;
namespace PhumFarm.Data { [CreateAssetMenu(menuName="Phum Farm/Definitions/Level", fileName="LevelDefinition")] public sealed class LevelDefinitionAsset : ScriptableObject { public int level=1; public int xpThreshold; public int coinReward=50; public string[] unlockIds=Array.Empty<string>(); } }
