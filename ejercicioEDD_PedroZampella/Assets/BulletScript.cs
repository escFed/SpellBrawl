using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] public bool hit = false;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] List<GameObject> bullets = new List<GameObject>();
    private PlayerScript player;



    private void Start()
    {
        player = FindAnyObjectByType<PlayerScript>();
    }




    public void Shoot(Vector3 direction, Vector3 spawnPos)
    {

      



            GameObject newBullet = Instantiate(bulletPrefab, spawnPos, transform.rotation);
            Rigidbody bulletRb = newBullet.GetComponent<Rigidbody>();
            bulletRb.useGravity = false;
            bulletRb.linearVelocity = Vector3.up * speed;
            bullets.Add(newBullet);





        
    }
}
