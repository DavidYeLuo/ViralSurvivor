using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenCameraMover : MonoBehaviour
{
    [SerializeField] private float speed = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0f, 0f, speed * Time.deltaTime);
        if (transform.position.z > 90f)
        {
            transform.position = new Vector3(0f, 0.384f, 0.506f);
        }
    }
}
