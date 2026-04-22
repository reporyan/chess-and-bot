using UnityEngine;

public class CanGoDot : MonoBehaviour
{
    Transform trans;

    //lerp
    Vector3 targetSize;
    public float sizeSpeed;
    public float stoppingDistance;

    void Start()
    {
        trans = transform;
        targetSize = trans.localScale;

        trans.localScale = Vector3.zero;
    }

    void Update()
    {
        if (trans.localScale != targetSize)
        {
            trans.localScale = Vector3.Lerp(trans.localScale, targetSize, sizeSpeed * Time.deltaTime);

            if (Vector3.Distance(targetSize, trans.localScale) <= stoppingDistance)
            {
                trans.localScale = targetSize;
            }
        }
    }
}
