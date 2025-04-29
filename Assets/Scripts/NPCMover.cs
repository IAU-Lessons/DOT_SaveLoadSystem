using UnityEngine;

[System.Serializable]
public struct SaveData
{
    public Vector3 position;
    public int routIndex;
}
public class NPCMover : MonoBehaviour, ISaveable
{
    [SerializeField] private float moveSpeed = 6f;
    
    [SerializeField] private Transform[] moveRoute;
    [SerializeField] private float r_distance = 1f;
    
    private Transform _tr;
    private Vector3 _moveTarget;
    private int routeIndex = 0;

    private Animator _animator;
    
    void Start()
    {
        _tr = transform;
        _animator = GetComponent<Animator>();
        _tr.LookAt(moveRoute[routeIndex].position, Vector3.up);
        _moveTarget = moveRoute[routeIndex].position;
        _animator.SetBool("iswalk",true);
    }

    void Update()
    {
        if (routeIndex == moveRoute.Length)
        {
            routeIndex = 0;
        }
        
        if (Vector3.Distance(_moveTarget, _tr.position) < r_distance)
        {
            _tr.LookAt(moveRoute[routeIndex].position, Vector3.up);
            _moveTarget = new Vector3(moveRoute[routeIndex].position.x, 0, moveRoute[routeIndex].position.z);
            routeIndex++;
        }

        _tr.position += _tr.forward * moveSpeed * Time.deltaTime;
    }

    public object CaptureState()
    {
        return new SaveData()
        {
            position = this._tr.position,
            routIndex = this.routeIndex
        };
    }

    public void RestoreState(object state)
    {
        
        if(state == null)
            return;

        if (state is Newtonsoft.Json.Linq.JObject jObject)
        {
            SaveData data = jObject.ToObject<SaveData>();
            this._tr.position = data.position;
            this.routeIndex = data.routIndex;
        }
        else
        {
            Debug.LogError("Tip hatası : " + state.GetType());
        }
    }

    public string GetUniqueIdentifier()
    {
        return GetComponent<UniqueId>().Id;
    }
}
