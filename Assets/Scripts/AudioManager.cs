using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource; // privateÇpublicÇ…èCê≥
    public AudioClip titleMusic;
    public AudioClip selectMusic;
    public AudioClip gameplayMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip nextClip = null;
        switch (sceneName)
        {
            case "Title":
                nextClip = titleMusic;
                break;
            case "Select":
                nextClip = selectMusic;
                break;
            case "Main":
                nextClip = gameplayMusic;
                break;
            default:
                nextClip = null;
                break;
        }

        if (nextClip != null && audioSource.clip != nextClip)
        {
            audioSource.clip = nextClip;
            audioSource.Play();
        }
        else if (nextClip == null)
        {
            audioSource.Stop();
        }
    }
}