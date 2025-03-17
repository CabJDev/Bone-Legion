// Author: Hope Oberez (H.O)
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private SoundFXManager sfxManager;

    [SerializeField] private AudioClip buttonPress;
    [SerializeField] private AudioClip menuSound;

	private void Start()
	{
		sfxManager = SoundFXManager.Instance;
        sfxManager.PlaySound(menuSound, new Vector3(0, 0, 0), 1f);
	}

	public void LoadGame()
    {
        sfxManager.PlaySound(buttonPress, new Vector3(0, 0, 0), 1f);
        SceneManager.LoadScene(1);
    }
    public void ExitGame()
    {
		sfxManager.PlaySound(buttonPress, new Vector3(0, 0, 0), 1f);
		Application.Quit();
    }
}
