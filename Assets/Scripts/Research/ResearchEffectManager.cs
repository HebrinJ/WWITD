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
        // ������������ ������
        if (!activeEffects.ContainsKey(sourceId))
        {
            activeEffects[sourceId] = new List<ResearchEffect>();
        }
        activeEffects[sourceId].Add(effect);

        // ��������� ������
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
        // ����� ����� ������ ���������� ������������� � ������
        // ���� ������ ��������
        Debug.Log($"Tower {effect.TargetId} modified by {effect.Value}");
    }

    private void ApplyGlobalBonus(ResearchEffect effect)
    {
        // ���������� ������ (��������, +10% � ��������� ��������)
        Debug.Log($"Global bonus applied: {effect.StringValue} = {effect.Value}");
    }

    private void UnlockTower(ResearchEffect effect)
    {
        // ������������� ����� �����
        Debug.Log($"Unlocked tower: {effect.TargetId}");
        // ����� ����� �������� ����� � TowerManager ��� BuildManager
    }

    private void UnlockAbility(ResearchEffect effect)
    {
        // ������������� �����������
        Debug.Log($"Unlocked ability: {effect.TargetId}");
        // ����� ����� �������� ����� � AbilityManager
    }

    public float GetTowerStatModifier(string towerType, string statName)
    {
        // ���������� ��������� ����������� ��� ����������� ��������� �����
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
