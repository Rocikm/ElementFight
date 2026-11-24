
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightBoard : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private GameObject boardRoot;
    [SerializeField] private Button startFightButton;
    [SerializeField] private TextMeshProUGUI fightResultText;
    [SerializeField] private TextMeshProUGUI fighterTypeText;
    [SerializeField] private VRScreenFade screenFade;

    [Header("Fight Settings")]
    [SerializeField] private Fighter player;
    [SerializeField] private Fighter npc;
    [SerializeField] private FighterInitializer npcInitializer;
    [SerializeField] private Transform npcStart;
    [SerializeField] private Transform playerStart;
    [SerializeField] private Transform playerTransform;

    [Header("Board Settings")]
    [SerializeField] private bool hideOnFightStart = true;
    [SerializeField] private float fadeTime = 0.5f;

    private FightManager fightManager;
    private bool fightStarted;
    private FighterType selectedFighterType;

    private void Awake()
    {
        fightManager = GameManager.Instance.fightManager;

        if (boardRoot == null)
            boardRoot = gameObject;

        if (startFightButton != null)
            startFightButton.onClick.AddListener(StartFight);

        SetFighterType(0);

        fightManager.OnFightEnded += HandleOnFightEnded;

        // fightResultText.gameObject.SetActive(false);
    }

    void Start()
    {
        npc.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (startFightButton != null)
            startFightButton.onClick.RemoveListener(StartFight);
    }

    public void SetFighterType(int type)
    {
        SetFighterType((FighterType) type);
    }

    public void SetFighterType(FighterType type)
    {
        selectedFighterType = type;
        fighterTypeText.text = type.ToString();
    }

    public void StartFight()
    {
        if (fightStarted)
            return;

        StartCoroutine(StartSequence());

        fightManager.StartFight(player, npc);
    }

    private IEnumerator StartSequence()
    {
        yield return screenFade.FadeIn(fadeTime);

        npc.gameObject.SetActive(true);

        npc.transform.position = npcStart.position;
        npc.transform.rotation = npcStart.rotation;
        
        npcInitializer.SetFighterType(selectedFighterType);

        playerTransform.position = new Vector3(
            playerStart.position.x, playerTransform.position.y, playerStart.position.z);
        // player.transform.rotation = playerStart.rotation;

        fightStarted = true;

        if (hideOnFightStart)
            boardRoot.SetActive(false);
        
        yield return new WaitForSeconds(0.1f);

        yield return screenFade.FadeOut(fadeTime);
    }

    private IEnumerator EndSequence(Fighter winner, Fighter loser)
    {
        yield return screenFade.FadeIn(fadeTime);

        npc.gameObject.SetActive(false);
        boardRoot.SetActive(true);
        fightStarted = false;
        
        playerTransform.position = new Vector3(
            playerStart.position.x, playerTransform.position.y, playerStart.position.z);
        // player.transform.rotation = playerStart.rotation;

        fightResultText.gameObject.SetActive(true);
        if (winner == player)
        {
            fightResultText.text = "You Won!";
            fightResultText.color = Color.green;
        }
        else
        {
            fightResultText.text = "You Lost!";
            fightResultText.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        yield return screenFade.FadeOut(fadeTime);
    }

    public void HandleOnFightEnded(Fighter winner, Fighter loser)
    {
        StartCoroutine(EndSequence(winner, loser));
    }

    // Can be used by future settings/menu buttons.
    public void ShowBoard()
    {
        fightStarted = false;
        boardRoot.SetActive(true);
    }

    public void HideBoard()
    {
        boardRoot.SetActive(false);
    }
}

public enum FighterType
{
    Air,
    Water,
    Earth,
    Fire,
    Mixed
}
