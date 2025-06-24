using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    //[Range(-1f, 1f)]
    //public float scrollSpeed = 0.5f;
    //private float offset = 0f;
    //private Material backgroundMat;

    //void Start()
    //{
    //    backgroundMat = GetComponent<Renderer>().material;
    //}

    //void Update()
    //{
    //    offset += (Time.deltaTime * scrollSpeed) / 10f; 
    //    backgroundMat.SetTextureOffset("_MainTex", new Vector2(offset, 0f));

    //}
    private float startPos, length;
    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position.x;   
        length = GetComponent<SpriteRenderer>().bounds.size.x; // Get the width of the background sprite

    }
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
