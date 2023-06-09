﻿using UnityEngine;
using System;
using System.Collections;

public class EasingControl : MonoBehaviour
{
    #region Events
    public event EventHandler updateEvent;
    public event EventHandler stateChangeEvent;
    public event EventHandler completedEvent;
    public event EventHandler loopedEvent;
    #endregion

    #region Enums
    public enum TimeType
    {
        Normal,
        Real,
        Fixed,
    };

    public enum PlayState
    {
        Stopped,
        Paused,
        Playing,
        Reversing,
    };

    public enum EndBehaviour
    {
        Constant,
        Reset,
    };

    public enum LoopType
    {
        Repeat,
        PingPong,
    };
    #endregion

    #region Properties
    public TimeType timeType = TimeType.Normal;
    public PlayState playState { get; private set; }
    public PlayState previousPlayState { get; private set; }
    public EndBehaviour endBehaviour = EndBehaviour.Constant;
    public LoopType loopType = LoopType.Repeat;
    public bool IsPlaying { get { return playState == PlayState.Playing || playState == PlayState.Reversing; } }

    public float startValue = 0.0f;
    public float endValue = 1.0f;
    public float duration = 1.0f;
    public int loopCount = 0;
    public Func<float, float, float, float> equation = EasingEquations.Linear;

    public float currentTime { get; private set; }
    public float currentValue { get; private set; }
    public float currentOffset { get; private set; }
    public int loops { get; private set; }
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        Resume();
    }

    private void OnDisable()
    {
        Pause();
    }
    #endregion

    #region Public
    public void Play()
    {
        SetPlayState(PlayState.Playing);
    }

    public void Reverse()
    {
        SetPlayState(PlayState.Reversing);
    }

    public void Pause()
    {
        SetPlayState(PlayState.Paused);
    }

    public void Resume()
    {
        SetPlayState(previousPlayState);
    }

    public void Stop()
    {
        SetPlayState(PlayState.Stopped);
        loops = 0;
        if (endBehaviour == EndBehaviour.Reset)
            SeekToBeginning();
    }

    public void SeekToTime(float time)
    {
        currentTime = Mathf.Clamp01(time / duration);
        float newValue = ((endValue - startValue) * currentTime) + startValue;
        currentOffset = newValue - currentValue;
        currentValue = newValue;

        updateEvent?.Invoke(this, EventArgs.Empty);
    }

    public void SeekToBeginning()
    {
        SeekToTime(0.0f);
    }

    public void SeekToEnd()
    {
        SeekToTime(duration);
    }
    #endregion

    #region Private
    private void SetPlayState(PlayState target)
    {
        if (playState == target)
            return;

        previousPlayState = playState;
        playState = target;

        stateChangeEvent?.Invoke(this, EventArgs.Empty);

        StopCoroutine(nameof(Ticker));
        if (IsPlaying)
            StartCoroutine(nameof(Ticker));
    }

    private IEnumerator Ticker()
    {
        while (true)
        {
            switch (timeType)
            {
                case TimeType.Normal:
                    yield return new WaitForEndOfFrame();
                    Tick(Time.deltaTime);
                    break;
                case TimeType.Real:
                    yield return new WaitForEndOfFrame();
                    Tick(Time.unscaledDeltaTime);
                    break;
                default: // Fixed
                    yield return new WaitForFixedUpdate();
                    Tick(Time.fixedDeltaTime);
                    break;
            }
        }
    }

    private void Tick(float time)
    {
        bool finished;
        if (playState == PlayState.Playing)
        {
            currentTime = Mathf.Clamp01(currentTime + (time / duration));
            finished = Mathf.Approximately(currentTime, 1.0f);
        }
        else // Reversing
        {
            currentTime = Mathf.Clamp01(currentTime - (time / duration));
            finished = Mathf.Approximately(currentTime, 0.0f);
        }

        float frameValue = ((endValue - startValue) * equation(0.0f, 1.0f, currentTime)) + startValue;
        currentOffset = frameValue - currentValue;
        currentValue = frameValue;

        updateEvent?.Invoke(this, EventArgs.Empty);

        if (finished)
        {
            ++loops;
            if (loopCount < 0 || loopCount >= loops)
            {
                if (loopType == LoopType.Repeat)
                    SeekToBeginning();
                else // PingPong
                    SetPlayState(playState == PlayState.Playing ? PlayState.Reversing : PlayState.Playing);

                loopedEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                completedEvent?.Invoke(this, EventArgs.Empty);

                Stop();
            }
        }
    }
    #endregion
}
