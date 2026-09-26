using UnityEngine;

public class WormAnimation : MonoBehaviour
{
    public Renderer wormRenderer;

    public Texture frame1;
    public Texture frame2;
    public Texture frame3;

    public float frameTime = 0.15f;

    private float timer;
    private int frame;

    void Start()
    {
        wormRenderer.material.mainTexture = frame1;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;

            frame++;

            if (frame > 2)
                frame = 0;

            if (frame == 0)
                wormRenderer.material.mainTexture = frame1;

            if (frame == 1)
                wormRenderer.material.mainTexture = frame2;

            if (frame == 2)
                wormRenderer.material.mainTexture = frame3;
        }
    }
}