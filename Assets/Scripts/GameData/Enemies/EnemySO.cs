using UnityEngine;

[CreateAssetMenu(fileName = "Data_Enemy_", menuName = "Game Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;                            // ������������
    public EnemyBehaviourType enemyBehaviourType;       // ��� ���������
    public int maxHealth;                               // ���������� ��������
    public int maxArmor;                                // ���������� �����
    public DamageType damageType;                       // ��� �����
    public int damageToBase;                            // ����, ������� ������� �����, ���� ������
    public int damageToTarget;                          // ���� ��� �������� �� ����
    public float moveSpeed;                             // �������� ������������
    public float fireRate;                              // ����������������
    public float fireDistance;                          // ��������� �����
    public int rewardMoney;                             // ������� �� ��������
    public int warningRate;                             // ���������� ���������
    public GameObject prefab;                           // ������
}