using DigitalSputnik;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController;
using VoyagerController.UI;
using VoyagerController.Workspace;

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
            CheckIfOkay();

            if (_lamps.Count != _prevCount) UpdateUI();
            if (_lamps.Count == 0) Open = false;
            if (_identifyPressed) IdentifyLamps();
        }
    }

    void CheckIfOkay()
    {
        foreach (var lamp in _lamps.ToArray())
        {
            if (lamp.Charging || lamp.BatteryLevel >= 30.0f)
            {
                _onLampAdded?.Invoke(lamp);
                _lamps.Remove(lamp);
            }
        }
    }

    void UpdateUI()
    {
        string[] lamps = new string[_lamps.Count];
        for (int i = 0; i < _lamps.Count; i++)
            lamps[i] = $"{_lamps[i].Serial}({_lamps[i].BatteryLevel}%)";
        _explenationText.text = EXPLENTAION_TEXT + string.Join(", ", lamps);
    }

    void IdentifyLamps()
    {
        var itshe = ApplicationSettings.IdentificationColor;
        var packet = new PixelOverridePacket(itshe, 0.3f);

        foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().Where(
            l => l.LampHandle.BatteryLevel < 30.0 && 
            !l.LampHandle.Charging && 
            l.LampHandle.Endpoint is LampNetworkEndPoint && 
            LampIsOlder(l.LampHandle)
            ))
            lamp.LampHandle.OverridePixels(ApplicationSettings.IdentificationColor, 0.3f);
    }

    bool LampIsOlder(VoyagerLamp lamp)
    {
        Version lampVersion = new Version(lamp.Version);
        Version softwareVersion = new Version(VoyagerUpdater.Version);
        return lampVersion.CompareTo(softwareVersion) < 0;
    }

    public void OnIdentifyDown() => _identifyPressed = true;
    public void OnIdentifyUp() => _identifyPressed = false;

    public void Remove()
    {
        WorkspaceSelection.Clear();

        foreach (var lamp in _lamps)
        {
            var item = WorkspaceManager.GetItems<VoyagerItem>().FirstOrDefault(v => v.LampHandle == lamp);
            WorkspaceManager.RemoveItem(item);
        }

        Open = false;
    }
}