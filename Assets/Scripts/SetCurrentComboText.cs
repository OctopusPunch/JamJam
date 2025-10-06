using TMPro;
using UnityEngine;

public class SetCurrentComboText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI comboText;

    // Update is called once per frame
    void LateUpdate()
    {
        comboText.text = GameManager.Instance.ChainedPerfects.ToString();
    }
}
