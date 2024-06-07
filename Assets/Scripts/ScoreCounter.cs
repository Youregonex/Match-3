using UnityEngine;
using TMPro;

public sealed class ScoreCounter : MonoBehaviour
{
    private static ScoreCounter _instance;

    public static ScoreCounter Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ScoreCounter();

            return _instance;
        }
    }

    private int _score;
    public int Score
    {
        get => _score;

        set
        {
            if (_score == value)
                return;

            _score = value;
            _scoreText.text = $"Score : {_score}";
        }
    }

    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

}
