using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TapDetection : MonoBehaviour
{
    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            Vector3 screenPoint = new Vector3(touch.position.x, touch.position.y, 0);
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "ar1" || hit.collider.tag == "ar2") {
                if (hit.collider.tag == "ar1") {
                    hit.collider.GetComponent<BehaviourScript>().Selected();
                    GameObject.FindGameObjectWithTag("ar2").GetComponent<BehaviourScript>().notSelected();
                } else {
                    hit.collider.GetComponent<BehaviourScript>().Selected();
                    GameObject.FindGameObjectWithTag("ar1").GetComponent<BehaviourScript>().notSelected();
                }

            }
        }
    }
}
