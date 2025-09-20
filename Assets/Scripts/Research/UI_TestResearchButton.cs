using UnityEngine;
using UnityEngine.UI;

public class UI_TestResearchButton : MonoBehaviour
{
    [SerializeField] private Button testResetResearchButton;
    private void Start()
    {
        testResetResearchButton.onClick.AddListener(ResetResearch);
    }

    private void ResetResearch()
    {
        ResearchManager.Instance.ResetResearchProgress();
    }
}
