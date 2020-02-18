using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI;
using VoyagerApp.Utilities;

public class UpdateDialog : Menu
{
    const string EXPLENTAION_TEXT = "Some lamps battery level is under 30%. Please connect the lamps to chargers to start updating: ";

    [SerializeField] Text _explenationText;

    List<VoyagerLamp> _lamps;
    int _prevCount;
    bool _identifyPressed;
    Action<VoyagerLamp> _onLampAdded;

    public void Show(List<VoyagerLamp> lamps, Action<VoyagerLamp> onLampAdded)
    {
        _lamps = lamps;
        _onLampAdded = onLampAdded;
        Open = true;
    }

    void Update()
    {
        if (Open)
        {
            CheckIfOkey();

            if (_lamps.Count != _prevCount) UpdateUI();
            if (_lamps.Count == 0) Open = false;
            if (_identifyPressed) IdentifyLamps();
        }
    }

    void CheckIfOkey()
    {
        foreach (var lamp in _lamps.ToArray())
        {
            if (lamp.charging || lamp.battery >= 30.0f)
            {
                _lamps.Remove(lamp);
                _onLampAdded?.Invoke(lamp);
            }
        }
    }

    void UpdateUI()
    {
        string[] lamps = new string[_lamps.Count];
        for (int i = 0; i < _lamps.Count; i++)
            lamps[i] = $"{_lamps[i].serial}({_lamps[i].battery}%)";
        _explenationText.text = EXPLENTAION_TEXT + string.Join(", ", lamps);
    }

    void IdentifyLamps()
    {
        var itshe = ApplicationSettings.IdentificationColor;
        var packet = new PixelOverridePacket(itshe, 0.3f);
        foreach (var lamp in WorkspaceUtils.SelectedLamps)
            NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
    }

    public void OnIdentifyDown() => _identifyPressed = true;
    public void OnIdentifyUp() => _identifyPressed = false;

    public void Cancel()
    {
        Open = false;
    }
}