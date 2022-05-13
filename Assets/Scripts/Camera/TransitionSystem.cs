using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TransitionSystem : MonoBehaviour
{
	private static TransitionSystem instance;

	private Texture2D texture;

	private Color guiColor;

	void Awake()
    {
        if (instance == null)
        {
			instance = this;
        }
        else
        {
			Destroy(this);
        }

		texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

		texture.filterMode = FilterMode.Point;

		for (int i = 0; i < Screen.width; i++)
		{
			for (int j = 0; j < Screen.height; j++)
			{
				texture.SetPixel(i, j, Color.white);
			}
		}

		guiColor = Color.white;
	}

	public void SetTransitionColor(Color textureColor)
    {
		float alpha = guiColor.a;

		guiColor = textureColor;
		guiColor.a = alpha;
	}

	public void SetTransitionTexture(Texture2D texture)
	{
		this.texture = texture;
	}

	public void Apply(Transition transitionType, float time)
    {
        switch (transitionType)
        {
			case Transition.FadeIn:
				FadeIn(time);
				break;

			case Transition.FadeOut:
				FadeOut(time);
				break;

			default:
				break;
		}
    }

	public void SwitchToScene(SceneSystem.GameScenes scene, Transition transitionType, float time)
    {
		Apply(transitionType, time);
		StartCoroutine(WaitForScene(time + .1f, scene));
    }

	public void ApplyDelayed(Transition transitionType, float delay, float time)
	{
		StartCoroutine(WaitForTransition(delay, transitionType, time));
	}

	//Transitions

	void FadeIn(float time)
	{
		guiColor.a = 1;
		DOTween.ToAlpha(() => guiColor, x => guiColor = x, 0, time);
	}

	void FadeOut(float time)
	{
		guiColor.a = 0;
		DOTween.ToAlpha(() => guiColor, x => guiColor = x, 1, time);
	}

	//////////////

	IEnumerator WaitForTransition(float wait, Transition transition, float time)
	{
		yield return new WaitForSeconds(wait);
		Apply(transition, time);
	}

	IEnumerator WaitForScene(float time, SceneSystem.GameScenes scene)
    {
		yield return new WaitForSeconds(time);
		SceneSystem.LoadScene(scene);
    }

	protected void OnGUI()
	{
		GUI.color = guiColor;
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
	}

	public static TransitionSystem GetInstance()
    {
		return instance;
    }

	public enum Transition
    {
		FadeIn,
		FadeOut,
		FadeColor,
		None
    }
}
