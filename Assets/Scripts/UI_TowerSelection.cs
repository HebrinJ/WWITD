using UnityEngine;
using UnityEngine.UI;

public class UI_TowerSelection : MonoBehaviour
{
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button machineGunButton;
    [SerializeField] private Button closeButton;

    private TowerPlaceholder currentPlaceholder;

    private void OnEnable()
    {
        EventHub.OnTowerPlaceholderSelected += ShowSelectionPanel;
    }

    private void OnDisable()
    {
        EventHub.OnTowerPlaceholderSelected -= ShowSelectionPanel;
    }

    private void Start()
    {
        // Скрываем панель при старте
        selectionPanel.SetActive(false);

        // Настраиваем кнопки
        machineGunButton.onClick.AddListener(() => SelectTower(TowerType.MachineGun));
        closeButton.onClick.AddListener(HideSelectionPanel);
    }

    private void ShowSelectionPanel(TowerPlaceholder placeholder)
    {
        Debug.Log("Show Panel");
        currentPlaceholder = placeholder;
        selectionPanel.SetActive(true);

        // Позиционируем панель
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(placeholder.transform.position);
        selectionPanel.transform.position = screenPosition + new Vector3(0, 100, 0);
    }

    private void HideSelectionPanel()
    {
        selectionPanel.SetActive(false);
        currentPlaceholder = null;
    }

    private void SelectTower(TowerType towerType)
    {
        if (currentPlaceholder != null)
        {
            // Сообщаем о выборе башни
            EventHub.OnTowerSelectedFromUI?.Invoke(towerType);
            HideSelectionPanel();
        }
    }
}