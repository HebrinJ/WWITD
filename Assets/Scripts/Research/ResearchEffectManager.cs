using UnityEngine;
using System.Collections.Generic;

public class ResearchEffectsManager : MonoBehaviour
{
    public static ResearchEffectsManager Instance { get; private set; }

    private Dictionary<string, List<ResearchEffect>> activeEffects = new Dictionary<string, List<ResearchEffect>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyResearchEffects(ResearchNodeSO researchNode)
    {
        if (researchNode.ResearchEffects == null) return;

        foreach (ResearchEffect effect in researchNode.ResearchEffects)
        {
            ApplyEffect(effect, researchNode.name);
        }
    }

    private void ApplyEffect(ResearchEffect effect, string sourceId)
    {
        // Регистрируем эффект
        if (!activeEffects.ContainsKey(sourceId))
        {
            activeEffects[sourceId] = new List<ResearchEffect>();
        }
        activeEffects[sourceId].Add(effect);

        // Применяем эффект
        switch (effect.Type)
        {
            case ResearchEffect.EffectType.TowerStatModifier:
                ApplyTowerStatModifier(effect);
                break;

            case ResearchEffect.EffectType.GlobalBonus:
                ApplyGlobalBonus(effect);
                break;

            case ResearchEffect.EffectType.UnlockTower:
                UnlockTower(effect);
                break;

            case ResearchEffect.EffectType.UnlockAbility:
                UnlockAbility(effect);
                break;
        }

        Debug.Log($"Applied effect: {effect.Type} from {sourceId}");
    }

    private void ApplyTowerStatModifier(ResearchEffect effect)
    {
        // Здесь будет логика применения модификаторов к башням
        // Пока просто логируем
        Debug.Log($"Tower {effect.TargetId} modified by {effect.Value}");
    }

    private void ApplyGlobalBonus(ResearchEffect effect)
    {
        // Глобальные бонусы (например, +10% к стартовым ресурсам)
        Debug.Log($"Global bonus applied: {effect.StringValue} = {effect.Value}");
    }

    private void UnlockTower(ResearchEffect effect)
    {
        // Разблокировка новой башни
        Debug.Log($"Unlocked tower: {effect.TargetId}");
        // Здесь нужно добавить вызов в TowerManager или BuildManager
    }

    private void UnlockAbility(ResearchEffect effect)
    {
        // Разблокировка способности
        Debug.Log($"Unlocked ability: {effect.TargetId}");
        // Здесь нужно добавить вызов в AbilityManager
    }

    public float GetTowerStatModifier(string towerType, string statName)
    {
        // Возвращает суммарный модификатор для конкретного параметра башни
        float totalModifier = 0f;

        foreach (var effects in activeEffects.Values)
        {
            foreach (ResearchEffect effect in effects)
            {
                if (effect.Type == ResearchEffect.EffectType.TowerStatModifier &&
                    effect.TargetId == towerType &&
                    effect.StringValue == statName)
                {
                    totalModifier += effect.Value;
                }
            }
        }

        return totalModifier;
    }
}
