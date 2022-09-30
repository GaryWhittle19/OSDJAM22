using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] public AudioSource menuMusic;
    [SerializeField] public AudioSource storyMusic;

    private AudioSource playingSong;

    // Start is called before the first frame update
    void Start()
    {
        playingSong = menuMusic;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Set a new song to be played
    public void SetQueuedSong( AudioSource songToPlay, float fadeDuration )
    {
        if( songToPlay != playingSong )
        {
            StartCoroutine( FadeOutThenPlay( playingSong, songToPlay, fadeDuration ) );// FadeOutSong( playingSong, fadeDuration );
        }
    }

    // Update is called once per frame
    IEnumerator FadeOutThenPlay( AudioSource song, AudioSource newSong, float duration )
    {
        float timePassed = 0f;
        while ( timePassed < duration )
        {
            // will always be a factor between 0 and 1
            var factor = timePassed / duration;
            // optional ease-in and ease-out
            // factor = Mathf.SmoothStep(0,1,factor);

            // linear interpolate the position
            song.volume = Mathf.Lerp( 1.0f, 0.0f, factor );

            // increase timePassed by time since last frame
            // using Min to avoid overshooting
            timePassed += Mathf.Min( Time.deltaTime, duration - timePassed );

            // "pause" the routine here, render the frame
            // and continue from here in the next one
            yield return null;
        }
        song.Stop();
        newSong.Play();
        newSong.volume = 1.0f;
        newSong.loop = true;
    }
}
