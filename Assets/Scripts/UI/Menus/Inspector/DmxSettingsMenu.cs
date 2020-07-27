using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Dmx;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class DmxSettingsMenu : Menu
    {
        [SerializeField] IntField startUniverse     = null;
        [SerializeField] IntField startChannel      = null;
        [SerializeField] Dropdown divisionDropdown  = null;
        [SerializeField] Dropdown protocolDropdown  = null;
        [SerializeField] Dropdown formatDropdown    = null;
        [SerializeField] Button setButton           = null;
        [Space(5)]
        [SerializeField] int stackIncreasement = 4;

        Dictionary<Lamp, DmxSettings> lampToSettings = new Dictionary<Lamp, DmxSettings>();

        int prevSelectedCount = 0;

        public override void Start()
        {
            base.Start();
            SubscribeToEvents();
        }

        void Update()
        {
            setButton.interactable = WorkspaceUtils.AtLastOneLampSelected;
        }

        void SubscribeToEvents()
        {
            startUniverse.onChanged += FieldValueChanged;
            startChannel.onChanged += FieldValueChanged;
            divisionDropdown.onValueChanged.AddListener(FieldValueChanged);
            protocolDropdown.onValueChanged.AddListener(ProtocolChanged);
            formatDropdown.onValueChanged.AddListener(FieldValueChanged);
        }

        void UnsubscribeFromEvents()
        {
            startUniverse.onChanged -= FieldValueChanged;
            startChannel.onChanged -= FieldValueChanged;
            divisionDropdown.onValueChanged.RemoveListener(FieldValueChanged);
            protocolDropdown.onValueChanged.RemoveListener(ProtocolChanged);
            formatDropdown.onValueChanged.RemoveListener(FieldValueChanged);
        }

        void FieldValueChanged(int value) => RecalculateChannelsAndUniverses();

        void ProtocolChanged(int value)
        {
            startUniverse.min = value;
            if (value == 1 && startUniverse.Value == 0)
                startUniverse.SetValue(1);
            FieldValueChanged(value);
        }

        public void Set()
        {
            foreach (var lamp in lampToSettings.Keys)
            {
                var settings = lampToSettings[lamp];
                var packet = new SetDmxModePacket(
                    settings.enabled,
                    settings.universe,
                    settings.channel,
                    settings.division,
                    settings.protocol,
                    settings.format
                );
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
            }
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.Clear();
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            LampItemView.ShowOrder = true;
            NetUtils.VoyagerClient.onReceived += OnReceived;
        }

        internal override void OnHide()
        {
            NetUtils.VoyagerClient.onReceived -= OnReceived;
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
            LampItemView.ShowOrder = false;
            WorkspaceUtils.LampItems.ForEach(view => view.SetPrefix(""));
            WorkspaceUtils.LampItems.ForEach(view => view.SetSuffix(""));
        }

        void SelectionChanged()
        {
            RecalculateChannelsAndUniverses();
            //CheckIfFirstSelected(WorkspaceUtils.SelectedLampItems);
        }

        void RecalculateChannelsAndUniverses(bool updateViewInfo = true)
        {
            if (updateViewInfo)
                WorkspaceUtils.LampItems.ForEach(view => view.SetSuffix(""));

            lampToSettings.Clear();

            int universe = startUniverse.Value;
            int channel = startChannel.Value - 1;
            var protocol = (DmxProtocol)protocolDropdown.value;
            var format = (DmxFormat)formatDropdown.value;

            foreach (var view in WorkspaceUtils.SelectedLampItemsInOrder)
            {
                Lamp lamp = view.lamp;

                int stack = StackSizeOfLamp(lamp);

                if (channel + stack > 512)
                {
                    universe++;
                    channel = 0;
                }

                DmxSettings settings = new DmxSettings
                {
                    enabled = true,
                    universe = universe,
                    channel = channel,
                    division = StackSizeOfLamp(lamp) / stackIncreasement,
                    protocol = protocol,
                    format = format
                };

                lampToSettings.Add(lamp, settings);

                if (updateViewInfo)
                    SetLampInfo(view, settings);

                channel += stack;
            }
        }

        void CheckIfFirstSelected(List<LampItemView> selected)
        {
            if (selected.Count == 1 && prevSelectedCount == 0)
                SetValuesFromLamp(selected[0]);
            prevSelectedCount = selected.Count;
        }

        void SetValuesFromLamp(LampItemView view)
        {
            if (view.lamp is VoyagerLamp lamp)
            {
                UnsubscribeFromEvents();
                startUniverse.SetValue(lamp.dmxUniverse);
                startChannel.SetValue(lamp.dmxChannel + 1);
                divisionDropdown.value = StackSizeToIndex(lamp.dmxDivision);
                protocolDropdown.value = (int)lamp.dmxProtocol;
                formatDropdown.value = (int)lamp.dmxFormat;
                SubscribeToEvents();
            }
        }

        void SetLampInfo(LampItemView view, DmxSettings settings)
        {
            if (view is VoyagerItemView voyagerView)
            {
                VoyagerLamp lamp = voyagerView.lamp;
                view.SetSuffix(
                    $"u {lamp.dmxUniverse}, c {lamp.dmxChannel + 1} / " +
                    $"u* {settings.universe}, c* {settings.channel + 1}"
                );
            }
        }

        void OnReceived(object sender, byte[] data)
        {
            var packet = Packet.Deserialize<DmxModeResponse>(data);
            if (packet != null && packet.op == OpCode.DmxModeResponse)
                RecalculateChannelsAndUniverses(false);
        }

        int StackSizeOfLamp(Lamp lamp)
        {
            switch (divisionDropdown.value)
            {
                case 0:
                    return stackIncreasement;
                case 1:
                    return 16 * stackIncreasement;
                case 2:
                    return lamp.pixels * stackIncreasement;
                default:
                    return 0;
            }
        }

        int StackSizeToIndex(int stackSize)
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
    }
}