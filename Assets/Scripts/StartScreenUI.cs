using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
    }

    private void Update()
    {
        if (startPanel != null && startPanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (GameFlow.I != null)
        {
            GameFlow.I.StartRun();
        }
    }
}
