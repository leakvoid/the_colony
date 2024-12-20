using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Housing BT", fileName = "New Housing Building Template")]
public class HousingBT : BuildingTemplate
{
    [field: SerializeField] public int Tier0ColonistCapacity { get; private set; }
    [field: SerializeField] public int Tier1ColonistCapacity { get; private set; }
    [field: SerializeField] public int Tier2ColonistCapacity { get; private set; }
    [field: SerializeField] public int Tier1UpgradeWoodCost { get; private set; }
    [field: SerializeField] public int Tier1UpgradeGoldCost { get; private set; }
    [field: SerializeField] public int Tier2UpgradeStoneCost { get; private set; }
    [field: SerializeField] public int Tier2UpgradeGoldCost { get; private set; }
    [field: SerializeField] public GameObject Tier1ModelPrefab { get; private set; }
    [field: SerializeField] public GameObject Tier2ModelPrefab { get; private set; }

}
