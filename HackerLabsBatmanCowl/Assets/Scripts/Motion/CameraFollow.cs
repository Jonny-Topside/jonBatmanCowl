using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float maxSpeed = 0;
    public float smoothTime = 0;

    private Vector3 initialOffset = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
   
                    
    
    private void Start()
    {
        initialOffset = transform.position;
    }

    private void Update()
    {
        Transform cam = Camera.main.transform;

        Vector3 rotatedOffset = cam.right * initialOffset.x + cam.up * initialOffset.y + cam.forward * initialOffset.z;
        Vector3 targetPosition = cam.position + cam.forward + rotatedOffset;
        if(this.gameObject.tag == "plane")
        {

        //    Transform testTrans = cam;

//     Vector3 testTransVec = new Vector3(cam.rotation.x + 30.0f, cam.rotation.y, cam.rotation.z);
//
//     transform.LookAt(testTrans);
//     gameObject.transform.Rotate(testTransVec);
        }
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref velocity, smoothTime, maxSpeed);

        transform.LookAt(cam);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - cam.eulerAngles.z);

        //if(GetComponent<TextMesh>())
        //{
        //    Vector3 scale = transform.localScale;
        //    scale.x *= -1;
        //    transform.localScale = scale;
        //}
    }
}