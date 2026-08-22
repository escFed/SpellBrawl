using UnityEngine;

public class ShadowProjectile : MonoBehaviour
{
    public float lifeTime = 1.0f;
    void Start() => Destroy(gameObject, lifeTime);
}
