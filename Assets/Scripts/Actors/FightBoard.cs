
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightBoard : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private GameObject boardRoot;
    [SerializeField] private Button startFightButton;
    [SerializeField] private TextMeshProUGUI fightResultText;

    [Header("Fight Settings")]
    [SerializeField] private Fighter player;
    [SerializeField] private Fighter npc;
    [SerializeField] private Transform npcStart;
    [SerializeField] private Transform playerStart;
    [SerializeField] private Transform playerTransform;

    [Header("Board Settings")]
    [SerializeField] private bool hideOnFightStart = true;

    private FightManager fightManager;
    private bool fightStarted;

    private void Awake()
    {
        fightManager = GameManager.Instance.fightManager;

        if (boardRoot == null)
            boardRoot = gameObject;

        if (startFightButton != null)
            startFightButton.onClick.AddListener(StartFight);

        fightManager.OnFightEnded += HandleOnFightEnded;

        fightResultText.gameObject.SetActive(false);
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

    public void StartFight()
    {
        if (fightStarted)
            return;

        npc.gameObject.SetActive(true);

        fightManager.StartFight(player, npc);

        npc.transform.position = npcStart.position;
        npc.transform.rotation = npcStart.rotation;

        // Only hide the board if the fight actually started.
        if (!fightManager.FightInProgress)
            return;

        fightStarted = true;

        if (hideOnFightStart)
            boardRoot.SetActive(false);
    }

    public void HandleOnFightEnded(Fighter winner, Fighter loser)
    {
        npc.gameObject.SetActive(false);
        boardRoot.SetActive(true);
        fightStarted = false;
        
        playerTransform.position = playerStart.position;
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
