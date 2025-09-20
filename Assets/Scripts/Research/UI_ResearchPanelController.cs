using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResearchPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResearchBranchSO testBranch; // Перетащите сюда Branch_Riflemen из проекта
    [SerializeField] private Transform branchesContainer; // Parent object для веток
    [SerializeField] private UI_ResearchNode researchNodePrefab;
    [SerializeField] private Image connectorLinePrefab;

    private void OnEnable()
    {
        // Временно: при открытии панели просто создаем тестовую ветку
        if (testBranch != null)
        {
            CreateBranchUI(testBranch);
        }
    }

    private void OnDisable()
    {
        // Очищаем панель при закрытии, чтобы не копились старые элементы
        foreach (Transform child in branchesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateBranchUI(ResearchBranchSO branchData)
    {
        // 1. Создаем контейнер для ветки (вертикальный или горизонтальный)
        GameObject branchContainer = new GameObject(branchData.BranchName, typeof(VerticalLayoutGroup));
        var layout = branchContainer.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 150;

        branchContainer.transform.SetParent(branchesContainer, false);

        // 2. Для каждого узла в ветке...
        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // 3. Создаем UI элемент для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node); // Этот метод мы создадим в скрипте UI_ResearchNode

            // 4. (Позже) Здесь будем рисовать линии между узлами
            // if (i > 0) { DrawConnector(...); }
        }
    }

    // Этот метод будет вызываться из кнопки в UI
    public void OnResearchButtonClicked(ResearchNodeSO nodeData)
    {
        Debug.Log($"Button clicked for research: {nodeData.NodeName}");
        // Пока просто логируем. В следующем блоке добавим вызов менеджера исследований.
    }
}