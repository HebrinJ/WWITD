using UnityEngine;

[System.Serializable]
public class ResearchEffect
{
    public enum EffectType
    {
        TowerStatModifier,   // ����������� ���������� �����
        GlobalBonus,         // ���������� �����
        UnlockTower,         // ������������� ����� �����
        UnlockAbility        // ������������� �����������
    }

    public EffectType Type;
    public string TargetId;  // ID ���� (��������, ��� �����)
    public float Value;      // �������� �������
    public string StringValue; // ��������� �������� (��� ������� ��������)
}