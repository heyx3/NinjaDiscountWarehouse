using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles looping a ControlledNoise by continuously
/// fading a new copy in as the old copy fades out.
/// </summary>
public class FadeLoopNoise
{
    public Transform SoundLoopContainer;

    /// <summary>
    /// The ControlledNoise to loop.
    /// </summary>
    public ControlledNoise Loop { get; private set; }

    /// <summary>
    /// Whether or not the noise is currently looping.
    /// </summary>
    public bool Running { get; private set; }

    private ControlledNoise[] loopInstances;
    private int loopsSoFar;
    private string loopName;

    public FadeLoopNoise(GameObject toLoop, Transform audioParent, string loopName)
    {
        SoundLoopContainer = audioParent;

        Loop = toLoop.GetComponent<ControlledNoise>();
        if (Loop != null)
        {
            Loop.name = loopName + " base";
            this.loopName = loopName;
        }

        loopInstances = new ControlledNoise[2];

        Running = false;
    }

    /// <summary>
    /// Copies the Loop noise object, stores it in the given index, and starts playing it.
    /// </summary>
    private void MakeCopy(int index)
    {
        GameObject copy = (GameObject)GameObject.Instantiate(Loop.gameObject);

        loopsSoFar += 1;
        copy.name = loopName + " " + loopsSoFar.ToString();

        copy.transform.parent = SoundLoopContainer.transform;
        copy.transform.localPosition = Vector3.zero;

        loopInstances[index] = copy.GetComponent<ControlledNoise>();
        loopInstances[index].StartClip();
    }

    private float maxVol = -1.0f;
    /// <summary>
    /// Sets the max volume for the looping noise.
    /// Set to "-1" if the max volume should not be changed from the prefab.
    /// </summary>
    public void SetMaxVolume(float newMax)
    {
        maxVol = newMax;
    }

    /// <summary>
    /// Starts the repeating loop, if it hasn't started already.
    /// </summary>
    public void StartLoop()
    {
        if (Loop == null || Running)
        {
            return;
        }

        loopsSoFar = 0;

        loopInstances[0] = null;
        loopInstances[1] = null;

        MakeCopy(0);

        Running = true;

        //if (loopInstances[0] != null)
        //{
        //    VolumeMultiplier = loopInstances[0].MaxVolume;
        //}
    }

    /// <summary>
    /// Updates the repeating loop.
    /// Throws an InvalidOperationException if the loop isn't running.
    /// </summary>
    public void UpdateLoop()
    {
        if (Loop == null || !Running)
            return;


        //If the first noise is done playing, remove it.
        if (loopInstances[0] == null)
        {
            //Make a second noise if it doesn't already exist for whatever reason.
            if (loopInstances[1] == null)
            {
                MakeCopy(1);
            }

            //Switch the second noise into the first slot.
            loopInstances[0] = loopInstances[1];
            loopInstances[1] = null;
        }

        //Otherwise, if there is just one noise playing, see if it's starting to fade out.
        else if (loopInstances[1] == null)
        {
            if (loopInstances[0].TimeUntilFadeOutStart <= 0.0f)
            {
                MakeCopy(1);
            }
        }

        //Set the max volume.
        if (maxVol != -1.0f)
        {
            if (loopInstances[0] != null)
            {
                loopInstances[0].MaxVolume = maxVol;
            }
            if (loopInstances[1] != null)
            {
                loopInstances[1].MaxVolume = maxVol;
            }
        }

        //Set the volume.
        //if (loopInstances[0] != null)
        //{
        //    loopInstances[0].MaxVolume = VolumeMultiplier;
        //}
        //if (loopInstances[1] != null)
        //{
        //    loopInstances[1].MaxVolume = VolumeMultiplier;
        //}
    }

    /// <summary>
    /// Ends the repeating loop if it was running.
    /// </summary>
    public void EndLoop()
    {
        if (Loop == null)
        {
            return;
        }

        if (loopInstances[0] != null)
        {
            GameObject.Destroy(loopInstances[0].gameObject);
        }
        if (loopInstances[1] != null)
        {
            GameObject.Destroy(loopInstances[1].gameObject);
        }

        Running = false;
    }
}