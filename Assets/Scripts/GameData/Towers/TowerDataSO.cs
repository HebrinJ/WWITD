using UnityEngine;

[CreateAssetMenu(fileName = "Data_Tower_", menuName = "Game Data/Tower Data")]
public class TowerDataSO : ScriptableObject
{
    public string towerName;
    public TowerType towerType;
    public TowerCost[] constructionCost;
    public int baseDamage;
    public float baseRange;
    public float attacksPerSecond;
    public GameObject towerPrefab; // Префаб самой башни
    public GameObject projectilePrefab; // Префаб снаряда (если башня стреляет)
}
