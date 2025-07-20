using Architect.Objects;
using Modding.Utils;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ObjectMover : MonoBehaviour
{
    private GameObject _object;
    public int movementType;
    
    public string id;
    
    public float rotation;
    public float yMovement;
    public float xMovement;
    
    private Quaternion _rotation;
    private Vector3 _movement;
    private Transform _source;

    private void Start()
    {
        _rotation = Quaternion.Euler(0, 0, rotation);
        _movement = new Vector2(xMovement, yMovement);

        _source = movementType == 2 ? HeroController.instance.gameObject.transform : gameObject.transform;
    }

    public void DoMove()
    {
        if (!_object)
        {
            if (!PlacementManager.Objects.TryGetValue(id, out _object)) _object = gameObject.scene.FindGameObject(id);
        }

        if (!_object) return;

        if (movementType == 0)
        {
            
            _object.transform.rotation = Quaternion.Euler(_object.transform.rotation.eulerAngles + _rotation.eulerAngles);
            _object.transform.position += _movement;
        }
        else
        {
            _object.transform.rotation = _rotation;

            var move = _source.position + _movement;
            move.z = _object.transform.position.z;
            _object.transform.position = move;
        }
    }
}