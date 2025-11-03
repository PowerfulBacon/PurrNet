using System;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    private SyncTimer _timer = new(manualUpdate:true);

    private void OnEnable()
    {
        _timer.onTimerSecondTick += OnTimerTick;
    }

    private void OnDisable()
    {
        _timer.onTimerSecondTick -= OnTimerTick;
    }

    private void OnTimerTick()
    {
        Debug.Log($"{_timer.remainingInt}");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            _timer.StartTimer(10);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            _timer.PauseTimer(true);
        if(Input.GetKeyDown(KeyCode.Alpha3))
            _timer.ResumeTimer();
        
        _timer.Advance(Time.deltaTime);
    }
}
