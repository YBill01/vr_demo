using System;
using System.Collections;
using unity4dv;
using UnityEngine;

public class PlayVVOnDateManager : MonoBehaviour
{
    [SerializeField] private Plugin4DS _plugin4Ds;

    private DateTime _startDateTime;

    private void OnEnable()
    {
        _startDateTime = DateTime.Now;
        if (_startDateTime.Second > 0)
        {
            var minute = _startDateTime.Minute % 2 == 0 ? _startDateTime.Minute + 2 : _startDateTime.Minute + 1;
            _startDateTime = new DateTime(_startDateTime.Year, _startDateTime.Month, _startDateTime.Day,
                _startDateTime.Hour, minute, 0);
        }
    }

    private void Update()
    {
        var second = (int)(_startDateTime - DateTime.Now).TotalSeconds;
        var minute = (int)(_startDateTime - DateTime.Now).TotalMinutes;
        DebugText.Instance.SetText("Time to play VV: " + minute.ToString("00") + ":" + (second - (minute*60)).ToString("00"));
        if ((_startDateTime - DateTime.Now) < TimeSpan.FromSeconds(1))
        {
            DebugText.Instance.SetText(true.ToString());
            _plugin4Ds.Play(true);
            _startDateTime += TimeSpan.FromMinutes(6);
        }
    }
}
