using UnityEngine;

[CreateAssetMenu(fileName = "Data_Enemy_", menuName = "Game Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;                            // наименование
    public EnemyBehaviourType enemyBehaviourType;       // Тип поведения
    public int maxHealth;                               // Количество здоровья
    public int maxArmor;                                // Количество брони
    public DamageType damageType;                       // Тип урона
    public int damageToBase;                            // Урон, который нанесет штабу, если дойдет
    public int damageToTarget;                          // Урон при стрельбе по цели
    public float moveSpeed;                             // Скорость передвижения
    public float fireRate;                              // Скорострельность
    public float fireDistance;                          // Дальность атаки
    public int rewardMoney;                             // Награда за убийство
    public int warningRate;                             // Показатель опасности
    public GameObject prefab;                           // Префаб
}