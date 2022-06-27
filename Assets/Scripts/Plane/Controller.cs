using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    string gearAndTryTurnMessage = "Retract gear before making maneuvers";
    PlaneController planeController;

    void Start()
    {
        planeController = GetComponent<PlaneController>();
    }

    void Update()
    {
        Controls();
    }

    void Controls()
    {
        if (Input.GetMouseButton(0)) planeController.Shoot();
        if (Input.GetKeyDown(KeyCode.F)) planeController.SwitchFlaps();

        if (Input.GetKey(KeyCode.W)) planeController.IncreaseThrottle();

        if (Input.GetKey(KeyCode.S)) planeController.DecreaseThrottle();

        if (Input.GetKeyDown(KeyCode.B)) planeController.SwitchBrakes();

        if (Input.GetKey(KeyCode.Q)) if (!planeController.GearUp) GearWarning(); else planeController.Roll();

        if (Input.GetKey(KeyCode.E)) if (!planeController.GearUp) GearWarning(); else planeController.TurnBack();

        if (Input.GetKey(KeyCode.G)) planeController.SwitchGear();



        if (Input.GetKeyDown(KeyCode.Space)) planeController.ThrowBomb();

        //float pitch = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        //planeController.SetPitch(pitch);

        SetHeading(GetLookPosition());

    }

    void GearWarning()
    {
        GameHandler.Instance.messageBox.ShowMessage(gearAndTryTurnMessage);
    }

    private void SetHeading(Vector3 point)
    {
        planeController.SetHeading((Camera.main.ScreenToWorldPoint(point) - transform.position).normalized);
    }

    public Vector2 GetLookPosition()
    {
        //result = new Vector2(inputInput.mousePosition;
        return Input.mousePosition;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawLine(transform.position, Camera.main.ScreenToWorldPoint(GetLookPosition()));
    //}
}
