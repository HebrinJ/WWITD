using UnityEngine;
using UnityEngine.UI;

public class UI_StrategyMap : MonoBehaviour
{
    [SerializeField] private Button level01Button;
    [SerializeField] private Button battleButton;
    [SerializeField] private Button researchButton;

    private string selectedLevel = "";

    private void Start()
    {
        // ���������� ������ "��������" ���������
        battleButton.interactable = false;

        level01Button.onClick.AddListener(() => OnLevelSelected("Level01"));
        battleButton.onClick.AddListener(OnBattleClicked);
    }

    private void OnLevelSelected(string levelName)
    {
        selectedLevel = levelName;
        Debug.Log($"Selected level: {selectedLevel}");

        // ���������� ������ "��������" ����� ������� ������
        battleButton.interactable = true;

        // TODO: ����� �������� ���������� ��������� ���������� ������
    }

    private void OnBattleClicked()
    {
        if (!string.IsNullOrEmpty(selectedLevel))
        {
            Debug.Log($"Starting battle on: {selectedLevel}");
            SceneLoaderManager.Instance.LoadTacticalLevel(selectedLevel);
        }
        else
        {
            Debug.LogWarning("No level selected!");
        }
    }
}