using UnityEngine;

[ExecuteInEditMode]
public class BgFollow : MonoBehaviour
{

    [SerializeField] GameObject Camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Camera.transform.position.x, transform.position.y, transform.position.z);
    }
}
