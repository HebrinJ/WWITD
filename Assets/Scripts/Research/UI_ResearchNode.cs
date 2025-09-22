using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI-компонент для отображения и взаимодействия с отдельным узлом исследования в древе технологий.
/// Отображает состояние узла (заблокирован, доступен, исследован) и позволяет игроку взаимодействовать с ним.
/// Автоматически обновляет визуальное представление при изменении состояния узла или ресурсов.
/// </summary>
public class UI_ResearchNode : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Компонент Image для отображения иконки исследования.
    /// Визуально представляет тип улучшения или новую технологию.
    /// </summary>
    [Tooltip("Image для отображения иконки исследования.")]
    [SerializeField] private Image iconImage;

    /// <summary>
    /// TextMeshPro компонент для отображения названия исследования.
    /// Показывает краткое описание улучшения или технологии.
    /// </summary>
    [Tooltip("Text для отображения названия исследования.")]
    [SerializeField] private TextMeshProUGUI nameText;

    /// <summary>
    /// TextMeshPro компонент для отображения стоимости исследования.
    /// Показывает количество чертежей, необходимых для исследования.
    /// </summary>
    [Tooltip("Text для отображения стоимости исследования в чертежах.")]
    [SerializeField] private TextMeshProUGUI costText;

    /// <summary>
    /// Кнопка UI-обертка для узла для запуска процесса исследования.
    /// Становится активной только когда исследование доступно и хватает ресурсов.
    /// </summary>
    [Tooltip("Кнопка UI-обертка для узла для запуска исследования.")]
    [SerializeField] private Button researchButton;

    /// <summary>
    /// Фоновое изображение узла, меняющее цвет в зависимости от состояния.
    /// Визуально индицирует текущий статус исследования.
    /// Необходма замена на специализированные UI изображения - рамки или особые иконки
    /// </summary>
    [Tooltip("Фоновое изображение узла (меняет цвет в зависимости от состояния).")]
    [SerializeField] private Image backgroundImage;

    [Header("State Colors")]
    /// <summary>
    /// Цвет для заблокированного состояния (требования не выполнены).
    /// Необходма замена на специализированные UI изображения - рамки или особые иконки
    /// </summary>
    [Tooltip("Цвет для заблокированного состояния.")]
    [SerializeField] private Color lockedColor = Color.gray;

    /// <summary>
    /// Цвет для доступного состояния (можно исследовать, ресурсов хватает).
    /// Необходма замена на специализированные UI изображения - рамки или особые иконки
    /// </summary>
    [Tooltip("Цвет для доступного состояния (ресурсов хватает).")]
    [SerializeField] private Color availableColor = Color.yellow;

    /// <summary>
    /// Цвет для исследованного состояния (уже исследовано).
    /// Необходма замена на специализированные UI изображения - рамки или особые иконки
    /// </summary>
    [Tooltip("Цвет для исследованного состояния.")]
    [SerializeField] private Color researchedColor = Color.green;

    /// <summary>
    /// Цвет для состояния "доступен, но не хватает ресурсов".
    /// Необходма замена на специализированные UI изображения - рамки или особые иконки
    /// </summary>
    [Tooltip("Цвет для состояния 'доступен, но нет ресурсов'.")]
    [SerializeField] private Color unavailableColor = Color.red;

    /// <summary>
    /// Ссылка на данные узла исследования, которые отображает этот UI-компонент.
    /// </summary>
    private ResearchNodeSO nodeData;

    /// <summary>
    /// Инициализирует UI-узел исследования с указанными данными.
    /// Настраивает текстовые поля, подписывается на события и обновляет визуальное состояние.
    /// </summary>
    /// <param name="data">Данные узла исследования для отображения.</param>
    public void Initialize(ResearchNodeSO data)
    {
        nodeData = data;
        nameText.text = data.NodeName;
        costText.text = data.BlueprintCost.ToString();

        researchButton.onClick.AddListener(OnButtonClick);

        // Подписываемся на событие изменения ЛЮБОГО узла
        ResearchManager.OnNodeStateChanged += HandleAnyNodeStateChanged;

        // Первоначальное обновление
        UpdateVisuals();
    }

    /// <summary>
    /// Подписывается на события изменения ресурсов при активации объекта.
    /// </summary>
    private void OnEnable()
    {
        // Подписываемся на изменение глобальных ресурсов
        EventHub.OnGlobalResourceChanged += HandleResourceChanged;
    }

    /// <summary>
    /// Отписывается от событий при деактивации объекта для предотвращения утечек памяти.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleResourceChanged;
    }

    /// <summary>
    /// Обработчик изменения глобальных ресурсов.
    /// Обновляет визуальное состояние при изменении количества чертежей.
    /// </summary>
    /// <param name="resourceType">Тип изменившегося ресурса.</param>
    /// <param name="amount">Новое количество ресурса.</param>
    private void HandleResourceChanged(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.Blueprints)
        {
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Отписывается от событий при уничтожении объекта.
    /// </summary>
    private void OnDestroy()
    {
        ResearchManager.OnNodeStateChanged -= HandleAnyNodeStateChanged;
    }

    /// <summary>
    /// Обработчик изменения состояния любого узла в древе исследований.
    /// Обновляет визуальное представление текущего узла при изменении состояния любого узла.
    /// </summary>
    /// <param name="changedNodeName">Имя изменившегося узла.</param>
    /// <param name="newState">Новое состояние узла.</param>
    private void HandleAnyNodeStateChanged(string changedNodeName, ResearchNodeState newState)
    {
        // При изменении ЛЮБОГО узла обновляем свой визуал
        // Менеджер сам позаботится о правильном состоянии
        UpdateVisuals();
    }

    /// <summary>
    /// Обновляет визуальное представление узла на основе его текущего состояния.
    /// Меняет цвета, текст и доступность кнопки в зависимости от состояния и доступности ресурсов.
    /// </summary>
    private void UpdateVisuals()
    {
        if (ResearchManager.Instance == null || nodeData == null) return;

        ResearchNodeState state = ResearchManager.Instance.GetNodeState(nodeData);

        switch (state)
        {
            case ResearchNodeState.Locked:
                backgroundImage.color = lockedColor;
                researchButton.interactable = false;
                costText.text = "Locked";
                break;

            case ResearchNodeState.Available:
                // Проверяем, хватает ли ресурсов для отображения разного цвета
                int currentBlueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
                if (currentBlueprints >= nodeData.BlueprintCost)
                {
                    backgroundImage.color = availableColor;
                    researchButton.interactable = true;
                    costText.text = nodeData.BlueprintCost.ToString();
                }
                else
                {
                    backgroundImage.color = unavailableColor;
                    researchButton.interactable = false;
                    costText.text = $"{nodeData.BlueprintCost} (Need {nodeData.BlueprintCost - currentBlueprints})";
                }
                break;

            case ResearchNodeState.Researched:
                backgroundImage.color = researchedColor;
                researchButton.interactable = false;
                costText.text = "Researched";
                break;
        }
    }

    /// <summary>
    /// Обработчик нажатия на кнопку исследования.
    /// Пытается запустить исследование через ResearchManager.
    /// В случае неудачи выводит предупреждение в консоль.
    /// </summary>
    private void OnButtonClick()
    {
        bool success = ResearchManager.Instance.TryResearch(nodeData);
        if (!success)
        {
            Debug.LogWarning($"Cannot research {nodeData.NodeName}");
            // Можно добавить анимацию или звук ошибки
        }
    }
}