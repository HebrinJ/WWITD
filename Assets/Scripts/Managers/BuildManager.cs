using UnityEngine;

/// <summary>
/// Центральный менеджер, отвечающий за процесс строительства и управления башнями на игровом уровне.
/// Координирует взаимодействие между выбором игрока, проверкой ресурсов, данными башен и их созданием в мире.
/// Является основным подписчиком и обработчиком событий, связанных со строительством.
/// </summary>
/// <remarks>
/// <para><b>Взаимодействие:</b></para>
/// <list type="bullet">
/// <item><description>Подписывается на события UI: <see cref="EventHub.OnTowerPlaceholderSelected"/>, <see cref="EventHub.OnTowerSelectedFromUI"/>.</description></item>
/// <item><description>Взаимодействует с <see cref="LocalResourceManager"/> для проверки и списания ресурсов.</description></item>
/// <item><description>Запрашивает данные башен через <see cref="GameData.Instance"/>.</description></item>
/// <item><description>Создает экземпляры башен и применяет к ним эффекты исследований через <see cref="ResearchEffectsManager"/>.</description></item>
/// </list>
/// </remarks>
public class BuildManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ссылка на менеджер локальных ресурсов (деньги, железо, горючее) для проверки стоимости строительства.")]
    private LocalResourceManager localResourceManager;

    /// <summary>
    /// Текущий выбранный игроком плейсхолдер для строительства. 
    /// Хранится после события OnTowerPlaceholderSelected и очищается после постройки или отмены.
    /// </summary>
    private TowerPlaceholder selectedPlaceholder;

    /// <summary>
    /// Тип башни, выбранный для постройки в текущем плейсхолдере.
    /// Хранится после события OnTowerSelectedFromUI и очищается после постройки или если ресурсов недостаточно.
    /// </summary>
    private TowerType pendingTowerType;

    private void OnEnable()
    {
        EventHub.OnTowerPlaceholderSelected += HandlePlaceholderSelected;
        EventHub.OnTowerSelectedFromUI += HandleTowerSelected;
        EventHub.OnTowerBuildConfirmed += HandleBuildConfirmed;
    }

    private void OnDisable()
    {
        EventHub.OnTowerPlaceholderSelected -= HandlePlaceholderSelected;
        EventHub.OnTowerSelectedFromUI -= HandleTowerSelected;
        EventHub.OnTowerBuildConfirmed -= HandleBuildConfirmed;
    }

    /// <summary>
    /// Обработчик события выбора плейсхолдера. Сохраняет выбранный плейсхолдер для последующего использования.
    /// </summary>
    /// <param name="placeholder">Выбранный плейсхолдер, содержащий информацию о позиции и ограничениях.</param>
    private void HandlePlaceholderSelected(TowerPlaceholder placeholder)
    {
        selectedPlaceholder = placeholder;
    }

    /// <summary>
    /// Обработчик события выбора типа башни из UI. Инициирует процесс проверки и попытки постройки.
    /// Для работы требует предварительно выбранный плейсхолдер.
    /// </summary>
    /// <param name="towerType">Тип башни, выбранный игроком.</param>
    private void HandleTowerSelected(TowerType towerType)
    {
        if (selectedPlaceholder != null)
        {
            pendingTowerType = towerType;
            TryBuildTower(towerType);
        }
    }

    /// <summary>
    /// Основной метод попытки постройки башни. Проверяет доступность данных и ресурсов.
    /// Если проверки проходят успешно, списывает ресурсы и вызывает метод непосредственной постройки.
    /// </summary>
    /// <param name="towerType">Тип башни для постройки.</param>
    private void TryBuildTower(TowerType towerType)
    {
        // 1. Получение данных башни из центрального реестра
        TowerDataSO towerData = GameData.Instance.GetTowerData(towerType);

        if (towerData == null)
        {
            Debug.LogError("[BuildManager] Данные башни не найдены для типа: " + towerType);
            return;
        }

        // 2. Проверка всех требуемых ресурсов по списку стоимости
        bool canAffordAll = true;
        foreach (ResourceCost cost in towerData.constructionCost)
        {
            if (localResourceManager != null && !localResourceManager.CanAfford(cost.resourceType, cost.amount))
            {
                canAffordAll = false;
                Debug.Log($"[BuildManager] Недостаточно {cost.resourceType}! Требуется: {cost.amount}");
                // TODO: Вызвать EventHub.OnTowerBuildFailed с указанием причины (мало ресурса X)
                break;
            }
        }

        // 3. Если ресурсов хватает - списать их и построить башню
        if (canAffordAll)
        {
            foreach (ResourceCost cost in towerData.constructionCost)
            {
                ResourceCost resourceCost = new ResourceCost(cost.resourceType, cost.amount);
                EventHub.OnLocalResourceSpendRequested?.Invoke(resourceCost);
            }
            BuildTower(towerData);
        }
        else
        {
            Debug.Log("[BuildManager] Недостаточно ресурсов для постройки этой башни");
            pendingTowerType = TowerType.None;
            EventHub.OnTowerBuildFailed?.Invoke(); // Уведомляем UI о неудаче
        }
    }

    /// <summary>
    /// Обработчик события подтверждения постройки. Зарезервирован.
    /// В текущей реализации выбор башни из UI сразу инициирует постройку.
    /// </summary>
    /// <param name="data">Данные башни.</param>
    /// <param name="position">Позиция для постройки.</param>
    private void HandleBuildConfirmed(TowerDataSO data, Vector3 position)
    {
        // Резерв для будущей системы подтверждения постройки
        // Debug.Log("Build confirmed event received. Future implementation.");
    }

    /// <summary>
    /// Финальный метод непосредственной постройки башни в мире.
    /// Создает экземпляр префаба, настраивает его и деактивирует использованный плейсхолдер.
    /// </summary>
    /// <param name="towerData">Данные строящейся башни.</param>
    private void BuildTower(TowerDataSO towerData)
    {
        if (selectedPlaceholder != null)
        {
            InstantiateTower(towerData, selectedPlaceholder.transform.position);

            // Делаем placeholder неактивным после постройки
            selectedPlaceholder.gameObject.SetActive(false);
            selectedPlaceholder = null;
            pendingTowerType = TowerType.None;
        }
    }

    /// <summary>
    /// Создает экземпляр башни в мире, инициализирует его данные и применяет исследованные улучшения.
    /// </summary>
    /// <param name="towerData">ScriptableObject с данными башни.</param>
    /// <param name="position">Позиция в мировом пространстве для создания башни.</param>
    private void InstantiateTower(TowerDataSO towerData, Vector3 position)
    {
        // 1. Создание экземпляра префаба башни
        GameObject towerInstance = Instantiate(towerData.towerPrefab, position, Quaternion.identity);
        Debug.Log($"[BuildManager] Построена башня: {towerData.towerName}");

        // 2. Инициализация данных башни
        TowerBehaviour towerBehaviour = towerInstance.GetComponent<TowerBehaviour>();
        if (towerBehaviour != null)
        {
            towerBehaviour.SetData(towerData);
        }
        else
        {
            Debug.LogError("[BuildManager] У префаба башни отсутствует компонент TowerBehaviour: " + towerData.towerName);
        }

        // 3. Применение эффектов от исследований (улучшений) к новой башне
        if (ResearchEffectsManager.Instance != null)
        {
            ResearchEffectsManager.Instance.ApplyResearchEffectsToTower(towerBehaviour);
        }
        else
        {
            Debug.LogWarning("[BuildManager] ResearchEffectsManager не найден! Башня построена без учёта исследований.");
        }
    }
}