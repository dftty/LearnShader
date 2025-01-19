using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string a = "World";
        TestRef(a);
        Debug.Log(a);
        // this.gameObject.co
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.name + " entered the trigger zone");
    }

    void TestRef(string a) {
        a = "Hello";
    }
}
