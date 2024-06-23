using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BgFollow : MonoBehaviour
{

    [SerializeField] GameObject Layer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Layer.transform.position = new Vector3(transform.position.x, Layer.transform.position.y, Layer.transform.position.z);
    }
}
