
// Класс-контейнер для данных о враге в волне
[System.Serializable] // Важно! Чтобы он отображался в Инспекторе
public class WaveEnemy
{
    public EnemyDataSO enemyData; // Какого врага спавнить
    public int count; // Сколько всего таких врагов
    public float spawnInterval; // С каким интервалом их спавнить (в секундах)
}
