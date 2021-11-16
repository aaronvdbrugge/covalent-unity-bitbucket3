using UnityEngine;

public class Singleton<T> : MonoBehaviour
	where T : Component
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				return null;
			}
			
			if (_instance == null)
			{
				var objs = FindObjectsOfType(typeof(T)) as T[];
				if (objs.Length > 0)
					_instance = objs[0];
				if (objs.Length > 1)
				{
					Debug.LogError($"There is more than one {typeof(T).Name} in the scene.");
				}

				if (_instance == null)
				{
					GameObject obj = new GameObject();
					obj.hideFlags = HideFlags.HideAndDontSave;
					_instance = obj.AddComponent<T>();
				}
			}

			return _instance;
		}
	}
	
	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	void OnApplicationQuit()
	{
		applicationIsQuitting = true;
	}
}

public class SingletonPersistent<T> : MonoBehaviour
	where T : Component
{
	public static T Instance { get; private set; }

	public virtual void Awake()
	{
		if (Instance == null)
		{
			Instance = this as T;
			DontDestroyOnLoad(this);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}