using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер применения эффектов исследований к игровым системам.
/// Отвечает за применение модификаторов к башням, разблокировку контента и глобальных бонусов.
/// Сохраняет состояние эффектов между сценами (DontDestroyOnLoad) для применения на тактических уровнях.
/// </summary>
public class ResearchEffectsManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру ResearchEffectsManager.
    /// Гарантирует, что эффекты исследований применяются единообразно across all scenes.
    /// </summary>
    public static ResearchEffectsManager Instance { get; private set; }

    /// <summary>
    /// Словарь для хранения накопленных эффектов исследований между сценами.
    /// Ключ: строка формата "TowerType_StatName" (например: "Riflemen_Damage")
    /// Значение: список эффектов, примененных к этому параметру башни
    /// </summary>
    private Dictionary<string, List<ResearchEffect>> towerEffects = new Dictionary<string, List<ResearchEffect>>();

    /// <summary>
    /// Инициализирует singleton-экземпляр и настраивает сохранение между сценами.
    /// </summary>
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

    /// <summary>
    /// Применяет все эффекты указанного исследованного узла к игровым системам.
    /// Вызывается ResearchManager при успешном завершении исследования.
    /// </summary>
    /// <param name="researchNode">Узел исследования, эффекты которого нужно применить.</param>
    public void ApplyResearchEffects(ResearchNodeSO researchNode)
    {
        if (researchNode.ResearchEffects == null) return;

        foreach (ResearchEffect effect in researchNode.ResearchEffects)
        {
            ApplyEffect(effect);
        }
    }

    /// <summary>
    /// Применяет отдельный эффект исследования в зависимости от его типа.
    /// </summary>
    /// <param name="effect">Эффект исследования для применения.</param>
    private void ApplyEffect(ResearchEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.TowerStatModifier:
                ApplyTowerStatModifier(effect);
                break;

            case EffectType.GlobalBonus:
                ApplyGlobalBonus(effect);
                break;

            case EffectType.UnlockTowerGrade:
                UnlockTower(effect);
                break;

            case EffectType.UnlockAbility:
                UnlockAbility(effect);
                break;
        }
    }

    /// <summary>
    /// Применяет модификатор параметров башни.
    /// Парсит TargetId для определения типа башни, параметра и типа модификатора.
    /// Сохраняет эффект для последующего применения к конкретным экземплярам башен.
    /// </summary>
    /// <param name="effect">Эффект модификатора башни.</param>
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

    /// <summary>
    /// Применяет глобальный бонус (не реализовано).
    /// </summary>
    /// <param name="effect">Эффект глобального бонуса.</param>
    private void ApplyGlobalBonus(ResearchEffect effect) { /* ... */ }

    /// <summary>
    /// Разблокирует новый грейд башни (не реализовано).
    /// </summary>
    /// <param name="effect">Эффект разблокировки башни.</param>
    private void UnlockTower(ResearchEffect effect) { /* ... */ }

    /// <summary>
    /// Разблокирует новую способность (не реализовано).
    /// </summary>
    /// <param name="effect">Эффект разблокировки способности.</param>
    private void UnlockAbility(ResearchEffect effect) { /* ... */ }

    /// <summary>
    /// ГЛАВНЫЙ МЕТОД: Применяет все исследования к конкретной башне при её создании.
    /// Вызывается TowerBehaviour при инициализации башни на тактическом уровне.
    /// </summary>
    /// <param name="tower">Экземпляр башни, к которой нужно применить эффекты исследований.</param>
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

    /// <summary>
    /// Применяет конкретный эффект к параметру башни.
    /// </summary>
    /// <param name="tower">Башня для модификации.</param>
    /// <param name="statName">Название параметра (Damage, Range, FireRate и т.д.).</param>
    /// <param name="modifierType">Тип модификатора (Flat - абсолютный, Mult - процентный).</param>
    /// <param name="value">Значение модификатора.</param>
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

    /// <summary>
    /// Метод для отладки: выводит в консоль все сохраненные эффекты исследований.
    /// Полезен для проверки корректности применения исследований.
    /// </summary>
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