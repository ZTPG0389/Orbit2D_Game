using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerDebug : MonoBehaviour
{
    VideoPlayer _vp;

    void Awake()
    {
        _vp = GetComponent<VideoPlayer>();
        _vp.errorReceived      += OnVideoError;
        _vp.prepareCompleted   += OnPrepareCompleted;
        _vp.loopPointReached   += OnLoopPointReached;
    }

    void Start()
    {
        Debug.Log($"[VideoPlayer] START — " +
                  $"clip={(_vp.clip != null ? _vp.clip.name : "NULL")} " +
                  $"isPrepared={_vp.isPrepared} " +
                  $"isPlaying={_vp.isPlaying} " +
                  $"renderMode={_vp.renderMode} " +
                  $"targetTexture={(_vp.targetTexture != null ? _vp.targetTexture.name : "NULL")}");

        if (_vp.clip == null)
        {
            Debug.LogError("[VideoPlayer] No clip assigned — assign a VideoClip in the Inspector.");
            return;
        }

        if (!_vp.isPrepared)
        {
            Debug.Log("[VideoPlayer] Not prepared — calling Prepare().");
            _vp.Prepare();
        }
        else
        {
            PlayVideo();
        }
    }

    void OnPrepareCompleted(VideoPlayer vp)
    {
        Debug.Log($"[VideoPlayer] PrepareCompleted — " +
                  $"clip={vp.clip?.name} width={vp.width} height={vp.height} " +
                  $"frameCount={vp.frameCount} duration={vp.length:F1}s");
        PlayVideo();
    }

    void PlayVideo()
    {
        if (!_vp.isPlaying)
        {
            _vp.Play();
            Debug.Log("[VideoPlayer] Play() called.");
        }
    }

    void OnLoopPointReached(VideoPlayer vp)
    {
        Debug.Log("[VideoPlayer] Loop point reached.");
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoPlayer] ERROR — {message}");
    }

    void OnDestroy()
    {
        if (_vp == null) return;
        _vp.errorReceived    -= OnVideoError;
        _vp.prepareCompleted -= OnPrepareCompleted;
        _vp.loopPointReached -= OnLoopPointReached;
    }
}
