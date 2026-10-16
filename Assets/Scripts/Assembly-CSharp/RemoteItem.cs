using System;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.UI;

public class RemoteItem : ItemToUse
{
	[SerializeField]
	private Image image;

	private Sprite spriteTvNear;

	private Sprite spriteTvFar;

	private static bool _closeToTv;

	public static bool CloseToTv
	{
		get
		{
			return _closeToTv;
		}
		set
		{
			if (_closeToTv != value)
			{
				_closeToTv = value;
				CloseToTvChanged?.Invoke(_closeToTv);
			}
		}
	}

	public static event Action<bool> CloseToTvChanged;

	private void Awake()
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
		spriteTvNear = Resources.Load<Sprite>("Sprites/Remote2");
		spriteTvFar = Resources.Load<Sprite>("Sprites/Remote");
	}

	private void OnEnable()
	{
		CloseToTvChanged += OnCloseToTvChangedHandler;
		UpdateSprite(CloseToTv);
	}

	private void OnDisable()
	{
		CloseToTvChanged -= OnCloseToTvChangedHandler;
	}

	private void OnCloseToTvChangedHandler(bool newValue)
	{
		UpdateSprite(newValue);
	}

	private void UpdateSprite(bool closeToTv)
	{
		if (!(image == null))
		{
			image.sprite = (closeToTv ? spriteTvNear : spriteTvFar);
		}
	}

	public override bool CanBeUsed()
	{
		return CloseToTv;
	}

	public override void UseItem()
	{
		TVEnemy tVEnemy = UnityEngine.Object.FindObjectOfType<TVEnemy>();
		if (tVEnemy != null)
		{
			SoundManager.PlaySound(SoundType.TVOff);
			UnityEngine.Object.Destroy(tVEnemy);
		}
	}
}
