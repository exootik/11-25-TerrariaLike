using UnityEngine;
using TMPro;

public class Debug_World : MonoBehaviour
{
    public TMP_Text Text;
    private void Start()
    {
        Game_Manager.Instance.RegisterDebugText(Text);
    }
}
