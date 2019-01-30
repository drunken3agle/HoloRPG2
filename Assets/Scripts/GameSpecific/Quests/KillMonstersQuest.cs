// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillMonstersQuest : AbstractQuest
{
    [Header("Kill Monsters")]
    [SerializeField] private AbstractEnemy[] enemiesToKill;
    [SerializeField] private int numberToKill = 5;

    public int KilledMonsters { get; set; }

    void Start()
    {
       GameManger.Instance.EnemyKilled += OnEnemyKilled;
    }

    protected override int GetQuestProgression()
    {
        return (int)(((KilledMonsters * 1.0f) / numberToKill) * 100);
    }

    protected override bool QuestAvailable()
    {
        return true; // TODO
    }

    protected override bool QuestCompleted()
    {
        return KilledMonsters >= numberToKill;
    }

    private void OnEnemyKilled(IEnemy enemy)
    {
        foreach(AbstractEnemy enemyToKill in enemiesToKill)
        {
            if (enemy.EnemyName == enemyToKill.EnemyName)
            {
                KilledMonsters++;
                /*   if (QuestCompleted() == true)
                   {
                       QuestManager.Instance.OnQuestCompleted(this);
                   }*/
            }
        } 

        AbstractEnemy nextEnemy = enemiesToKill[Random.Range(0, enemiesToKill.Length - 1)];
        ScanningManager.Instance.SpawnOnFloor(nextEnemy.gameObject, nextEnemy.AttackRange, nextEnemy.AttackRange * 2,
                                              0f, 45f);
    }
}
