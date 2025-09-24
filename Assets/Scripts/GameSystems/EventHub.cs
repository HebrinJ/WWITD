using System;
using UnityEngine;

/// <summary>
/// Статический класс, являющийся центральным узлом для событийной коммуникации между системами игры.
/// Реализует шаблон "Шина событий" (Event Bus) для обеспечения слабой связанности компонентов.
/// Любая система может вызвать событие, и любая система может на него подписаться, не зная друг о друге.
/// </summary>
/// <remarks>
/// <para><b>Важность:</b> Является краеугольным камнем архитектуры проекта. Все ключевые взаимодействия систем проходят через него.</para>
/// <para><b>Потокобезопасность:</b> События не являются потокобезопасными. Все вызовы должны происходить из Main Thread (Unity).</para>
/// <para><b>Инициализация:</b> Подписка на события обычно происходит в `OnEnable`, отписка - в `OnDisable`.</para>
/// </remarks>
public static class EventHub
{
    // === ГЛОБАЛЬНЫЕ РЕСУРСЫ (Blueprints/Чертежи) ===

    /// <summary>
    /// Событие вызывается при ЛЮБОМ изменении количества глобального ресурса (чертежей).
    /// </summary>
    /// <param name="resourceType">Тип изменившегося ресурса.</param>
    /// <param name="newAmount">Новое, текущее количество данного ресурса.</param>
    /// <example>
    /// Typical usage: UI для отображения количества чертежей подписывается на это событие для обновления.
    /// <code>EventHub.OnGlobalResourceChanged += UpdateBlueprintsUI;</code>
    /// </example>
    public static Action<ResourceType, int> OnGlobalResourceChanged;

    /// <summary>
    /// Событие вызывается при добавлении (увеличении) количества глобального ресурса.
    /// Отличается от OnGlobalResourceChanged тем, что не вызывается при трате ресурса.
    /// </summary>
    /// <param name="resourceType">Тип добавленного ресурса.</param>
    /// <param name="amountAdded">Количество, которое было добавлено.</param>
    public static Action<ResourceType, int> OnGlobalResourceAdded;

    /// <summary>
    /// Событие-запрос на проверку и списание глобальных ресурсов.
    /// Вызывается, например, при попытке исследования в меню технологий.
    /// Подписчики должны проверить наличие ресурсов и, если возможно, списать их.
    /// </summary>
    /// <param name="cost">Структура, описывающая стоимость (тип и количество ресурса).</param>
    public static Action<ResourceCost> OnGlobalResourceSpendRequested;


    // === ЛОКАЛЬНЫЕ РЕСУРСЫ (Money, Iron, Fuel) ===

    /// <summary>
    /// Событие вызывается при ЛЮБОМ изменении количества локального ресурса на текущем уровне.
    /// </summary>
    /// <param name="resourceType">Тип изменившегося ресурса (деньги, железо, горючее).</param>
    /// <param name="newAmount">Новое, текущее количество данного ресурса.</param>
    public static Action<ResourceType, int> OnLocalResourceChanged;

    /// <summary>
    /// Событие вызывается при добавлении (увеличении) количества локального ресурса на уровне.
    /// </summary>
    /// <param name="resourceType">Тип добавленного ресурса.</param>
    /// <param name="amountAdded">Количество, которое было добавлено.</param>
    public static Action<ResourceType, int> OnLocalResourceAdded;

    /// <summary>
    /// Событие-запрос на проверку и списание локальных ресурсов.
    /// Вызывается системой строительства при попытке постройки или улучшения башни, ремонта, использования способностей.
    /// Подписчики должны проверить наличие ресурсов и, если возможно, списать их.
    /// В случае успеха ожидается вызов события OnLocalResourceChanged.
    /// </summary>
    /// <param name="cost">Структура, описывающая стоимость (тип и количество ресурса).</param>
    public static Action<ResourceCost> OnLocalResourceSpendRequested;

    /// <summary>
    /// Событие, вызываемое при смерти врага, за которого положена денежная награда.
    /// Отличается от общего OnEnemyDied, так как награда может зависеть от типа врага, его уровня и т.д.
    /// Подписчик: LocalResourceManager.
    /// </summary>
    /// <param name="enemy">Поведение (Behaviour) врага, который был уничтожен. Содержит данные для расчета награды.</param>
    public static Action<EnemyBehaviour> OnEnemyDiedForMoney;


    // === СТРОИТЕЛЬСТВО И УПРАВЛЕНИЕ БАШНЯМИ ===

    /// <summary>
    /// Событие вызывается, когда игрок выбирает (тапает) на плейсхолдер для башни на карте.
    /// Подписчик: BuildManager и UI_TowerSelection, чтобы показать доступные для постройки варианты.
    /// </summary>
    /// <param name="placeholder">Выбранный плейсхолдер. Содержит информацию о позиции и допустимых типах башен.</param>
    public static Action<TowerPlaceholder> OnTowerPlaceholderSelected;

    /// <summary>
    /// Событие вызывается из UI, когда игрок выбирает конкретный тип башни для постройки.
    /// Подписчик: BuildManager, чтобы запомнить выбор и ожидать подтверждения на месте.
    /// </summary>
    /// <param name="towerType">Тип башни, выбранный из UI.</param>
    public static Action<TowerType> OnTowerSelectedFromUI;

    /// <summary>
    /// Событие подтверждения постройки. Вызывается после выбора места и типа, обычно по кнопке "Построить".
    /// Подписчик: BuildManager, который должен создать экземпляр башни и списать ресурсы.
    /// </summary>
    /// <param name="towerData">Данные (ScriptableObject) строящейся башни.</param>
    /// <param name="buildPosition">Позиция в мире (World Space), где должна быть построена башня.</param>
    public static Action<TowerDataSO, Vector3> OnTowerBuildConfirmed;

    /// <summary>
    /// Событие вызывается, если попытка постройки не удалась (например, недостаточно ресурсов).
    /// Подписчик: UI система для показа сообщения об ошибке.
    /// </summary>
    public static Action OnTowerBuildFailed;

    /// <summary>
    /// Событие-команда на переключение режима строительства. Вызывается при нажатии кнопки "Режим строительства".
    /// Подписчик: ConstructionModeManager, который активирует/деактивирует логику и визуализацию режима.
    /// </summary>
    public static Action OnConstructionModeToggled;

    /// <summary>
    /// Событие, оповещающее об изменении состояния режима строительства (включен/выключен).
    /// Отличается от OnConstructionModeToggled тем, что содержит новое состояние.
    /// Подписчики: UI_ConstructionButton.
    /// </summary>
    /// <param name="isActive">True - режим строительства активирован, False - деактивирован.</param>
    public static Action<bool> OnConstructionModeChanged;

    /// <summary>
    /// Событие-запрос на отображение меню опций для уже построенной башни (улучшение, уничтожение, ремонт).
    /// Вызывается при нажатии на существующую башню в режиме строительства.
    /// Подписчик: UI система, которая должна показать контекстное меню с кнопками.
    /// </summary>
    /// <param name="tower">Поведение (Behaviour) башни, для которой запрошены опции.</param>
    public static Action<TowerBehaviour> OnTowerOptionsRequested;


    // === ВОЛНЫ И ВРАГИ ===

    /// <summary>
    /// Событие вызывается при начале новой волны врагов.
    /// Подписчики: Менеджер волн WaveManager.
    /// </summary>
    public static Action OnWaveStarted;

    /// <summary>
    /// Событие вызывается после того, как все враги текущей волны были уничтожены или достигли базы.
    /// Подписчики: WaveManager (для запуска следующей волны), UI, система наград.
    /// </summary>
    public static Action OnWaveCompleted;

    /// <summary>
    /// Событие вызывается, когда враг достигает конечной точки пути (штаба игрока).
    /// Подписчик: LevelManager управляющий логикой на тактическом уровне.
    /// </summary>
    /// <param name="enemy">Поведение (Behaviour) врага, достигшего базы.</param>
    public static Action<EnemyBehaviour> OnEnemyReachedBase;

    /// <summary>
    /// Событие вызывается при начале обратного отсчета до следующей волны.
    /// </summary>
    /// <param name="delay">Длительность таймера в секундах.</param>
    public static Action<float> OnNextWaveTimerStarted;

    /// <summary>
    /// Событие вызывается при обновлении таймера следующей волны.
    /// </summary>
    /// <param name="remainingTime">Оставшееся время в секундах.</param>
    public static Action<float> OnNextWaveTimerUpdated;

    /// <summary>
    /// Событие вызывается по окончании таймера следующей волны.
    /// </summary>
    public static Action OnNextWaveTimerEnded;

    /// <summary>
    /// Событие вызывается при начале конкретной волны.
    /// </summary>
    /// <param name="waveNumber">Номер начавшейся волны (начиная с 1).</param>
    public static Action<int> OnWaveStartedWithNumber;

    /// <summary>
    /// Событие вызывается при завершении конкретной волны.
    /// </summary>
    /// <param name="waveNumber">Номер завершенной волны.</param>
    public static Action<int> OnWaveCompletedWithNumber;

    /// <summary>
    /// Событие вызывается когда все волны уровня пройдены и все враги уничтожены.
    /// </summary>
    public static Action OnAllWavesCompleted;


    // === ИГРОВОЙ ПРОЦЕСС И СОСТОЯНИЯ ===

    /// <summary>
    /// Событие вызывается для старта игрового процесса на уровне (после фазы подготовки).
    /// Подписчик: Пока отсутствует.
    /// </summary>
    public static Action OnGameStart;

    /// <summary>
    /// Событие вызывается при поражении игрока (здоровье базы опустилось до 0).
    /// Подписчики: Пока отсутствует.
    /// </summary>
    public static Action OnGameOver;

    /// <summary>
    /// Общее событие смерти любого врага. Вызывается в момент его уничтожения.
    /// Подписчики: WaveManager (для отслеживания прогресса волны).
    /// </summary>
    /// <param name="enemy">Поведение (Behaviour) уничтоженного врага.</param>
    public static Action<EnemyBehaviour> OnEnemyDied;

    /// <summary>
    /// Событие вызывается при успешном завершении уровня (все волны отражены, база цела).
    /// Подписчики: Пока отсутствуют.
    /// </summary>
    public static Action OnLevelComplete;


    // === СИСТЕМА ИССЛЕДОВАНИЙ ===

    /// <summary>
    /// Событие вызывается при переключении вкладки в меню исследований (например, с "Башен" на "Способности").
    /// Подписчик: UI меню исследований для отображения соответствующего дерева технологий (UI_ResearchPanelController)
    /// </summary>
    /// <param name="tabType">Тип вкладки, на которую произошло переключение.</param>
    public static Action<ResearchTabType> OnResearchTabChanged;
}