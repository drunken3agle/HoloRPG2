using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System;
using UnityEngine.XR.WSA.Input;
using UnityEngine.SceneManagement;

public enum ScenarioState
{
    Scanning,
    Game_Started,
    NPC_Spawn,
    Quest_Accepted,
    Quest_Finished,
    Game_Over
}

public class ScenarioManager : MonoBehaviour {

    [SerializeField] private AbstracNpc npcToSpawn;
    [SerializeField] private AbstractEnemy enemy1;
    [SerializeField] private AbstractEnemy enemy2;
    [SerializeField] private AbstractEnemy enemy3;


    [SerializeField] private float delayToSpawnNPC = 2.0f;
    [SerializeField] private float delayToSpawnFirstEnemy = 2.4f;
    [SerializeField] private float delayToSpawnEnemy = 1.3f;
    

    private ScenarioState CurrentState;

    private GameObject npcInstance;


    private void Start()
    {
        SpatialUnderstanding.Instance.ScanStateChanged += OnGameStarted;
        GameManger.Instance.QuestCompleted += OnQuestFinished;
        GameManger.Instance.QuestTaken += OnQuestStarted;
        GameManger.Instance.EnemyKilled += OnEnemyKilled;
        GameManger.Instance.PlayerDied += OnPlayerDied;
        OnStateUpdated(ScenarioState.Scanning);
    }

   


    private void OnStateUpdated(ScenarioState newState)
    {
        CurrentState = newState;
        Debug.Log("Scenarion Manager : " + newState);
        switch (newState)
        {
            case ScenarioState.Game_Started:
                DecorateScene();
                StartCoroutine(SpawnNPCCoroutine());
                break;

            case ScenarioState.Quest_Accepted:
                npcInstance.GetComponent<AbstractAnchor>().AnchorPosition += Vector3.forward * 5; 
                StartCoroutine(SpawnEnemyCoroutine(enemy1, delayToSpawnFirstEnemy));   
                break;

            case ScenarioState.Quest_Finished:
                StopAllCoroutines();
                npcInstance.GetComponent<AbstractAnchor>().AnchorPosition -= Vector3.forward * 5;
                break;

            case ScenarioState.Game_Over:
                InteractionManager.InteractionSourceReleased += OnInteractionSourceReleased;
                break;
        }
    }

    

    private void OnGameStarted()
    {
        if (CurrentState == ScenarioState.Scanning)
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                OnStateUpdated(ScenarioState.Game_Started);
            }
        }
    }

    private void OnQuestStarted(IQuest quest)
    {
        if (CurrentState == ScenarioState.NPC_Spawn)
        {
            OnStateUpdated(ScenarioState.Quest_Accepted);
        }
    }

    

    private void OnEnemyKilled(IEnemy EnemyKilled)
    {
        if (CurrentState == ScenarioState.Quest_Accepted)
        {
            StartCoroutine(SpawnEnemyCoroutine(enemy2, delayToSpawnEnemy));
        }
    }
    

    private void OnQuestFinished(IQuest quest)
    {
        if (CurrentState == ScenarioState.Quest_Accepted)
        {
            OnStateUpdated(ScenarioState.Quest_Finished);
        }

    }

    private void OnPlayerDied()
    {
        OnStateUpdated(ScenarioState.Game_Over);
    }







    private IEnumerator SpawnNPCCoroutine()
    {
        if (CurrentState == ScenarioState.Game_Started)
        {
            yield return new WaitForSeconds(delayToSpawnNPC);

            npcInstance = ScanningManager.Instance.SpawnOnFloor(npcToSpawn.gameObject, 1f, 5f, 0f, 180f);
            OnStateUpdated(ScenarioState.NPC_Spawn);
        }
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator SpawnEnemyCoroutine(AbstractEnemy enemyToSpawn, float delay)
    {
        if (CurrentState == ScenarioState.Quest_Accepted)
        {
            yield return new WaitForSeconds(delay);
            ScanningManager.Instance.SpawnOnFloor(enemyToSpawn.gameObject, 1f, 25f, 0f, 180f);
        }
        yield return new WaitForEndOfFrame();
    }


    private void DecorateScene()
    {
        for (int i = 0; i < 7; i ++)
        {
            GameObject[] objectPool = RegionManager.Instance.regionDecorations[0].smallDecorationItems;
            GameObject objectToSpawn = objectPool[Utils.GetRndIndex(objectPool.Length)];
            ScanningManager.Instance.SpawnOnFloor(objectToSpawn, 1f, 25f, 0f, 360);
        }
    }

    private void OnInteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
    {
        SceneManager.LoadScene(0);
    }
}
