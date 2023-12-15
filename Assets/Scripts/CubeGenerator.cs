using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    [SerializeField]
    private int createAmount;

    [SerializeField]
    private float radius;


    private void Awake()
    {
        for (int i = 0; i < createAmount; i++)
        {
            GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObj.transform.SetParent(transform);
            newObj.transform.position = Random.insideUnitSphere * radius;
        }
    }
}
