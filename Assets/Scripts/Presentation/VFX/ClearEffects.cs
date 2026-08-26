using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Multiformatris.Presentation.VFX
{
    public class ClearEffects : MonoBehaviour
    {
        [Header("Particle Settings")]
        public ParticleSystem ClearParticlePrefab;
        public int PoolSize = 20;

        [Header("Flash Settings")]
        public float FlashDuration = 0.2f;
        public Color FlashColor = Color.white;

        [Header("Scale Animation")]
        public float ScaleUpDuration = 0.1f;
        public float ScaleMultiplier = 1.2f;

        private Queue<ParticleSystem> _particlePool = new Queue<ParticleSystem>();
        private List<ParticleSystem> _activeParticles = new List<ParticleSystem>();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (ClearParticlePrefab == null) return;

            for (int i = 0; i < PoolSize; i++)
            {
                ParticleSystem ps = Instantiate(ClearParticlePrefab, transform);
                ps.gameObject.SetActive(false);
                _particlePool.Enqueue(ps);
            }
        }

        public void PlayClearEffect(Vector3 position, Color blockColor, int particleCount = 20)
        {
            ParticleSystem ps = GetFromPool();
            if (ps == null) return;

            ps.transform.position = position;

            var main = ps.main;
            main.startColor = blockColor;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)particleCount)
            });

            ps.gameObject.SetActive(true);
            ps.Play();

            _activeParticles.Add(ps);
            StartCoroutine(ReturnToPool(ps, main.startLifetime.constant + 0.1f));
        }

        public void PlayLayerClearEffect(List<Vector3> positions, Color layerColor)
        {
            foreach (Vector3 pos in positions)
            {
                PlayClearEffect(pos, layerColor, 10);
            }
        }

        public void PlayFlashEffect(Renderer renderer)
        {
            if (renderer == null) return;
            StartCoroutine(FlashCoroutine(renderer));
        }

        private IEnumerator FlashCoroutine(Renderer renderer)
        {
            Material mat = renderer.material;
            Color originalColor = mat.color;
            mat.color = FlashColor;

            float elapsed = 0f;
            while (elapsed < FlashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / FlashDuration;
                mat.color = Color.Lerp(FlashColor, originalColor, t);
                yield return null;
            }

            mat.color = originalColor;
        }

        public void PlayScaleAnimation(Transform target)
        {
            StartCoroutine(ScaleAnimationCoroutine(target));
        }

        private IEnumerator ScaleAnimationCoroutine(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * ScaleMultiplier;

            float elapsed = 0f;
            while (elapsed < ScaleUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ScaleUpDuration;
                target.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < ScaleUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ScaleUpDuration;
                target.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            target.localScale = originalScale;
        }

        private ParticleSystem GetFromPool()
        {
            if (_particlePool.Count > 0)
                return _particlePool.Dequeue();

            if (_activeParticles.Count > 0)
                return _activeParticles[0];

            return null;
        }

        private IEnumerator ReturnToPool(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            _activeParticles.Remove(ps);
            _particlePool.Enqueue(ps);
        }

        public void ClearAll()
        {
            foreach (var ps in _activeParticles)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                    _particlePool.Enqueue(ps);
                }
            }
            _activeParticles.Clear();
        }
    }
}
