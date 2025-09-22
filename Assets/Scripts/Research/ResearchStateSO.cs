using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject для хранения и управления состоянием прогресса исследований игрока.
/// Сериализует состояние исследованных узлов в PlayerPrefs для сохранения между сессиями.
/// Интегрируется с <see cref="SaveManager"/> для автоматического сохранения/загрузки.
/// </summary>
[CreateAssetMenu(fileName = "ResearchState", menuName = "Research System/Research State")]
public class ResearchStateSO : ScriptableObject
{
    /// <summary>
    /// Вложенный класс для хранения состояния отдельного узла исследования.
    /// Содержит идентификатор узла и его текущее состояние.
    /// </summary>
    [System.Serializable]
    public class ResearchStateData
    {
        /// <summary>
        /// Уникальный идентификатор узла исследования (должен соответствовать имени ResearchNodeSO).
        /// </summary>
        public string nodeId;

        /// <summary>
        /// Текущее состояние узла исследования (заблокирован, доступен, исследован).
        /// </summary>
        public ResearchNodeState state;
    }

    /// <summary>
    /// Вложенный класс для сериализации данных сохранения исследований.
    /// Используется JsonUtility для преобразования в JSON и обратно.
    /// </summary>
    [System.Serializable]
    public class ResearchSaveData
    {
        /// <summary>
        /// Список состояний всех исследованных узлов.
        /// </summary>
        public List<ResearchStateData> researchedNodes = new List<ResearchStateData>();
    }

    /// <summary>
    /// Текущий список состояний исследованных узлов в памяти.
    /// Используется для быстрого доступа к состоянию узлов во время игры.
    /// </summary>
    public List<ResearchStateData> researchedNodes = new List<ResearchStateData>();

    /// <summary>
    /// Устанавливает состояние для указанного узла исследования.
    /// Если узел уже существует в списке - обновляет его состояние, иначе добавляет новый.
    /// Автоматически сохраняет изменения в PlayerPrefs.
    /// </summary>
    /// <param name="nodeId">Идентификатор узла исследования.</param>
    /// <param name="state">Новое состояние узла.</param>
    public void SetNodeState(string nodeId, ResearchNodeState state)
    {
        ResearchStateData existing = researchedNodes.Find(n => n.nodeId == nodeId);
        if (existing != null)
        {
            existing.state = state;
        }
        else
        {
            researchedNodes.Add(new ResearchStateData { nodeId = nodeId, state = state });
        }

        // Автоматическое сохранение при изменении состояния
        SaveState();
    }

    /// <summary>
    /// Возвращает состояние указанного узла исследования.
    /// Если узел не найден в списке, возвращает состояние Locked.
    /// </summary>
    /// <param name="nodeId">Идентификатор узла исследования.</param>
    /// <returns>Состояние узла или Locked если узел не найден.</returns>
    public ResearchNodeState GetNodeState(string nodeId)
    {
        ResearchStateData data = researchedNodes.Find(n => n.nodeId == nodeId);
        return data?.state ?? ResearchNodeState.Locked;
    }

    /// <summary>
    /// Сохраняет текущее состояние исследований в PlayerPrefs.
    /// Сериализует данные в JSON формат для постоянного хранения.
    /// </summary>
    public void SaveState()
    {
        ResearchSaveData saveData = new ResearchSaveData { researchedNodes = researchedNodes };
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("ResearchState", json);
        Debug.Log("Research state saved");
    }

    /// <summary>
    /// Загружает состояние исследований из PlayerPrefs.
    /// Восстанавливает прогресс игрока из предыдущей сессии.
    /// Если сохранение не найдено, инициализирует пустое состояние.
    /// </summary>
    public void LoadState()
    {
        if (PlayerPrefs.HasKey("ResearchState"))
        {
            string json = PlayerPrefs.GetString("ResearchState");
            ResearchSaveData saveData = JsonUtility.FromJson<ResearchSaveData>(json);
            if (saveData != null)
            {
                researchedNodes = saveData.researchedNodes;
                Debug.Log("Research state loaded");
            }
        }
        else
        {
            Debug.Log("No research state found - initializing new");
            researchedNodes = new List<ResearchStateData>();
        }
    }

    /// <summary>
    /// Сбрасывает состояние исследований (для тестирования).
    /// Очищает все данные в памяти и удаляет сохранение из PlayerPrefs.
    /// ВНИМАНИЕ: Этот метод должен быть удален или защищен в production-версии.
    /// </summary>
    public void ResetState()
    {
        researchedNodes.Clear();
        PlayerPrefs.DeleteKey("ResearchState");
        Debug.Log("Research state reset");
    }
}