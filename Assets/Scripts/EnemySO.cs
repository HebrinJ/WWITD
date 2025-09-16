using UnityEngine;

[CreateAssetMenu(fileName = "Data_Enemy_", menuName = "Game Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float moveSpeed;
    public int damageToBase; // Урон, который нанесет штабу, если дойдет
    public int rewardMoney; // Награда за убийство
    public GameObject prefab;
}