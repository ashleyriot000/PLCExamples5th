using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CartesianRobotSpeaker : MonoBehaviour
{
    public ServoAmp amp;
    private AudioSource source;

    private bool busy = false;
    public bool IsBusy
    {
        set
        {
            if (busy == value)
                return;

            if(busy = value)
            {
                source.Play();
            }
            else
            {
                source.Stop();
            }
        }
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.Stop();
        source.playOnAwake = false;
        source.loop = true;
    }

    void Update()
    {
        IsBusy = amp.IsBusy || amp.IsJogging;
    }
}
