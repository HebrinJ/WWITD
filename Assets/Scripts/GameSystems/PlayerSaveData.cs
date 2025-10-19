using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс-контейнер для всех данных сохранения игрока.
/// Сериализуется в JSON для хранения в PlayerPrefs.
/// </summary>
/// <remarks>
/// <para><b>Структура данных:</b> Содержит все необходимые данные для восстановления состояния игры
/// между сессиями. Все поля помечены как [Serializable] для корректной работы с JSON.</para>
/// </remarks>
[Serializable]
public class PlayerSaveData
{
    /// <summary>
    /// Версия структуры данных сохранения.
    /// Используется для миграции данных при обновлениях игры.
    /// </summary>
    public string saveVersion = "1.0.0";

    /// <summary>
    /// Список идентификаторов успешно пройденных уровней.
    /// Используется для определения доступных уровней на стратегической карте.
    /// </summary>
    /// <example>
    /// Пример содержимого: ["Level01", "Level02", "Level03"]
    /// </example>
    public List<string> completedLevels = new List<string>();

    /// <summary>
    /// Словарь глобальных ресурсов игрока.
    /// Ключ: ResourceType (преобразуется в string)
    /// Значение: количество ресурса
    /// </summary>
    /// <example>
    /// Пример содержимого: { "Blueprints": 25, "OtherCurrency": 100 }
    /// </example>
    public Dictionary<string, int> globalResources = new Dictionary<string, int>();

    /// <summary>
    /// Дата и время последнего сохранения.
    /// Используется для отображения информации о сохранении в UI.
    /// </summary>
    public string lastSaveTime;

    /// <summary>
    /// Общее количество чертежей, заработанных за все время игры.
    /// Используется для статистики и достижений.
    /// </summary>
    public int totalBlueprintsEarned;

    /// <summary>
    /// Создает новый экземпляр PlayerSaveData с начальными значениями.
    /// </summary>
    public PlayerSaveData()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        totalBlueprintsEarned = 0;
    }

    /// <summary>
    /// Добавляет уровень в список пройденных, если его там еще нет.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня для добавления.</param>
    /// <returns>true - если уровень был добавлен, false - если уже был в списке.</returns>
    public bool AddCompletedLevel(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("[PlayerSaveData] Попытка добавить пустой идентификатор уровня");
            return false;
        }

        if (!completedLevels.Contains(levelId))
        {
            completedLevels.Add(levelId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Проверяет, пройден ли указанный уровень.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня для проверки.</param>
    /// <returns>true - если уровень пройден, false - в противном случае.</returns>
    public bool IsLevelCompleted(string levelId)
    {
        return completedLevels.Contains(levelId);
    }

    /// <summary>
    /// Устанавливает количество глобального ресурса.
    /// </summary>
    /// <param name="resourceType">Тип ресурса.</param>
    /// <param name="amount">Количество ресурса.</param>
    public void SetGlobalResource(ResourceType resourceType, int amount)
    {
        string resourceKey = resourceType.ToString();
        globalResources[resourceKey] = amount;
    }

    /// <summary>
    /// Получает количество глобального ресурса.
    /// </summary>
    /// <param name="resourceType">Тип ресурса.</param>
    /// <returns>Количество ресурса. Если ресурс не найден, возвращает 0.</returns>
    public int GetGlobalResource(ResourceType resourceType)
    {
        string resourceKey = resourceType.ToString();
        return globalResources.ContainsKey(resourceKey) ? globalResources[resourceKey] : 0;
    }

    /// <summary>
    /// Обновляет время последнего сохранения.
    /// </summary>
    public void UpdateSaveTime()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}