using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity;

public class QuestManager : Singleton<QuestManager> {

    // accepted quest instances
    private List<IQuest> acceptedQuests;
    // completed quests (no instance)
    private List<IQuest> completedQuests;

    [Tooltip("NPC providing quests"), SerializeField] 
    private GameObject QuestNPC;

    private GameObject NPCInstance = null;

    void Start() {
        SpatialUnderstanding.Instance.ScanStateChanged += StartGame;
        GameManger.Instance.QuestCompleted += QuestFinished;
        GameManger.Instance.QuestTaken += QuestStarted;
    }

    protected override void OnDestroy(){
        SpatialUnderstanding.Instance.ScanStateChanged -= StartGame;
        GameManger.Instance.QuestCompleted -= QuestFinished;
        GameManger.Instance.QuestTaken -= QuestStarted;

        base.OnDestroy();
    }

    private void StartGame(){
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done) {   
            NPCInstance = ScanningManager.Instance.SpawnOnFloor(QuestNPC, 1f, 5f, 90f, 180f);
        }
    }

    private void QuestStarted(IQuest quest) {
        if (NPCInstance != null) { 
            Destroy(NPCInstance); 
            NPCInstance = null;
        }
    }

    private void QuestFinished(IQuest quest) {        
        NPCInstance = ScanningManager.Instance.SpawnOnFloor(QuestNPC, 1f, 5f, 90f, 180f);
    }

    protected override void Awake()
    {
        base.Awake();
        acceptedQuests = new List<IQuest>();
        completedQuests = new List<IQuest>();
    }

    public void AcceptQuest(IQuest newQuest)
    {
        if (acceptedQuests.Contains(newQuest) == false)
        {
            // instantiate Quest 
            GameObject questPrefab = Instantiate<GameObject>(Resources.Load<GameObject>(newQuest.QuestID));
            questPrefab.transform.parent = transform;

            // Add instantiated quest to list
            acceptedQuests.Add(questPrefab.GetComponent<IQuest>());

            GameManger.Instance.InvokeQuestTaken(newQuest);
            GameManger.Instance.InvokeUpdateWorldUI();

           
        }
        else
        {
            Debug.LogError("Trying to add a quest that already exists");
        }
    }

    public bool HasTakenQuest(IQuest takenQuest)
    {
        foreach(IQuest quest in acceptedQuests)
        {
            if (quest.QuestID == takenQuest.QuestID)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasTakenQuest(string takenQuestID)
    {
        foreach(IQuest quest in acceptedQuests)
        {
            if (quest.QuestID == takenQuestID)
            {
                return true;
            }
        }
        return false;
    }

    public IQuest GetQuestInstance(string questID)
    {
        foreach(IQuest quest in acceptedQuests)
        {
            if (quest.QuestID == questID)
            {
                return quest;
            }
        }
        Debug.LogError("Quest " + questID + " not found");
        return null;
    }

    public void OnQuestCompleted(IQuest completedQuest)
    {
        // find gameobject instance of quest and destroy it
        bool childFound = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i); 
            if (child.GetComponent<IQuest>().QuestID == completedQuest.QuestID)
            {
                Destroy(child.gameObject);
                childFound = true;
            }
        }
        if (childFound == false) Debug.LogError("Quest Gameobject not foud.");

        // remove quest from list and add it completed list
        acceptedQuests.Remove(completedQuest);
        completedQuests.Add(completedQuest);

        // Trigger event
        GameManger.Instance.InvokeQuestCompleted(completedQuest);
    }

    public List<string> GetAcceptedQuestsProgression()
    {
        List<string> questsDesc = new List<string>();
        if (acceptedQuests.Count > 0)
        {
            foreach (IQuest quest in acceptedQuests)
            {
                string questText = quest.ShortDescription + " (" + quest.QuestProgression + "%)";
                questsDesc.Add(questText);
            } 
        }
        else
        {
            questsDesc.Add("There is acutally no quest available.");
        }
        if (completedQuests.Count > 0)
        {
            questsDesc.Add("\n  <b>Completed Quests :</b>");
            foreach (IQuest quest in completedQuests)
            {
                string questText = quest.ShortDescription;
                questsDesc.Add(questText);
            }
        }
        return questsDesc;
    }
}
