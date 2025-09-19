using UnityEngine;

[CreateAssetMenu(fileName = "Data_Tower_", menuName = "Game Data/Tower Data")]
public class TowerDataSO : ScriptableObject
{
    public string towerName;                            // �������� �����
    public TowerType towerType;                         // ��� �����
    public TowerCost[] constructionCost;                // ��������� ���������
    public int maxHealth;                               // ���������� ��������
    public int maxArmor;                                // ���������� �����
    public DamageType damageType;                       // ��� �����
    public int damage;                                  // ���� �� ����
    public float fireDistance;                          // ��������� ��������
    public float fireRate;                              // ����������������
    public GameObject towerPrefab;                      // ������ ����� �����
    public GameObject projectilePrefab;                 // ������ �������
}
