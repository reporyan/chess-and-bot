using UnityEngine;
using UnityEngine.UI;

public class ResultBanner : MonoBehaviour
{
    public RectTransform rect;

    //lerp
    Vector3 targetSize;
    public float sizeSpeed;
    public float stoppingDistance;
    bool scaling;

    void Start()
    {
        targetSize = rect.localScale;

        rect.localScale = new Vector3(targetSize.x, 0, targetSize.y);

        rect.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!scaling)
            return;

        if (rect.localScale != targetSize)
        {
            rect.localScale = Vector3.Lerp(rect.localScale, targetSize, sizeSpeed * Time.deltaTime);

            if (Vector3.Distance(targetSize, rect.localScale) <= stoppingDistance)
            {
                rect.localScale = targetSize;
            }
        }
    }

    public void DisplayBanner()
    {
        scaling = true;
        rect.gameObject.SetActive(true);
    }
}
