using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _campaignButton;
    [SerializeField] private GameObject _randomLevelButton;

    [SerializeField] private GameObject _easyModeButton;
    [SerializeField] private GameObject _mediumModeButton;
    [SerializeField] private GameObject _hardModeButton;

    [SerializeField] private GameObject _levelSelector;
    [SerializeField] private TextMeshProUGUI _levelSelectorText;

    [SerializeField] private GameObject _backButton;

    private List<GameObject> _afterStartButtons;
    private List<GameObject> _startButtons;

    private void Start()
    {
        _afterStartButtons = new List<GameObject>();
        _afterStartButtons.Add(_easyModeButton);
        _afterStartButtons.Add(_mediumModeButton);
        _afterStartButtons.Add(_hardModeButton);
        _afterStartButtons.Add(_backButton);
        _afterStartButtons.Add(_levelSelector);

        _startButtons = new List<GameObject>();
        _startButtons.Add(_campaignButton);
        _startButtons.Add(_randomLevelButton);

        DataKeeper.instance.CampaignLevelNum = 1;
    }
    public void GoBack()
    {
        foreach (var button in _afterStartButtons)
        {
            button.SetActive(false);
        }
        foreach (var button in _startButtons)
        {
            button.SetActive(true);
        }
    }

    public void LoadGameLevel(int num)
    {
        foreach (var button in _startButtons)
        {
            button.SetActive(false);
        }
        AskLevelNum();
    }

    public void LoadRandomLevel()
    {
        foreach (var button in _startButtons)
        {
            button.SetActive(false);
        }
        AskDifficulty();
    }

    private void AskDifficulty()
    {
        _easyModeButton.SetActive(true);
        _mediumModeButton.SetActive(true);
        _hardModeButton.SetActive(true);
        _backButton.SetActive(true);
    }

    private void AskLevelNum()
    {
        _levelSelector.SetActive(true);
        _levelSelectorText.text = DataKeeper.instance.CampaignLevelNum.ToString();
        _backButton.SetActive(true);
    }

    public void ChangeLevelNum(int change)
    {
        if (DataKeeper.instance.CampaignLevelNum + change <= 3 && DataKeeper.instance.CampaignLevelNum + change >= 1)
            DataKeeper.instance.CampaignLevelNum += change;
        _levelSelectorText.text = DataKeeper.instance.CampaignLevelNum.ToString();
    }

    public void StartGame()
    {
        DataKeeper.instance.IsRandomLevel = false;
        SceneManager.LoadScene("RandomLevel");
    }

    public void StartGame(int difficulty)
    {
        DataKeeper.instance.RandomLevelDifficulty = difficulty;
        DataKeeper.instance.IsRandomLevel = true;
        SceneManager.LoadScene("RandomLevel");
    }
}
