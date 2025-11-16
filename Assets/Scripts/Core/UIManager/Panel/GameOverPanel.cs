using System.Collections;
using System.Collections.Generic;
using iPAHeartBeat.Core.Dependency;
using milan_UI;
using milan.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : UIPanel
{
    public TextMeshProUGUI _currentScoreTMP;
    public TextMeshProUGUI _bestScoreTMP;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        _currentScoreTMP.text = $"CurrentScore: {DependencyResolver.Resolve<GameManager>().CurrentScore}";
        _bestScoreTMP.text = $"BestScore: {DependencyResolver.Resolve<GameManager>()._bestScore}";
    }

    public void RestartBtnCall()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
