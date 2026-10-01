using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage targetImage;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogError("[VideoController] VideoPlayer is missing.", this);
            return;
        }

        // 비디오 준비 시작
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.frameReady += OnFrameReady;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // 영상 준비가 끝나면 재생
        vp.Play();
    }

    private void OnFrameReady(VideoPlayer vp, long frameIndex)
    {
        if (targetImage != null)
            targetImage.enabled = true;

        vp.sendFrameReadyEvents = false;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("[VideoController] Video failed: " + message + " | " + vp.url, this);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.frameReady -= OnFrameReady;
        videoPlayer.errorReceived -= OnVideoError;
    }
}
