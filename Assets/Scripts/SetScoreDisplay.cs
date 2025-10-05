using System.Numerics;
using TMPro;
using UnityEngine;

public class SetScoreDisplay : MonoBehaviour
{
    public static SetScoreDisplay Instance => instance;
    private static SetScoreDisplay instance;

    public TextMeshProUGUI scoreDisplay;
    public AudioSource sfxSource;
    public AudioClip milestoneClip;

    private BigInteger currentDisplay = 0;
    private BigInteger lastMilestone = 0;

    private float lerpSpeed = 5f;

    void Awake()
    {
        instance = this;
        currentDisplay = 0;
    }

    void Update()
    {
        BigInteger targetScore = GameManager.Instance.Score;

        if (targetScore != currentDisplay)
        {
            BigInteger difference = targetScore - currentDisplay;
            if (difference != 0)
            {
                BigInteger step = (BigInteger)(difference / (BigInteger)10);
                if (step == 0) step = difference > 0 ? 1 : -1;
                currentDisplay += step;
            }

            scoreDisplay.text = currentDisplay.ToString("N0");

            CheckMilestones(currentDisplay);
        }
    }

    void CheckMilestones(BigInteger value)
    {
        BigInteger[] milestones = {
            100_000, 1_000_000, 10_000_000, 100_000_000,
            1_000_000_000, 10_000_000_000, 100_000_000_000, 
            1000_000_000_000, 100000_000_000_000, 1000000_000_000_000, 
            10000000_000_000_000
        };

        foreach (var milestone in milestones)
        {
            if (value >= milestone && lastMilestone < milestone)
            {
                if (sfxSource && milestoneClip)
                {
                    sfxSource.PlayOneShot(milestoneClip);
                }
                lastMilestone = milestone;
                break;
            }
        }
    }
}
