using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightSword : MonoBehaviour
{
	private HashSet<Enemy> enemiesInLight = new HashSet<Enemy>();

	private Dictionary<Enemy, float> enemyResistanceTimers = new Dictionary<Enemy, float>();

	private BoxCollider lightCollider;

	private Light2D flashlightLight;

	private Transform playerTransform;

	private SpriteRenderer playerSpriteRenderer;

	private Vector3 lightOffset = new Vector3(0.7f, 0f, 0f);

	private static FlashlightSword instance;

	private Color normalColor = Color.white;

	private Color enemyDetectedColor = new Color(0.5f, 0.8f, 1f);

	private float colorTransitionSpeed = 3f;

	public Dictionary<Enemy, float> EnemyResistanceTimers => enemyResistanceTimers;

	public static FlashlightSword GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<FlashlightSword>();
		}
		return instance;
	}

	private void Awake()
	{
		lightCollider = GetComponent<BoxCollider>();
		flashlightLight = GetComponent<Light2D>();
		if (flashlightLight == null)
		{
			Debug.LogError("Light2D component not found on FlashlightSword!");
		}
	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			playerTransform = gameObject.transform;
			playerSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		}
		else
		{
			Debug.LogError("Player not found.");
		}
		base.gameObject.SetActive(value: false);
		if (flashlightLight != null)
		{
			flashlightLight.color = normalColor;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Enemy component = other.GetComponent<Enemy>();
		if (component != null && component.IsLightSwordWeakness() && enemiesInLight.Add(component))
		{
			component.WhileOnLight();
			enemyResistanceTimers[component] = component.resistanceTime;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Enemy component = other.GetComponent<Enemy>();
		if (component != null && enemiesInLight.Remove(component))
		{
			component.EscapeFromLight();
			enemyResistanceTimers.Remove(component);
		}
	}

	private void OnDisable()
	{
		foreach (Enemy item in new List<Enemy>(enemiesInLight))
		{
			if (item != null)
			{
				item.EscapeFromLight();
			}
		}
		enemiesInLight.Clear();
		enemyResistanceTimers.Clear();
		if (BatteryManager.Instance != null)
		{
			BatteryManager.Instance.InterruptBatteryDrain();
		}
		if (flashlightLight != null)
		{
			flashlightLight.color = normalColor;
		}
	}

	public void ManualTriggerCheck()
	{
		if (lightCollider == null || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(base.transform.position, lightCollider.size / 2f, base.transform.rotation);
		Debug.Log($"Collider Count: {array.Length}");
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Enemy component = array2[i].GetComponent<Enemy>();
			if (component != null && component.IsLightSwordWeakness() && enemiesInLight.Add(component))
			{
				component.WhileOnLight();
				enemyResistanceTimers[component] = component.resistanceTime;
			}
		}
	}

	private void Update()
	{
		foreach (Enemy item in new List<Enemy>(enemiesInLight))
		{
			if (item == null || !enemiesInLight.Contains(item))
			{
				enemyResistanceTimers.Remove(item);
				continue;
			}
			item.WhileOnLight();
			if (enemyResistanceTimers.TryGetValue(item, out var value))
			{
				value -= Time.deltaTime;
				enemyResistanceTimers[item] = value;
				if (value <= 0f)
				{
					item.LightReact();
					enemiesInLight.Remove(item);
					enemyResistanceTimers.Remove(item);
				}
			}
			else
			{
				enemiesInLight.Remove(item);
			}
		}
		UpdateLightColor();
	}

	private void UpdateLightColor()
	{
		if (flashlightLight == null)
		{
			return;
		}
		bool flag = false;
		foreach (Enemy item in enemiesInLight)
		{
			if (item != null && item.IsLightSwordWeakness())
			{
				flag = true;
				break;
			}
		}
		Color b = (flag ? enemyDetectedColor : normalColor);
		flashlightLight.color = Color.Lerp(flashlightLight.color, b, Time.deltaTime * colorTransitionSpeed);
	}
}
