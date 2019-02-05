using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System;
using UnityEngine.XR.WSA.Input;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // Enemy prefabs
    [SerializeField] private AbstractEnemy smallRhino;
    [SerializeField] private AbstractEnemy bigRhino;
    [SerializeField] private AbstractEnemy devilMeelee;
    [SerializeField] private AbstractEnemy devilProjectile;
    [SerializeField] private AbstractEnemy dragon;

    [SerializeField] private float startGameDelay = 5;
    [SerializeField] private float delayToSpawnNPC = 2.0f;
    [SerializeField] private float delayToSpawnFirstEnemy = 2.4f;
    [SerializeField] private float delayToSpawnEnemy = 1.3f;

    [SerializeField] private AudioSource background_AudioSource;
    [SerializeField] private AudioSource gameOver_AudioSource;
    [SerializeField] private Image gameOver_sprite;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private ScenarioState CurrentState;

    private GameObject npcInstance;

    private int killProgress = 0;

    private void Start()
    {
        //SpatialUnderstanding.Instance.ScanStateChanged += OnGameStarted;
        GameManger.Instance.QuestCompleted += OnQuestFinished;
        GameManger.Instance.QuestTaken += OnQuestStarted;
        GameManger.Instance.EnemyKilled += OnEnemyKilled;
        GameManger.Instance.PlayerDied += OnPlayerDied;


        OnStateUpdated(ScenarioState.Scanning);
        StartCoroutine(StartTheGame(startGameDelay));
    }


    private IEnumerator StartTheGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnStateUpdated(ScenarioState.Game_Started);

    }

    /// <summary>
    /// Callback for whenever a state is updated.
    /// Add changes here.
    /// </summary>
    /// <param name="newState"> New state </param>    
    private void OnStateUpdated(ScenarioState newState)
    {
        CurrentState = newState;
        Debug.Log("Scenarion Manager : " + newState);
        switch (newState)
        {
            case ScenarioState.Game_Started:
                background_AudioSource.Play();
                //DecorateScene();
                StartCoroutine(SpawnNPCCoroutine());
                break;

            case ScenarioState.Quest_Accepted:
                npcInstance.GetComponent<AbstractAnchor>().AnchorPosition += Vector3.forward * 20; 
                StartCoroutine(SpawnEnemyCoroutine(smallRhino, delayToSpawnFirstEnemy));   
                break;

            case ScenarioState.Quest_Finished:
                StopAllCoroutines();
                npcInstance.GetComponent<AbstractAnchor>().AnchorPosition -= Vector3.forward * 20;
                break;

            case ScenarioState.Game_Over:
                background_AudioSource.Stop();
                gameOver_AudioSource.Play();
                gameOver_sprite.enabled = true;
                KillAllEnemeies();
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
            killProgress++;
            switch (killProgress)
            {
                case 1:
                StartCoroutine(SpawnEnemyCoroutine(bigRhino, delayToSpawnEnemy));
                break;

                case 2:
                StartCoroutine(SpawnEnemyCoroutine(devilMeelee, delayToSpawnEnemy));
                break;

                case 3:
                StartCoroutine(SpawnEnemyCoroutine(devilProjectile, delayToSpawnEnemy));
                break;

                case 4:
                StartCoroutine(SpawnEnemyCoroutine(dragon, delayToSpawnEnemy));
                break;

            }
            
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

            npcInstance = SpawnAnchor(npcToSpawn);
            OnStateUpdated(ScenarioState.NPC_Spawn);
        }
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator SpawnEnemyCoroutine(AbstractEnemy enemyToSpawn, float delay)
    {
        if (CurrentState == ScenarioState.Quest_Accepted)
        {
            yield return new WaitForSeconds(delay);
            GameObject spawnedEnemy = SpawnAnchor(enemyToSpawn);
            spawnedEnemies.Add(spawnedEnemy);
        }
        yield return new WaitForEndOfFrame();
    }

    private GameObject SpawnAnchor(AbstractAnchor anchor)
    {
        Vector3 playerPosition      = CameraHelper.Stats.camPos;
        Vector3 lookingOrientation = CameraHelper.Stats.camLookDir;
        Vector3 groundPosition = CameraHelper.Stats.groundPos;

        Vector3 finalPosition = playerPosition + lookingOrientation * 4.0f;
        finalPosition = new Vector3(finalPosition.x, groundPosition.y, finalPosition.z);

        Quaternion finalRotation = VectorUtils.LookAt2D(finalPosition, playerPosition);

        GameObject spawnedAnchor = Instantiate(anchor.gameObject, finalPosition, finalRotation);
        return spawnedAnchor;
 
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

    private void KillAllEnemeies()
    {
        foreach(GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
    }
}
