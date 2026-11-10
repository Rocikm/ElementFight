
using UnityEngine;
using System;
using System.Collections;

public class FightManager : MonoBehaviour
{
    [Header("Fight Settings")]
    [SerializeField] private bool fightInProgress;
    [SerializeField] private float fightEndDelay;

    private Fighter fighter1;
    private Fighter fighter2;

    public bool FightInProgress => fightInProgress;

    public Fighter Fighter1 => fighter1;
    public Fighter Fighter2 => fighter2;

    // Events
    public event Action<Fighter, Fighter> OnFightStarted;
    public event Action<Fighter, Fighter> OnFightEnded;

    public void StartFight(Fighter first, Fighter second)
    {
        if (fightInProgress)
        {
            Debug.LogWarning("A fight is already in progress!");
            return;
        }

        if (first == null || second == null || first == second)
        {
            Debug.LogWarning("Invalid fighters!");
            return;
        }

        fighter1 = first;
        fighter2 = second;

        fightInProgress = true;

        fighter1.ResetStats();
        fighter2.ResetStats();

        fighter1.SetCombatEnabled(true);
        fighter2.SetCombatEnabled(true);

        fighter1.OnDefeated += HandleFighterDefeated;
        fighter2.OnDefeated += HandleFighterDefeated;

        Debug.Log($"{fighter1.name} vs {fighter2.name}!");

        OnFightStarted?.Invoke(fighter1, fighter2);
    }

    private void HandleFighterDefeated(Fighter defeated)
    {
        if (!fightInProgress)
            return;

        Fighter winner = defeated == fighter1 ? fighter2 : fighter1;

        StartCoroutine(EndFight(winner, defeated, fightEndDelay));
    }

    public IEnumerator EndFight(Fighter winner, Fighter loser, float delay)
    {
        if (!fightInProgress)
            yield break;

        fighter1.SetCombatEnabled(false);
        fighter2.SetCombatEnabled(false);

        yield return new WaitForSeconds(delay);

        fightInProgress = false;

        fighter1.OnDefeated -= HandleFighterDefeated;
        fighter2.OnDefeated -= HandleFighterDefeated;

        Debug.Log($"{winner.name} wins! {loser.name} was defeated.");

        OnFightEnded?.Invoke(winner, loser);

        fighter1 = null;
        fighter2 = null;
    }
}
