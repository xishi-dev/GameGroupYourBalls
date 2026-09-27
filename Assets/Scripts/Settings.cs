using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("Volume", 1f);

        volumeSlider.value = volume;

        AudioListener.volume = volume;
    }

    // ลดเสียง
    public void DecreaseVolume()
    {
        volumeSlider.value -= 0.1f;
        SetVolume();
    }

    // เพิ่มเสียง
    public void IncreaseVolume()
    {
        volumeSlider.value += 0.1f;
        SetVolume();
    }

    // ปรับเสียงจาก Slider
    public void SetVolume()
    {
        float volume = volumeSlider.value;

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    // กลับ Main Menu
    public void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}