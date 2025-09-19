using UnityEngine;

[CreateAssetMenu(fileName = "Data_Tower_", menuName = "Game Data/Tower Data")]
public class TowerDataSO : ScriptableObject
{
    public string towerName;                            // Название башни
    public TowerType towerType;                         // Тип башни
    public TowerCost[] constructionCost;                // Стоиомтьс постройки
    public int maxHealth;                               // Количество здоровья
    public int maxArmor;                                // Количество брони
    public DamageType damageType;                       // Тип урона
    public int damage;                                  // Урон по цели
    public float fireDistance;                          // Дальность стрельбы
    public float fireRate;                              // Скорострельность
    public GameObject towerPrefab;                      // Префаб самой башни
    public GameObject projectilePrefab;                 // Префаб снаряда
}
