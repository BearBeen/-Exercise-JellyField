using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalUI : MonoBehaviour
{
    [SerializeField] private Image _img;
    [SerializeField] private TMP_Text _count;

    public void InitGoalUI(Color color, int count)
    {
        _img.color = color;
        SetCount(count);
    }

    public void SetCount(int count)
    {
        _count.text = count.ToString();
    }
}
