using UnityEngine;

public class TowerPlaceholder : MonoBehaviour
{
    private void OnMouseDown()
    {
        // ��� ����� �� ���� ������ ��������, ��� �� ������
        EventHub.OnTowerPlaceholderSelected?.Invoke(this);
    }
}