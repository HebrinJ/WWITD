using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI-��������� ������ ������ �� �������������� �����.
/// ���������� ��������� ������ � ������������ �������������� ������.
/// ������������ ��� ������� ���������� �� ����� ��������������.
/// </summary>
/// <remarks>
/// <para><b>��������� � ����������:</b></para>
/// <list type="number">
/// <item><description>���������� ������ �� �������������� �����</description></item>
/// <item><description>��������� LevelId ��������������� LevelDataSO</description></item>
/// <item><description>��������� UI ���������� (�����������, ����� � �.�.)</description></item>
/// </list>
/// </remarks>
public class UI_LevelButton : MonoBehaviour
{
    [Header("Level Configuration")]
    /// <summary>
    /// ������������� ������, ��������������� LevelDataSO.
    /// ������ ��������� � levelId � ScriptableObject ������.
    /// </summary>
    [Tooltip("������������� ������ (������ ��������� � LevelDataSO.levelId).")]
    [SerializeField] private string levelId;

    [Header("UI Components")]
    /// <summary>
    /// ��������� Button ��� ��������� �������.
    /// </summary>
    [Tooltip("��������� Button ���� ������ ������.")]
    [SerializeField] private Button button;

    /// <summary>
    /// ����������� ������ ������.
    /// </summary>
    [Tooltip("����������� ������ ������.")]
    [SerializeField] private Image iconImage;

    /// <summary>
    /// ����� �������� ������.
    /// </summary>
    [Tooltip("����� �������� ������.")]
    [SerializeField] private TextMeshProUGUI nameText;

    /// <summary>
    /// ������� ���������������� ������.
    /// </summary>
    [Tooltip("GameObject ������� ���������������� ������.")]
    [SerializeField] private GameObject lockedOverlay;

    /// <summary>
    /// ������� ����������� ������.
    /// </summary>
    [Tooltip("GameObject ������� ����������� ������.")]
    [SerializeField] private GameObject completedOverlay;

    /// <summary>
    /// ��������� ���������� ������.
    /// </summary>
    [Tooltip("��������� ���������� ������.")]
    [SerializeField] private GameObject selectedIndicator;

    [Header("Visual States")]
    /// <summary>
    /// ���� ���������� ������.
    /// </summary>
    [Tooltip("���� ���������� ������.")]
    [SerializeField] private Color availableColor = Color.white;

    /// <summary>
    /// ���� ���������������� ������.
    /// </summary>
    [Tooltip("���� ���������������� ������.")]
    [SerializeField] private Color lockedColor = Color.gray;

    /// <summary>
    /// ���� ����������� ������.
    /// </summary>
    [Tooltip("���� ����������� ������.")]
    [SerializeField] private Color completedColor = Color.green;

    /// <summary>
    /// ������ ������, ��������� � ���� �������.
    /// </summary>
    public LevelDataSO LevelData { get; private set; }

    /// <summary>
    /// ������������� ������ ���� ������.
    /// </summary>
    public string LevelId => levelId;

    /// <summary>
    /// �������������� ������ ������.
    /// ���������� UI_StrategyMap ��� ������.
    /// </summary>
    /// <param name="levelData">������ ������.</param>
    /// <param name="onClickCallback">Callback ��� ������� �� ������.</param>
    public void Initialize(LevelDataSO levelData, System.Action<LevelDataSO> onClickCallback)
    {
        LevelData = levelData;

        // ��������� ������������ ���������������
        if (levelData.levelId != levelId)
        {
            Debug.LogWarning($"[UI_LevelButton] �������������� ���������������: ������ '{levelId}', ������ '{levelData.levelId}'");
        }

        // ����������� ������� ���
        if (nameText != null)
        {
            nameText.text = levelData.displayName;
        }

        if (iconImage != null && levelData.thumbnail != null)
        {
            iconImage.sprite = levelData.thumbnail;
        }

        // ��������� ����������
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(levelData));
        }

        Debug.Log($"[UI_LevelButton] ���������������� ������ ������: {levelData.displayName}");
    }

    /// <summary>
    /// ��������� ���������� ��������� ������ � ����������� �� ��������� ������.
    /// </summary>
    /// <param name="state">������� ��������� ������.</param>
    public void UpdateVisualState(LevelState state)
    {
        if (button != null)
        {
            button.interactable = state != LevelState.Locked;
        }

        // ��������� �������
        if (lockedOverlay != null)
            lockedOverlay.SetActive(state == LevelState.Locked);

        if (completedOverlay != null)
            completedOverlay.SetActive(state == LevelState.Completed);

        // ��������� ����
        if (iconImage != null)
        {
            iconImage.color = state switch
            {
                LevelState.Locked => lockedColor,
                LevelState.Available => availableColor,
                LevelState.Completed => completedColor,
                _ => availableColor
            };
        }

        Debug.Log($"[UI_LevelButton] ��������� ��������� ������ {LevelId}: {state}");
    }

    /// <summary>
    /// ������������� ��� ������� ��������� � ������.
    /// </summary>
    /// <param name="isSelected">true - �������� ������, false - ����� ���������.</param>
    public void SetSelected(bool isSelected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(isSelected);
    }

    /// <summary>
    /// ��������������� ����� ��� �������� ������������ ��������� � ����������.
    /// ���������� � ��������� Unity.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning($"[UI_LevelButton] �� �������� levelId �� {gameObject.name}", this);
        }

        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[UI_LevelButton] �� ������ ��������� Button �� {gameObject.name}", this);
            }
        }
    }
}