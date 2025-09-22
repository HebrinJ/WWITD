using System.Collections.Generic;

/// <summary>
/// Класс-контейнер для данных сохранения прогресса исследований.
/// Сериализуется в JSON для постоянного хранения в PlayerPrefs или файловой системе.
/// Используется <see cref="ResearchStateSO"/> для сохранения и загрузки состояния исследований между игровыми сессиями.
/// </summary>
[System.Serializable]
public class ResearchSaveData
{
    /// <summary>
    /// Словарь, содержащий состояния всех исследованных узлов.
    /// Ключ: уникальный идентификатор узла исследования (должен соответствовать имени ResearchNodeSO).
    /// Значение: текущее состояние узла (Locked, Available, Researched).
    /// </summary>
    public Dictionary<string, ResearchNodeState> NodeStates;
}