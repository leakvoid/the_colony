using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Housing BT", fileName = "New Housing Building Template")]
public class HousingBT : BuildingTemplate
{
    [SerializeField] public int tier0ColonistCapacity;
    [SerializeField] public int tier1ColonistCapacity;
    [SerializeField] public int tier2ColonistCapacity;
    [SerializeField] public int tier1UpgradeWoodCost;
    [SerializeField] public int tier1UpgradeGoldCost;
    [SerializeField] public int tier2UpgradeStoneCost;
    [SerializeField] public int tier2UpgradeGoldCost;
}
