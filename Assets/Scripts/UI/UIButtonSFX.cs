using UnityEngine;

public class UIButtonSFX : MonoBehaviour
{
    public void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
