﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// WeaverCore's implementation of a mantis shot
    /// </summary>
    public class MantisShot : MonoBehaviour
    {
        static CachedPrefab<MantisShot> cachedPrefab = new CachedPrefab<MantisShot>();

        BoxCollider2D _mainCollider;
        public BoxCollider2D MainCollider => _mainCollider ??= GetComponent<BoxCollider2D>();

        Rigidbody2D _rb;
        public Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();

        WeaverAnimationPlayer _animator;
        public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

        AudioPlayer _audio;
        public AudioPlayer Audio => _audio ??= GetComponent<AudioPlayer>();

        /// <summary>
        /// The amount of force that is applied on the y-axis when the mantis shot reaches the player
        /// </summary>
        [field: SerializeField]
        public float BoomerangForce { get; set; } = 1000f;

        [SerializeField]
        AudioClip boomerangLoopSound;

        public bool PlaySound { get; private set; } = true;

        private void OnEnable()
        {
            MainCollider.enabled = true;
            StartCoroutine(MainRoutine());
        }

        private void OnDisable()
        {
            RB.velocity = default;
            StopAllCoroutines();
        }

        IEnumerator MainRoutine()
        {
            yield return null;
            if (PlaySound)
            {
                Audio.Clip = boomerangLoopSound;
                Audio.Play();
            }
            Vector2 targetPos = Player.Player1.transform.position;

            var xSpeed = RB.velocity.x;

            var xForce = -xSpeed;

            float startTime = Time.time;

            while (Time.time < startTime + 4f && !(WithinMarginOfError(RB.velocity.x,0f,0.1f) && WithinMarginOfError(RB.velocity.y,0f,0.1f)))
            {
                var pos = transform.position;
                if (RB.velocity.y == 0f || (WithinMarginOfError(pos.x, targetPos.x,2f) && WithinMarginOfError(pos.y,targetPos.y,2f)) || (pos.y < targetPos.y - 0.5f && Time.time < startTime + 2f))
                {
                    yield return ApplyBoomerangForce(xForce);
                    yield break;

                }

                yield return null;
            }

            yield return ApplyBoomerangForce(xForce);
        }

        IEnumerator ApplyBoomerangForce(float xForce)
        {
            for (float t = 0; t < 4f; t += Time.deltaTime)
            {
                RB.AddForce(new Vector2(xForce * Time.deltaTime, BoomerangForce * Time.deltaTime));

                if (t > 1f && WithinMarginOfError(RB.velocity.x, 0f, 0.1f) && WithinMarginOfError(RB.velocity.y, 0f, 0.1f))
                {
                    break;
                }
                yield return null;
            }
            yield return EndRoutine();
        }

        IEnumerator EndRoutine()
        {
            Audio.StopPlaying();
            MainCollider.enabled = false;
            yield return Animator.PlayAnimationTillDone("Shot End");

            RB.velocity = default;
            StopAllCoroutines();
            Pooling.Destroy(this);
        }

        bool WithinMarginOfError(float a, float b, float error)
        {
            return Mathf.Abs(a - b) <= error;
        }

        /// <summary>
        /// Spawns a mantis shot
        /// </summary>
        /// <param name="position">The position to spawn it at</param>
        /// <param name="velocity">The starting velocity of the mantis shot</param>
        /// <param name="playLaunchSound">If set to true, will play a sound when it is launched</param>
        /// <returns></returns>
        public static MantisShot Spawn(Vector3 position, Vector2 velocity, bool playLaunchSound = true, MantisShot prefab = null)
        {
            if (cachedPrefab.Value == null)
            {
                cachedPrefab.Value = WeaverAssets.LoadWeaverAsset<GameObject>("Mantis Shot").GetComponent<MantisShot>();
            }

            if (prefab == null)
            {
                prefab = MantisShot.cachedPrefab.Value;
            }

            var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);
            instance.RB.velocity = velocity;
            instance.PlaySound = playLaunchSound;
            instance.Audio.AudioSource.volume = 1f;

            return instance;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Orbit Shield"))
            {
                StopAllCoroutines();
                StartCoroutine(OrbitShieldHit());
            }
        }

        IEnumerator OrbitShieldHit()
        {
            yield return null;
            RB.velocity = default;
            yield return EndRoutine();
        }
    }
}
