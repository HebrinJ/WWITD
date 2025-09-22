using UnityEngine;

/// <summary>
/// Компонент-контейнер, определяющий путь движения для врагов на тактическом уровне.
/// Содержит массив точек (Transform), которые последовательно посещают враги при движении к штабу.
/// Реализует паттерн Singleton для глобального доступа к пути из любого места уровня.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Служит "рельсами" для движения врагов. Является data-holder,
/// не содержащим логики движения, только предоставляющим данные о пути.</para>
/// <para><b>Сцена использования:</b> Размещается на сцене уровня. Точки пути создаются как дочерние объекты.
/// <see cref="EnemyBehaviour"/> запрашивает путь через <see cref="Instance.PathPoints"/> для движения.</para>
/// </remarks>
public class Waypoints : MonoBehaviour
{
    /// <summary>
    /// Статическое свойство для глобального доступа к экземпляру Waypoints на уровне.
    /// Гарантирует, что все враги используют один и тот же путь.
    /// </summary>
    public static Waypoints Instance { get; private set; }

    /// <summary>
    /// Массив точек пути. Заполняется автоматически из дочерних объектов в Awake().
    /// Для ручного управления можно отключить автоматическое заполнение и назначать точки через инспектор.
    /// </summary>
    [SerializeField] private Transform[] pathPoints;

    /// <summary>
    /// Публичное свойство только для чтения для доступа к массиву точек пути.
    /// </summary>
    public Transform[] PathPoints => pathPoints;

    /// <summary>
    /// Метод инициализации Singleton. Вызывается при создании объекта.
    /// Автоматически заполняет массив точек пути дочерними объектами.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Автоматическое заполнение массива точками-детьми
        pathPoints = new Transform[transform.childCount];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i] = transform.GetChild(i);
        }
    }

    // Для визуализации пути в редакторе
     void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }
    }
}