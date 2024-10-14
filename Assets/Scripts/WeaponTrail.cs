using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
    public GameObject _tip;
    public GameObject _base;
    public GameObject _trailMesh;
    public int _trailFrameLength;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private int _frameCount;
    private Vector3 _previousTipPosition;
    private Vector3 _previousBasePosition;

    private const int NUM_VERTICES = 12;

    void Start()
    {
        _mesh = new Mesh();
        _trailMesh.GetComponent<MeshFilter>().mesh = _mesh;

        _vertices = new Vector3[_trailFrameLength * NUM_VERTICES];
        _triangles = new int[_vertices.Length];

        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
    }

    void LateUpdate()
    {
        if (_frameCount == _trailFrameLength * NUM_VERTICES)
        {
            _frameCount = 0;
        }

        Vector3 currentTip = _tip.transform.position;
        Vector3 currentBase = _base.transform.position;

        _vertices[_frameCount] = currentBase;
        _vertices[_frameCount + 1] = currentTip;
        _vertices[_frameCount + 2] = _previousTipPosition;

        _vertices[_frameCount + 3] = currentBase;
        _vertices[_frameCount + 4] = _previousTipPosition;
        _vertices[_frameCount + 5] = currentTip;

        _vertices[_frameCount + 6] = _previousTipPosition;
        _vertices[_frameCount + 7] = currentBase;
        _vertices[_frameCount + 8] = _previousBasePosition;

        _vertices[_frameCount + 9] = _previousTipPosition;
        _vertices[_frameCount + 10] = _previousBasePosition;
        _vertices[_frameCount + 11] = currentBase;

        for (int i = 0; i < 12; i++)
        {
            _triangles[_frameCount + i] = _frameCount + i;
        }

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;

        _frameCount += NUM_VERTICES;
        _previousTipPosition = currentTip;
        _previousBasePosition = currentBase;        
    }
}
