using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Videos;

public class RenderLoadingDisplay : MonoBehaviour
{
    [SerializeField] Image loadingBarImage  = null;
    [SerializeField] Text progressText      = null;
    [SerializeField] Text infoText          = null;

    void Start()
    {
        VideoRenderer.onStateChanged    += RendererStateChanged;
        VideoRenderer.onProgressChanged += RendererProgressChanged;

        if (VideoRenderer.state is DoneState)
            gameObject.SetActive(false);

        UpdateText(VideoRenderer.state);
    }

    void Update()
    {
        UpdateText(VideoRenderer.state);
    }

    void OnDestroy()
    {
        VideoRenderer.onStateChanged    -= RendererStateChanged;
        VideoRenderer.onProgressChanged -= RendererProgressChanged;
    }

    void RendererStateChanged(RenderState state)
    {
        if (state is DoneState || state is ConfirmPixelsState)
        {
            if (gameObject.activeInHierarchy)
                gameObject.SetActive(false);
        }
        else
        {
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);
        }

        UpdateText(state);
    }

    void UpdateText(RenderState state)
    {
        if (state is DoneState)
            infoText.text = "DONE";

        if (state is PrepereQueueState)
            infoText.text = "PREPARING RENDERER";

        if (state is FullRenderState)
            infoText.text = "RENDERING";

        if (state is ConfirmPixelsState)
            infoText.text = "UPLOADING MISSING PIXELS";

        if (state is ResendBufferState)
            infoText.text = "RESENDING BUFFERS";
    }

    void RendererProgressChanged(float progress)
    {
        loadingBarImage.fillAmount = progress;
        progressText.text = ((int)(progress * 100)).ToString() + "%";

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }
}