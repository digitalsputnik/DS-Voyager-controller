using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Dmx;
using VoyagerApp.Lamps;
using VoyagerApp.Networking;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class DmxSettingsMenu : Menu
    {
        [SerializeField] Toggle enableToggle    = null;
        [SerializeField] IntField startUniverse = null;
        [SerializeField] IntField startChannel  = null;
        [SerializeField] Dropdown division      = null;
        [SerializeField] Dropdown protocol      = null; 
        [Space(5)]
        [SerializeField] int stackIncreasement = 4;

        Dictionary<Lamp, DmxSettings> lampToSettings = new Dictionary<Lamp, DmxSettings>();

        int prevSelectedCount = 0;

        public override void Start()
        {
            base.Start();
            SubscribeToEvents();
        }

        void SubscribeToEvents()
        {
            enableToggle.onValueChanged.AddListener(BoolValueChanged);
            startUniverse.onChanged += IntValueChanged;
            startChannel.onChanged += IntValueChanged;
            division.onValueChanged.AddListener(IntValueChanged);
            protocol.onValueChanged.AddListener(IntValueChanged);
        }

        void UnsubscribeFromEvents()
        {
            enableToggle.onValueChanged.RemoveListener(BoolValueChanged);
            startUniverse.onChanged -= IntValueChanged;
            startChannel.onChanged -= IntValueChanged;
            division.onValueChanged.RemoveListener(IntValueChanged);
            protocol.onValueChanged.RemoveListener(IntValueChanged);
        }

        void BoolValueChanged(bool value) => RecalculateChannelsAndUniverses();
        void IntValueChanged(int value) => RecalculateChannelsAndUniverses();

        public void Set()
        {
            VoyagerClient client = NetUtils.VoyagerClient;
            foreach (var lamp in lampToSettings)
                client.SendDmxSettings(lamp.Key.address, lamp.Value);
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.Clear();
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            WorkspaceSelection.instance.Enabled = true;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
            WorkspaceSelection.instance.Enabled = false;
        }

        void SelectionChanged(WorkspaceSelection selection)
        {
            RecalculateChannelsAndUniverses();
            CheckIfFirstSelected(selection.Selected);
            Set();
        }

        void RecalculateChannelsAndUniverses()
        {
            WorkspaceUtils.LampItems.ForEach(view => view.SetInfo(""));

            lampToSettings.Clear();

            int universe = startUniverse.Value;
            int channel = startChannel.Value;

            foreach (var view in WorkspaceSelection.instance.Selected)
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
                    enabled = enableToggle.isOn,
                    universe = universe,
                    channel = channel,
                    division = StackSizeOfLamp(lamp) / stackIncreasement,
                    protocol = protocol.value == 0 ? "ArtNet" : "sACN"
                };

                lampToSettings.Add(lamp, settings);
                SetLampInfo(view, settings);
                channel += stack;
            }

            Set();
        }

        void CheckIfFirstSelected(List<LampItemView> selected)
        {
            if (selected.Count == 1 && prevSelectedCount == 0)
                SetValuesFromLamp(selected[0]);
            prevSelectedCount = selected.Count;
        }

        void SetValuesFromLamp(LampItemView view)
        {
            if (view.lamp.dmx == null) return;

            UnsubscribeFromEvents();

            Lamp lamp = view.lamp;

            enableToggle.isOn = lamp.dmx.enabled;
            startUniverse.SetValue(lamp.dmx.universe);
            startChannel.SetValue(lamp.dmx.channel);
            division.value = StackSizeToIndex(lamp.dmx.division);
            protocol.value = lamp.dmx.protocol == "ArtNet" ? 0 : 1;

            SubscribeToEvents();
        }

        void SetLampInfo(LampItemView view, DmxSettings settings)
        {
            view.SetInfo($"u {settings.universe}, c {settings.channel}");
        }

        int StackSizeOfLamp(Lamp lamp)
        {
            switch (division.value)
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