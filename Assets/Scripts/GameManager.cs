using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    [SerializeField] private GameObject _pauseBackground;
    [SerializeField] private GameObject _askAgainButton;
    [SerializeField] private GameObject _mainMenuButton;
    [SerializeField] private GameObject _continueGameButton;
    [SerializeField] private GameObject _pauseButton;

    [SerializeField] private TextMeshProUGUI _gameResultText;

    [SerializeField] private TextMeshProUGUI _shotsText;
    [SerializeField] private Shooter _shooter;
    [SerializeField] private int _shotsAvailable;
    private int _shotsLeft;

    private int _minSequenceSize = 3;
    private List<Transform> _bubbleSequence;

    public int ShotsLeft { get { return _shotsLeft; } }
    public int ShotsAvailable { set => _shotsAvailable = value; }

    public void GameStart()
    {
        _bubbleSequence = new List<Transform>();
        _shotsLeft = _shotsAvailable;
        _shotsText.text = _shotsLeft.ToString();
        _shooter.ShooterStart();
    }

    public void ProcessTurn(Transform currentBubble)
    {
        _bubbleSequence.Clear();
        CheckBubbleSequence(currentBubble);

        if(_bubbleSequence.Count >= _minSequenceSize)
        {
            DestroyBubblesInSequence();
            DropDisconectedBubbles();
        }

        StartCoroutine(ProcessBubbles());
    }

    public void OnShoot()
    {
        _shotsLeft--;
        _shotsText.text = _shotsLeft.ToString();
    }

    private void CheckBubbleSequence(Transform bubbleTransform)
    {
        _bubbleSequence.Add(bubbleTransform);

        Bubble bubble = bubbleTransform.GetComponent<Bubble>();
        List<Transform> neighbors = bubble.GetNeighbors();

        foreach(Transform neighbor in neighbors)
        {
            if (!_bubbleSequence.Contains(neighbor))
            {
                Bubble bubbleNeighbor = neighbor.GetComponent<Bubble>();

                if (bubbleNeighbor.BubbleColor == bubble.BubbleColor)
                {
                    CheckBubbleSequence(neighbor);
                }
            }
        }
    }

    private void DestroyBubblesInSequence()
    {
        foreach(Transform bubbleTransform in _bubbleSequence)
        {
            if (bubbleTransform.gameObject != null) GameLevelManager.instance.RecycleBubble(bubbleTransform);
        }
    }

    private void DropDisconectedBubbles()
    {
        SetAllBubblesConnectionToFalse();
        SetConnectedBubblesToTrue();
        SetGravityToDisconectedBubbles();
    }

    #region Drop Disconected Bubbles
    private void SetAllBubblesConnectionToFalse()
    {
        foreach (Transform bubble in GameLevelManager.instance.BubblesTransform)
        {
            bubble.GetComponent<Bubble>().IsConnected = false;
        }
    }

    private void SetConnectedBubblesToTrue()
    {
        _bubbleSequence.Clear();

        RaycastHit2D[] hits = Physics2D.RaycastAll(GameLevelManager.instance.HighestPos, Vector3.right);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.gameObject.GetComponent<Bubble>() != null)
                SetNeighboursConnectionToTrue(hits[i].transform);
        }
    }

    private void SetNeighboursConnectionToTrue(Transform bubbleTransform)
    {
        Bubble bubble = bubbleTransform.GetComponent<Bubble>();
        bubble.IsConnected = true;
        _bubbleSequence.Add(bubbleTransform);

        foreach(Transform bubbleNeighborTransform in bubble.GetNeighbors())
        {
            if(!_bubbleSequence.Contains(bubbleNeighborTransform))
            {
                SetNeighboursConnectionToTrue(bubbleNeighborTransform);
            }
        }
    }

    private void SetGravityToDisconectedBubbles()
    {
        var bufferToRemove = new List<Transform>();
        foreach (Transform bubbleTransform in GameLevelManager.instance.BubblesTransform)
        {
            if (!bubbleTransform.GetComponent<Bubble>().IsConnected)
            {
                bufferToRemove.Add(bubbleTransform);
                bubbleTransform.gameObject.GetComponent<CircleCollider2D>().enabled = false;
                if(!bubbleTransform.GetComponent<Rigidbody2D>())
                {
                    Rigidbody2D rb2d = bubbleTransform.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                }       
            }
        }
        foreach (Transform bubbleTransform in bufferToRemove)
        {
            GameLevelManager.instance.RemoveBubbleTransform(bubbleTransform);
        }
    }
    #endregion

    public void OnGameEnd()
    {
        StartCoroutine(CheckGameResults());
    }

    private void OnGameEnd(bool isWin)
    {
        Time.timeScale = 0;
        _shooter.AbleToShoot = false;
        _pauseBackground.SetActive(true);
        _gameResultText.gameObject.SetActive(true);
        _askAgainButton.SetActive(true);
        _mainMenuButton.SetActive(true);
        _pauseButton.SetActive(false);
        if (isWin)
        {
            _gameResultText.text = "Congrats!";
        }
        else
        {
            _gameResultText.text = "You lose";
        }
    }

    public void RestartGame()
    {
        _pauseButton.SetActive(true);
        _pauseBackground.SetActive(false);
        _askAgainButton.SetActive(false);
        _mainMenuButton.SetActive(false);
        _gameResultText.gameObject.SetActive(false);
        Time.timeScale = 1;
        foreach (Transform bubbleTransform in GameLevelManager.instance.BubbleArea)
        {
            if (bubbleTransform != null)
            {
                Destroy(bubbleTransform.gameObject);
            }
        }
        GameLevelManager.instance.Start();
    }
    #region PauseButtons
    public void GoToMainMenu()
    {
        RestartGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void PressPause()
    {
        _pauseBackground.SetActive(true);
        _mainMenuButton.SetActive(true);
        _continueGameButton.SetActive(true);
        _pauseButton.SetActive(false);
        _shooter.AbleToShoot = false;
        Time.timeScale = 0;
    }

    public void ContinueGame()
    {
        _pauseBackground.SetActive(false);
        _mainMenuButton.SetActive(false);
        _continueGameButton.SetActive(false);
        _pauseButton.SetActive(true);
        _shooter.AbleToShoot = true;
        Time.timeScale = 1;
    }
    #endregion
    private IEnumerator ProcessBubbles()
    {
        yield return new WaitForEndOfFrame();

        GameLevelManager.instance.UpdateScene();
        if (GameLevelManager.instance.BubblesLeft <= 0) OnGameEnd(true);

        _shooter.OnBubbleCollided();
    }

    private IEnumerator CheckGameResults()
    {
        yield return new WaitForSeconds(0.5f);
        if (GameLevelManager.instance.BubblesLeft > 0) OnGameEnd(false);
        else OnGameEnd(true);
    }
}