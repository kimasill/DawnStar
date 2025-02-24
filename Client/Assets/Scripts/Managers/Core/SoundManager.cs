using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        AudioListener audioListener = Camera.main.GetComponent<AudioListener>();
        if (audioListener != null)
            Object.Destroy(audioListener);
        _audioClips.Clear();
    }

    public void Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

	public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
	{
        if (audioClip == null)
            return;

		if (type == Define.Sound.Bgm)
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
			if (audioSource.isPlaying)
				audioSource.Stop();

			audioSource.pitch = pitch;
			audioSource.clip = audioClip;
			audioSource.Play();
		}
		else
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
	}
    public void PlayLoop(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        PlayLoop(audioClip, type, pitch);
    }

    public AudioSource PlayLoop(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        if (audioClip == null)
            return null;

        AudioSource audioSource = _audioSources[(int)type];
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.pitch = pitch;
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
    }

    public void Stop(AudioSource audio)
    {
        if (audio == null)
            return;

        audio.loop = false;
        audio.Stop();
    }
    public virtual void PlayBGM()
    {
        MapData mapData = null;
        Managers.Data.MapDict.TryGetValue(Managers.Map.CurrentMapId, out mapData);
        if (mapData == null)
            return;
        string mapName = mapData.name;
        if (string.IsNullOrEmpty(mapName))
            return;

        List<string> bgmFiles = new List<string>();
        int index = 1;
        while (true)
        {
            string filePath = $"Sounds/Bgm/{mapName}_{index}";
            AudioClip clip = Managers.Resource.Load<AudioClip>(filePath);
            if (clip == null)
                break;
            bgmFiles.Add(filePath);
            index++;
        }

        if (bgmFiles.Count > 0)
        {
            int randomIndex = Random.Range(0, bgmFiles.Count);
            Managers.Sound.Play(bgmFiles[randomIndex], Define.Sound.Bgm);
        }
    }
    public void SetTotalVolume(float volume)
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.volume = volume;
        }
    }

    public void SetBGMVolume(float volume)
    {
        _audioSources[(int)Define.Sound.Bgm].volume = volume;
    }

    AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{path}";

		AudioClip audioClip = null;

		if (type == Define.Sound.Bgm)
		{
			audioClip = Managers.Resource.Load<AudioClip>(path);
		}
		else
		{
			if (_audioClips.TryGetValue(path, out audioClip) == false)
			{
				audioClip = Managers.Resource.Load<AudioClip>(path);
				_audioClips.Add(path, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {path}");

		return audioClip;
    }
}
