# Архитектура событий (Event System)

## Обзор
Центральным узлом коммуникации между системами в проекте является статический класс `EventHub`. Он реализует паттерн "Event Bus"...

## Ключевые принципы
1.  **Слабая связанность:**...
2.  **Ответственность:**...
3.  **Порядок вызова:**...

## Группы событий

### 1. Ресурсы
| Событие | Вызывается, когда... | Подписывается, чтобы... |
| :--- | :--- | :--- |
| `OnLocalResourceChanged` | ... | ... |
| `OnLocalResourceSpendRequested` | ... | ... |

### 2. Строительство
...

## Руководство по использованию
### Как подписаться на событие
```csharp
void OnEnable()
{
    EventHub.OnEnemyDied += HandleEnemyDeath;
}

void OnDisable()
{
    EventHub.OnEnemyDied -= HandleEnemyDeath;
}

private void HandleEnemyDeath(EnemyBehaviour enemy)
{
    // Логика обработки
}