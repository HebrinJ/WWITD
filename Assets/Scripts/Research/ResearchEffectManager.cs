using UnityEngine;
using System.Collections.Generic;

public class ResearchEffectsManager : MonoBehaviour
{
    public static ResearchEffectsManager Instance { get; private set; }

    // Для хранения эффектов между сценами
    private Dictionary<string, List<ResearchEffect>> towerEffects = new Dictionary<string, List<ResearchEffect>>();

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
            ApplyEffect(effect);
        }
    }

    private void ApplyEffect(ResearchEffect effect)
    {
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
    }

    private void ApplyTowerStatModifier(ResearchEffect effect)
    {
        // Формат: "TowerType_StatName_ModifierType" например "Riflemen_Damage_Flat"
        string[] parts = effect.TargetId.Split('_');
        if (parts.Length != 3)
        {
            Debug.LogError($"Invalid target format: {effect.TargetId}. Expected: TowerType_StatName_ModifierType");
            return;
        }

        string towerType = parts[0];
        string statName = parts[1];
        string modifierType = parts[2];

        // Сохраняем эффект для применения на тактических уровнях
        string effectKey = $"{towerType}_{statName}";
        if (!towerEffects.ContainsKey(effectKey))
        {
            towerEffects[effectKey] = new List<ResearchEffect>();
        }
        towerEffects[effectKey].Add(effect);

        Debug.Log($"Research effect saved: {effect.TargetId} = {effect.Value}");
        Debug.Log($"Total effects for {effectKey}: {towerEffects[effectKey].Count}");
    }

    // Остальные методы остаются без изменений
    private void ApplyGlobalBonus(ResearchEffect effect) { /* ... */ }
    private void UnlockTower(ResearchEffect effect) { /* ... */ }
    private void UnlockAbility(ResearchEffect effect) { /* ... */ }

    // ГЛАВНЫЙ МЕТОД: Применяем все исследования к башне при её создании
    public void ApplyResearchEffectsToTower(TowerBehaviour tower)
    {
        if (tower == null) return;

        string towerType = tower.GetTowerType().ToString();
        Debug.Log($"Applying research effects to tower: {towerType}");

        int appliedEffectsCount = 0;

        foreach (var effectEntry in towerEffects)
        {
            string[] keyParts = effectEntry.Key.Split('_');
            if (keyParts.Length == 2 && keyParts[0] == towerType)
            {
                string statName = keyParts[1];

                foreach (ResearchEffect effect in effectEntry.Value)
                {
                    string[] effectParts = effect.TargetId.Split('_');
                    if (effectParts.Length == 3)
                    {
                        string modifierType = effectParts[2];
                        ApplyEffectToTower(tower, statName, modifierType, effect.Value);
                        appliedEffectsCount++;
                    }
                }
            }
        }

        Debug.Log($"Applied {appliedEffectsCount} research effects to {towerType}");
        tower.LogCurrentStats();
    }

    private void ApplyEffectToTower(TowerBehaviour tower, string statName, string modifierType, float value)
    {
        switch (modifierType.ToLower())
        {
            case "flat":
                tower.ApplyFlatBonus(statName, Mathf.RoundToInt(value));
                break;

            case "mult":
                tower.ApplyMultiplicativeBonus(statName, value);
                break;

            default:
                Debug.LogError($"Unknown modifier type: {modifierType}");
                break;
        }
    }

    // Метод для отладки: показать все сохраненные эффекты
    public void DebugPrintAllEffects()
    {
        Debug.Log("=== ALL SAVED RESEARCH EFFECTS ===");
        foreach (var entry in towerEffects)
        {
            Debug.Log($"Key: {entry.Key}, Effects: {entry.Value.Count}");
            foreach (var effect in entry.Value)
            {
                Debug.Log($"  - {effect.TargetId} = {effect.Value}");
            }
        }
    }
}