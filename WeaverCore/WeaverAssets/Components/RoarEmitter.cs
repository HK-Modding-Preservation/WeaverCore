﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class RoarEmitter : MonoBehaviour 
{
	Transform Wave1;
	Transform Wave2;
	Transform Lines;
	ParticleSystem RubbleParticles;

	SpriteRenderer Wave2Sprite;

	public float stopAfterTime = 12f;

	static GameObject Prefab;

	void Awake()
	{
		if (Wave1 == null)
		{
			Wave1 = transform.Find("Wave 1");
			Wave2 = transform.Find("Wave 2");
			Wave2Sprite = Wave2.GetComponent<SpriteRenderer>();
			Lines = transform.Find("Lines");
			RubbleParticles = transform.Find("Rubble Fall").GetComponent<ParticleSystem>();
		}

		Wave2Sprite.enabled = false;
	}


	void Start()
	{
		StartCoroutine(MainRoutine());
		if (stopAfterTime > 0f)
		{
			StopRoaringAfter(stopAfterTime);
		}
	}

	IEnumerator MainRoutine()
	{
		CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingMed);

		StartCoroutine(TweenScale(Wave1,new Vector3(5.5f,5.5f,0f),0.15f,false));
		StartCoroutine(TweenScale(Lines,new Vector3(6f,6f,0f),0.15f,false));

		yield return new WaitForSeconds(0.15f);

		Wave2Sprite.enabled = true;
		Wave1.localScale = new Vector3(1f, 1f, 0f);

		StartCoroutine(TweenScale(Wave1, new Vector3(5.5f, 5.5f, 0f), 0.15f, false));
		StartCoroutine(TweenScale(Wave2, new Vector3(5.5f, 5.5f, 0f), 0.15f, false));
		StartCoroutine(TweenScale(Lines, new Vector3(6f, 6f, 0f), 0.15f, false));

		yield return new WaitForSeconds(0.15f);

		Destroy(Lines.gameObject);

		Wave1.localScale = new Vector3(1f, 1f, 0f);
		Wave2.localScale = new Vector3(5.5f, 5.5f, 0f);

		StartCoroutine(TweenScale(Wave1, new Vector3(5.5f, 5.5f, 0f), 0.15f, true));
		StartCoroutine(TweenScale(Wave2, new Vector3(5.5f, 5.5f, 0f), 0.15f, true));
	}

	public void StopRoaring()
	{
		StopAllCoroutines();
		CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
		Destroy(Wave1.gameObject);
		Destroy(Wave2.gameObject);
		RubbleParticles.Stop();
		StartCoroutine(StoppingRoutine());
	}

	public void StopRoaringAfter(float time)
	{
		StartCoroutine(Waiter(time));
	}

	IEnumerator Waiter(float t)
	{
		yield return new WaitForSeconds(t);
		StopRoaring();
	}

	IEnumerator StoppingRoutine()
	{
		yield return new WaitForSeconds(3f);
		Destroy(gameObject);
	}

	static IEnumerator TweenScale(Transform transform, Vector3 scaleFactor,float time, bool loop)
	{
		do
		{
			Vector3 previousScale = transform.localScale;
			Vector3 destinationScale = Vector3.Scale(previousScale, scaleFactor);

			for (float i = 0; i < time; i += Time.deltaTime)
			{
				transform.localScale = Vector3.Lerp(previousScale, destinationScale, i / time);
				yield return null;
			}
			if (loop)
			{
				transform.localScale = previousScale;
			}
		}
		while (loop);
	}

	/// <summary>
	/// Causes the player to lock in place. Used primarily to lock the player during a roar
	/// </summary>
	public void RoarLockPlayer()
	{
		Player.Player1.EnterRoarLock();
	}

	/// <summary>
	/// Stops the player from being locked in place
	/// </summary>
	public void RoarUnlockPlayer()
	{
		Player.Player1.ExitRoarLock();
	}

	public static RoarEmitter Spawn(Vector3 position)
	{
		if (Prefab == null)
		{
			Prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Roar Wave");
		}

		var instance = GameObject.Instantiate(Prefab, position + Prefab.transform.localPosition, Quaternion.identity).GetComponent<RoarEmitter>();

		instance.stopAfterTime = 0f;

		return instance;
	}
}
