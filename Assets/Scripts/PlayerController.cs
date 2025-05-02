using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator _animator;
    private float velocity = 0f;
    [SerializeField] private float acceleration = 0.1f;

    private int _velocityHash;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _velocityHash = Animator.StringToHash("Velocity");
    }

    void Update()
    {
        bool forwardPrs = Input.GetKey("w");
        bool runPrs = Input.GetKey("left shift");

        if (forwardPrs)
        {
            velocity += Time.deltaTime * acceleration;
        }
        _animator.SetFloat(_velocityHash, velocity);

        if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("saving");
            SaveSystem.SaveGame();
            Debug.Log("Saved!");
        }

        if (Input.GetKey(KeyCode.L))
        {
            Debug.Log("Loading");
            SaveSystem.LoadGame();
            Debug.Log("Loaded");
        }

        if (Input.GetKey(KeyCode.P))
        {
            List<MetaData> l = SaveSystem.SaveMetaDataManager.ListAllSaveMetaDatas();
            foreach (MetaData v in l)
            {
                Debug.Log(v.saveName);
            }
        }
    }
}
