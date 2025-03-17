using System.Threading.Tasks;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

	[SerializeField] private AudioSource audioFXSource;

	[SerializeField] private AudioClip SkeletonWalk1;
	[SerializeField] private AudioClip SkeletonWalk2;
	[SerializeField] private AudioClip SkeletonAttack;
	[SerializeField] private AudioClip SkeletonDie;

	[SerializeField] private AudioClip ButtonPress;

	public bool playSounds = true;

	public float gameVolume = 0.25f;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		} 
		else
		{
			Instance = this;
		}
	}

	public void PlaySound(AudioClip clip, Vector3 position, float volume)
	{
		if (!playSounds) return;
		AudioSource audioSource = Instantiate(audioFXSource, position, Quaternion.identity);
		audioSource.clip = clip;
		audioSource.volume = volume * gameVolume;
		audioSource.Play();
		Destroy(audioSource.gameObject, audioSource.clip.length);
	}

	public void ButtonPressSound()
	{
		if (!playSounds) return;
		AudioSource audioSource = Instantiate(audioFXSource, new Vector3(0, 0, 0), Quaternion.identity);
		audioSource.clip = ButtonPress;
		audioSource.volume = 1f * gameVolume;
		audioSource.Play();
		Destroy(audioSource.gameObject, audioSource.clip.length);
	}

	public void PlaySound(SoundType type, Vector3 position, float volume)
	{
		if (!playSounds) return;
		AudioSource audioSource = Instantiate(audioFXSource, new Vector3(position.x, position.y, 0), Quaternion.identity);
		if (type == SoundType.EnemyWalk)
		{
			int clipNumber = Random.Range(0, 2);

			if (clipNumber == 0)
				audioSource.clip = SkeletonWalk1;
			else
				audioSource.clip = SkeletonWalk2;
		}
		else if (type == SoundType.EnemyAttack)
		{
			audioSource.clip = SkeletonAttack;
		}
		else if (type == SoundType.EnemyDie)
		{
			audioSource.clip = SkeletonDie;
		}
			
		audioSource.volume = volume * gameVolume;
		audioSource.spatialBlend = 1f;
		audioSource.Play();
		Destroy(audioSource.gameObject, audioSource.clip.length);
	}
}

public enum SoundType
{
	EnemyWalk,
	EnemyAttack,
	EnemyDie
}
