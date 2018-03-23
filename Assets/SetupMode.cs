using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupMode : MonoBehaviour {

    [Header("Page state")]
    public bool active;
    public bool mouseCheck;

    [Header("Tools")]
    public DrawMode drawTool;

    [Header("Buttons")]
    public GameObject SetupButtons;
    public GameObject AddButton;
    public GameObject RemoveButton;

    [Header("External buttons")]
    public GameObject SetupToggle;
    
    [Header("Lamps")]
    public GameObject LampTemplates; //NOTE: For use later, when we have a dialog box or lamp recognition.
    public GameObject ShortVoyager;

    [Header("Draw panel")]
    public GameObject Workspace;

    private bool AddLampMode = false;
    private GameObject AddedLamp = null;

    // Use this for initialization
    void Start () {
        this.gameObject.SetActive(active);
        //TODO: Set active all drag and drop controls!
    }

    // Update is called once per frame
    void Update()
    {
        if (AddLampMode)
        {
            AddedLamp.transform.position = GetMouseLampPosition();
        }
        
        //TODO: GetMouseButton with drawing and GetMouseButtonDown with adding!
        //Check for mouse down and ray hit
        if (!Input.GetMouseButtonDown(0))
            return;

        //clicking on lights
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (AddLampMode)
            {
                AddLampMode = false;
                AddedLamp.transform.SetParent(Workspace.transform);
            }
            
            //if (hit.transform.gameObject == AddButton)
            //{
            //    //TODO: If add lamp button is pressed:
            //    //Display dialog window for:
            //    //-Short/long Voyager
            //    //-IP
            //    //-Port
            //    //Create new lamp
            //    //NOTE: Currently creates long Voyager lamp.
            //    //Activate lamp adding mode after dialog box and add lamp.

            //    //TODO: Create with custom IP, port and option to choose lamp type.
            //    GameObject selectedLamp = ShortVoyager; //Not implemented!

            //    //Create lamp
            //    Vector3 CreatedLampPosition = hit.point;
            //    //All Voyagers are added to plane z = 1
            //    CreatedLampPosition.z = 1;

            //    var newVoyager = Instantiate(selectedLamp, CreatedLampPosition, Quaternion.identity);
            //    newVoyager.transform.SetParent(Workspace.transform);
            //    newVoyager.GetComponent<Ribbon>().IP = "0.0.0.0";
            //    //-Drop lamp to canvas when mouse button is pressed
            //}

            if (hit.transform.gameObject == AddButton)
            {
                //TODO: display dialog for lamp creation
                //1st implementation: skip dialog box, add short 2ft Voyager and add it.
                AddLampMode = true;
                Vector3 position = GetMouseLampPosition();
                AddedLamp = Instantiate(ShortVoyager, position, Quaternion.identity);

                //Set its properties
                AddedLamp.GetComponent<Ribbon>().IP = "0.0.0.0";

            }

            if (hit.transform.tag == "lamp")
            {
                //Activate drag and drop for current lamp...
                //hit.transform.parent.parent.gameObject.GetComponent<dragA>
            }

            if (hit.transform.gameObject == SetupToggle)
            {
                //Activate drawing and it's buttons
                drawTool.SetActive(true);
                SetupButtons.SetActive(false); //Is this necessary!?
                this.gameObject.SetActive(false);
            }

        }
    }

    private static Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        return new Vector3(hit.point.x, hit.point.y, 1);
    }

    public void SetActive(bool inVal)
    {
        this.gameObject.SetActive(inVal);
        if (SetupButtons != null)
            SetupButtons.SetActive(inVal);
        active = inVal;
    }
}
