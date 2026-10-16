using UnityEngine;

public class EnterFlyClothesShirts : StateMachineBehaviour
{
	protected virtual string WWpath => "prefabs/WierdWordEffects/FlyShirts";

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		CreateRandomPosEffect(animator);
	}

	public virtual void CreateRandomPosEffect(Animator animator)
	{
		Vector3 vector = Random.insideUnitSphere.normalized * 50f;
		CreateEffect(animator, WWpath, animator.transform.position + vector);
	}

	private void CreateEffect(Animator animator, string path, Vector3? position = null)
	{
		GameObject gameObject = Resources.Load<GameObject>(path);
		if (gameObject != null)
		{
			Object.Instantiate(gameObject, position ?? animator.transform.position, Quaternion.identity, animator.transform).name = gameObject.name;
		}
		else
		{
			Debug.LogError("Не удалось загрузить эффект по пути: " + path);
		}
	}
}
