using System.Linq;
using UnityEngine;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class ChoiceDivider : Tutorial
    {
        [SerializeField] int ifBleOne;
        [SerializeField] int ifBleFive;
        [SerializeField] int ifBleMore;
        [SerializeField] int ifWifiOne;
        [SerializeField] int ifWifiTen;
        [SerializeField] int ifWifiMore;

        [SerializeField] int ifBleOneWithClientOption;
        [SerializeField] int ifBleFiveWithClientOption;
        [SerializeField] int ifBleMoreWithClientOption;
        [SerializeField] int ifWifiOneWithClientOption;
        [SerializeField] int ifWifiTenWithClientOption;
        [SerializeField] int ifWifiMoreWithClientOption;

        public override void CheckForAction()
        {
            if (!TutorialManager.setClientPicked)
            {
                switch (TutorialManager.Choice)
                {
                    case TutorialManager.TutorialChoice.BleOne:
                        TutorialManager.Instance.SetNextTutorial(ifBleOne);
                        Debug.Log(ifBleOne);
                        break;
                    case TutorialManager.TutorialChoice.BleFive:
                        TutorialManager.Instance.SetNextTutorial(ifBleFive);
                        Debug.Log(ifBleFive);
                        break;
                    case TutorialManager.TutorialChoice.BleMore:
                        TutorialManager.Instance.SetNextTutorial(ifBleMore);
                        Debug.Log(ifBleMore);
                        break;
                    case TutorialManager.TutorialChoice.WifiOne:
                        TutorialManager.Instance.SetNextTutorial(ifWifiOne);
                        Debug.Log(ifWifiOne);
                        break;
                    case TutorialManager.TutorialChoice.WifiTen:
                        TutorialManager.Instance.SetNextTutorial(ifWifiTen);
                        Debug.Log(ifWifiTen);
                        break;
                    case TutorialManager.TutorialChoice.WifiMore:
                        TutorialManager.Instance.SetNextTutorial(ifWifiMore);
                        Debug.Log(ifWifiMore);
                        break;
                }
            }
            else
            {
                switch (TutorialManager.Choice)
                {
                    case TutorialManager.TutorialChoice.BleOne:
                        TutorialManager.Instance.SetNextTutorial(ifBleOneWithClientOption);
                        Debug.Log(ifBleOneWithClientOption);
                        break;
                    case TutorialManager.TutorialChoice.BleFive:
                        TutorialManager.Instance.SetNextTutorial(ifBleFiveWithClientOption);
                        Debug.Log(ifBleFiveWithClientOption);
                        break;
                    case TutorialManager.TutorialChoice.BleMore:
                        TutorialManager.Instance.SetNextTutorial(ifBleMoreWithClientOption);
                        Debug.Log(ifBleMoreWithClientOption);
                        break;
                    case TutorialManager.TutorialChoice.WifiOne:
                        TutorialManager.Instance.SetNextTutorial(ifWifiOneWithClientOption);
                        Debug.Log(ifWifiOneWithClientOption);
                        break;
                    case TutorialManager.TutorialChoice.WifiTen:
                        TutorialManager.Instance.SetNextTutorial(ifWifiTenWithClientOption);
                        Debug.Log(ifWifiTenWithClientOption);
                        break;
                    case TutorialManager.TutorialChoice.WifiMore:
                        TutorialManager.Instance.SetNextTutorial(ifWifiMoreWithClientOption);
                        Debug.Log(ifWifiMoreWithClientOption);
                        break;
                }
            }
        }
    }
}

