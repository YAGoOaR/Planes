using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInactiveWhileUnderwater : MonoBehaviour
{
    [SerializeField] ParticleSystem componentToDisable;
    [SerializeField] float altThreshold = -10;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (componentToDisable == null) return;
        if(componentToDisable.isEmitting && transform.position.y < altThreshold)
        {
            componentToDisable.Stop();
        }
        if (!componentToDisable.isEmitting && transform.position.y > altThreshold)
        {
            componentToDisable.Play();
        }
    }
}
