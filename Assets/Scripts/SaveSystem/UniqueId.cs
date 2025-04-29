using UnityEngine;
using System;
using Unity.Collections;

[ExecuteAlways]
public class UniqueId : MonoBehaviour
{
    [ReadOnly] [SerializeField]
    private string id;

    public string Id
    {
        get { return id; }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
            GenerateId();
    }
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            GenerateId();
    }

    private void GenerateId()
    {
        this.id = Guid.NewGuid().ToString();
    }

}
