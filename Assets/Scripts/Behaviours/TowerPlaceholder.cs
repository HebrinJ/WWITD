using UnityEngine;

public class TowerPlaceholder : MonoBehaviour
{
    private void OnMouseDown()
    {
        // При клике на этот объект сообщаем, что он выбран
        EventHub.OnTowerPlaceholderSelected?.Invoke(this);
    }
}