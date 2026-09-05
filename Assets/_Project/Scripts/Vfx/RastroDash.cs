using UnityEngine;

/// <summary>
/// Rastro luminoso del dash. PlayerController2D lo agrega solo en Awake;
/// el rastro se enciende durante el dash y se desvanece al terminar.
/// </summary>
[RequireComponent(typeof(PlayerController2D))]
public class RastroDash : MonoBehaviour
{
    private TrailRenderer _trail;
    private PlayerController2D _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController2D>();

        _trail = gameObject.AddComponent<TrailRenderer>();
        _trail.time = 0.22f;
        _trail.startWidth = 0.55f;
        _trail.endWidth = 0.02f;
        _trail.numCapVertices = 2;
        _trail.material = VfxUtil.MaterialSprites;
        _trail.startColor = new Color(1f, 0.92f, 0.55f, 0.85f);
        _trail.endColor = new Color(1f, 0.92f, 0.55f, 0f);
        _trail.sortingOrder = 3;
        _trail.emitting = false;
    }

    private void Update()
    {
        if (_trail != null)
        {
            _trail.emitting = _player != null && _player.EstaEnDash;
        }
    }
}
