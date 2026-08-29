using PhumFarm.Core;
using PhumFarm.Data;
using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class ExpansionDistrict : FarmDistrict
    {
        private static readonly Vector3[] Positions =
        {
            new(-34, 0, 5), new(4, 0, 27), new(35, 0, -20)
        };

        public override void Build(FarmWorldController world)
        {
            ExpansionDefinition[] values = GameServices.Instance.Balance.expansions;
            for (int i = 0; i < values.Length && i < Positions.Length; i++)
            {
                ExpansionDefinition expansion = values[i];
                if (GameServices.Instance.Unlocks.Expansion(expansion.id)) continue;
                Vector3 position = Positions[i];
                GameObject marker = WorldPrimitiveFactory.Cylinder(transform, $"{expansion.id}_marker", position + Vector3.up * .18f, new Vector3(2.5f, .18f, 2.5f), WorldPrimitiveFactory.Hex("D7B64D"));
                marker.transform.localScale = new Vector3(2.5f, .18f, 2.5f);
                world.AddStation(transform, StationKind.Expansion, expansion.id, expansion.id.Replace('_', ' '), position, expansion.requiredLevel);
            }
        }
    }
}
