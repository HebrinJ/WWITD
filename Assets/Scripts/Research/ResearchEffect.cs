using UnityEngine;

[System.Serializable]
public class ResearchEffect
{
    public enum EffectType
    {
        TowerStatModifier,   // Модификатор параметров башни
        GlobalBonus,         // Глобальный бонус
        UnlockTower,         // Разблокировка новой башни
        UnlockAbility        // Разблокировка способности
    }

    public EffectType Type;
    public string TargetId;  // ID цели (например, тип башни)
    public float Value;      // Значение эффекта
    public string StringValue; // Строковое значение (для сложных эффектов)
}