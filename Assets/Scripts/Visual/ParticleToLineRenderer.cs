using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleToLineRenderer : MonoBehaviour {
    private class LineRendererWithPositions {

        public LineRendererWithPositions(LineRenderer renderer, Vector3 startingPos) {
            lr = renderer;
            _positions = new List<Vector3>(128);
            AddPointToRenderer(startingPos);
        }

        public void AddPosition(Vector3 pos) {
            float dist = Vector3.Distance(pos, _positions[_positions.Count - 1]);
            if (dist > _minDistanceFowNewPoint) {
                AddPointToRenderer(pos);
            }
        }

        private void AddPointToRenderer(Vector3 newPos) {
            _positions.Add(newPos);
            lr.positionCount = (_positions.Count);
            lr.SetPositions(_positions.ToArray());
        }

        private LineRenderer lr;
        private List<Vector3> _positions;
        private float _minDistanceFowNewPoint = 0.05f;

    }

    public GameObject LineRendererPrefab;
    private ParticleSystem _ps;
    private List<LineRendererWithPositions> _lrs;

    private float _sizeMultiplier = 1;

    void Awake() {
        _ps = GetComponent<ParticleSystem>();
        _lrs = new List<LineRendererWithPositions>(_ps.maxParticles);
    }

    void Update() {
        //Alloc but w/e. Simpler this way since we store only the positions
        ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[_ps.particleCount];
        _ps.GetParticles(_particles);

        for (int i = 0; i < _particles.Length; ++i) {
            ParticleSystem.Particle p = _particles[i];
            if (i >= _lrs.Count) {
                GameObject newGo = Instantiate(LineRendererPrefab, transform);
                LineRenderer newRenderer = newGo.GetComponent<LineRenderer>();
                _lrs.Add(new LineRendererWithPositions(newRenderer, p.position));

                newRenderer.widthMultiplier = _sizeMultiplier;
            } else {
                _lrs[i].AddPosition(p.position);
            }
        }

    }

    public void SetSizeMultiplier(float multiplier) {
        _sizeMultiplier = multiplier;
    }
}
