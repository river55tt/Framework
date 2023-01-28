using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanBullet : MonoBehaviour
{
    public float size;
    public float speed;
    public GameObject bulletobject;
    public Transform bullet;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        bullet.transform.localScale = new Vector3(400, size, 400);
        size -= speed * Time.deltaTime;
        if(size<=0)
        {
            Destroy(bulletobject);
        }
    }
}
