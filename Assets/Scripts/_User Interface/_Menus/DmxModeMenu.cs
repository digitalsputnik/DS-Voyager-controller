using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Dmx;
using DigitalSputnik.Voyager;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class DmxModeMenu : Menu
    {
        private const float UPDATE_TEXT_RATE = 2.0f;

        [SerializeField] private IntField _startUniverse = null;
        [SerializeField] private IntField _startChannel = null;
        [SerializeField] private Dropdown _divisionDropdown = null;
        [SerializeField] private Dropdown _protocolDropdown = null;
        [SerializeField] private Dropdown _formatDropdown = null;
        [SerializeField] private Button _setButton = null;
        [Space(5)]
        [SerializeField] private int _stackIncreasement = 4;

        private readonly Dictionary<VoyagerLamp, DmxSettings> _lampToSettings = new Dictionary<VoyagerLamp, DmxSettings>();

        private int _prevSelectedCount = 0;
        private float _prevTextUpdate = 0.0f;
        
        public void Set()
        {
            foreach (var lamp in _lampToSettings.Keys)
            {
                var settings = _lampToSettings[lamp];
                lamp.ActivateDmxMode(settings);
            }

            Project.AutoSave();
        }
        
        public override void Start()
        {
            base.Start();
            SubscribeToEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
                    Debug.Log(JsonConvert.SerializeObject(item.LampHandle.DmxSettings, Formatting.Indented));
            }

            UpdateLampsInfo();
        }

        private void UpdateLampsInfo()
        {
            if (Time.time - _prevTextUpdate >= UPDATE_TEXT_RATE && VoyagerItem.ShowOrderNumber && !ShowInfo.Active)
            {
                foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
                {
                    var lamp = item.LampHandle;
                    if (_lampToSettings.ContainsKey(lamp))
                    {
                        var settings = _lampToSettings[lamp];
                        SetLampInfo(item, settings);
                    }
                }
                _prevTextUpdate = Time.time;
            }
        }

        internal override void OnShow()
        {
            WorkspaceSelection.Clear();
            WorkspaceSelection.SelectionChanged += SelectionChanged;
            VoyagerItem.ShowOrderNumber = true;
            
            // NetUtils.VoyagerClient.onReceived += OnReceived;

            RecalculateChannelsAndUniverses();
        }

        internal override void OnHide()
        {
            // NetUtils.VoyagerClient.onReceived -= OnReceived;

            VoyagerItem.ShowOrderNumber = false;
            WorkspaceSelection.SelectionChanged -= SelectionChanged;
            WorkspaceManager.GetItems<VoyagerItem>().ToList().ForEach(view => view.Prefix = "");
            WorkspaceManager.GetItems<VoyagerItem>().ToList().ForEach(view => view.Suffix = "");
        }

        private void SubscribeToEvents()
        {
            _startUniverse.OnChanged += FieldValueChanged;
            _startChannel.OnChanged += FieldValueChanged;
            _divisionDropdown.onValueChanged.AddListener(FieldValueChanged);
            _protocolDropdown.onValueChanged.AddListener(ProtocolChanged);
            _formatDropdown.onValueChanged.AddListener(FieldValueChanged);
        }    

        private void UnsubscribeFromEvents()
        {
            _startUniverse.OnChanged -= FieldValueChanged;
            _startChannel.OnChanged -= FieldValueChanged;
            _divisionDropdown.onValueChanged.RemoveListener(FieldValueChanged);
            _protocolDropdown.onValueChanged.RemoveListener(ProtocolChanged);
            _formatDropdown.onValueChanged.RemoveListener(FieldValueChanged);
        }

        private void FieldValueChanged(int value) => RecalculateChannelsAndUniverses();

        private void ProtocolChanged(int value)
        {
            _startUniverse.Min = value;

            switch (value)
            {
                case 0:
                    _startUniverse.Max = 32767;
                    if (_startUniverse.Value > 32767)
                        _startUniverse.SetValue(32767);
                    break;

                default:
                    _startUniverse.Max = 63999;
                    if (_startUniverse.Value == 0)
                        _startUniverse.SetValue(1);
                    break;
            }

            FieldValueChanged(value);
        }

        private void SelectionChanged() => RecalculateChannelsAndUniverses();

        private void RecalculateChannelsAndUniverses(bool updateViewInfo = true)
        {
            if (updateViewInfo)
                WorkspaceManager.GetItems<VoyagerItem>().ToList().ForEach(l => l.Suffix = "");

            _lampToSettings.Clear();

            var universe = _startUniverse.Value;
            var channel = _startChannel.Value - 1;
            var protocol = (DmxProtocol)_protocolDropdown.value;
            var format = (DmxFormat)_formatDropdown.value;

            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                var lamp = item.LampHandle;
                var stack = StackSizeOfLamp(lamp);

                if (channel + stack > 512)
                {
                    universe++;
                    channel = 0;
                }

                var settings = new DmxSettings
                {
                    Universe = universe,
                    Channel = channel,
                    Division = StackSizeOfLamp(lamp) / _stackIncreasement,
                    Protocol = protocol,
                    Format = format
                };

                _lampToSettings.Add(lamp, settings);

                if (updateViewInfo) SetLampInfo(item, settings);

                channel += stack;
            }

            Project.AutoSave();
        }

        private void SetLampInfo(VoyagerItem item, DmxSettings settings)
        {
            var lamp = item.LampHandle;
            item.Suffix = $"u {lamp.DmxSettings.Universe}, c {lamp.DmxSettings.Channel + 1} / " +
                          $"u* {settings.Universe}, c* {settings.Channel + 1}";
        }

        private int StackSizeOfLamp(VoyagerLamp lamp)
        {
            switch (_divisionDropdown.value)
            {
                case 0:
                    return _stackIncreasement;
                case 1:
                    return 16 * _stackIncreasement;
                case 2:
                    return lamp.PixelCount * _stackIncreasement;
                case 3:
                    return 2 * _stackIncreasement;
                default:
                    return 0;
            }
        }

        private int StackSizeToIndex(int stackSize)
        {
            switch (stackSize)
            {
                case 1:
                    return 0;
                case 16:
                    return 1;
                default:
                    return 2;
            }
        }

        /*
        private void SetValuesFromLamp(VoyagerItem item)
        {
            var lamp = item.LampHandle;
            
            UnsubscribeFromEvents();
            _startUniverse.SetValue(lamp.DmxSettings.Universe);
            _startChannel.SetValue(lamp.DmxSettings.Channel + 1);
            _divisionDropdown.value = StackSizeToIndex(lamp.DmxSettings.Division);
            _protocolDropdown.value = (int)lamp.DmxSettings.Protocol;
            _formatDropdown.value = (int)lamp.DmxSettings.Format;
            SubscribeToEvents();
        }
        */
    }
}