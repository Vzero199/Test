using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class Controls : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct PlayerActions
	{
		private Controls m_Wrapper;

		public InputAction MoveScreen => m_Wrapper.m_Player_MoveScreen;

		public InputAction console => m_Wrapper.m_Player_console;

		public InputAction OnLeftClick => m_Wrapper.m_Player_OnLeftClick;

		public InputAction OnRightClick => m_Wrapper.m_Player_OnRightClick;

		public InputAction move => m_Wrapper.m_Player_move;

		public InputAction interact => m_Wrapper.m_Player_interact;

		public InputAction flashlight => m_Wrapper.m_Player_flashlight;

		public InputAction useitem => m_Wrapper.m_Player_useitem;

		public InputAction selectcell => m_Wrapper.m_Player_selectcell;

		public InputAction Run => m_Wrapper.m_Player_Run;

		public InputAction Check => m_Wrapper.m_Player_Check;

		public InputAction escape => m_Wrapper.m_Player_escape;

		public InputAction map => m_Wrapper.m_Player_map;

		public InputAction tasks => m_Wrapper.m_Player_tasks;

		public InputAction RestartScene => m_Wrapper.m_Player_RestartScene;

		public bool enabled => Get().enabled;

		public PlayerActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IPlayerActions instance)
		{
			if (instance != null && !m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
				MoveScreen.started += instance.OnMoveScreen;
				MoveScreen.performed += instance.OnMoveScreen;
				MoveScreen.canceled += instance.OnMoveScreen;
				console.started += instance.OnConsole;
				console.performed += instance.OnConsole;
				console.canceled += instance.OnConsole;
				OnLeftClick.started += instance.OnOnLeftClick;
				OnLeftClick.performed += instance.OnOnLeftClick;
				OnLeftClick.canceled += instance.OnOnLeftClick;
				OnRightClick.started += instance.OnOnRightClick;
				OnRightClick.performed += instance.OnOnRightClick;
				OnRightClick.canceled += instance.OnOnRightClick;
				move.started += instance.OnMove;
				move.performed += instance.OnMove;
				move.canceled += instance.OnMove;
				interact.started += instance.OnInteract;
				interact.performed += instance.OnInteract;
				interact.canceled += instance.OnInteract;
				flashlight.started += instance.OnFlashlight;
				flashlight.performed += instance.OnFlashlight;
				flashlight.canceled += instance.OnFlashlight;
				useitem.started += instance.OnUseitem;
				useitem.performed += instance.OnUseitem;
				useitem.canceled += instance.OnUseitem;
				selectcell.started += instance.OnSelectcell;
				selectcell.performed += instance.OnSelectcell;
				selectcell.canceled += instance.OnSelectcell;
				Run.started += instance.OnRun;
				Run.performed += instance.OnRun;
				Run.canceled += instance.OnRun;
				Check.started += instance.OnCheck;
				Check.performed += instance.OnCheck;
				Check.canceled += instance.OnCheck;
				escape.started += instance.OnEscape;
				escape.performed += instance.OnEscape;
				escape.canceled += instance.OnEscape;
				map.started += instance.OnMap;
				map.performed += instance.OnMap;
				map.canceled += instance.OnMap;
				tasks.started += instance.OnTasks;
				tasks.performed += instance.OnTasks;
				tasks.canceled += instance.OnTasks;
				RestartScene.started += instance.OnRestartScene;
				RestartScene.performed += instance.OnRestartScene;
				RestartScene.canceled += instance.OnRestartScene;
			}
		}

		private void UnregisterCallbacks(IPlayerActions instance)
		{
			MoveScreen.started -= instance.OnMoveScreen;
			MoveScreen.performed -= instance.OnMoveScreen;
			MoveScreen.canceled -= instance.OnMoveScreen;
			console.started -= instance.OnConsole;
			console.performed -= instance.OnConsole;
			console.canceled -= instance.OnConsole;
			OnLeftClick.started -= instance.OnOnLeftClick;
			OnLeftClick.performed -= instance.OnOnLeftClick;
			OnLeftClick.canceled -= instance.OnOnLeftClick;
			OnRightClick.started -= instance.OnOnRightClick;
			OnRightClick.performed -= instance.OnOnRightClick;
			OnRightClick.canceled -= instance.OnOnRightClick;
			move.started -= instance.OnMove;
			move.performed -= instance.OnMove;
			move.canceled -= instance.OnMove;
			interact.started -= instance.OnInteract;
			interact.performed -= instance.OnInteract;
			interact.canceled -= instance.OnInteract;
			flashlight.started -= instance.OnFlashlight;
			flashlight.performed -= instance.OnFlashlight;
			flashlight.canceled -= instance.OnFlashlight;
			useitem.started -= instance.OnUseitem;
			useitem.performed -= instance.OnUseitem;
			useitem.canceled -= instance.OnUseitem;
			selectcell.started -= instance.OnSelectcell;
			selectcell.performed -= instance.OnSelectcell;
			selectcell.canceled -= instance.OnSelectcell;
			Run.started -= instance.OnRun;
			Run.performed -= instance.OnRun;
			Run.canceled -= instance.OnRun;
			Check.started -= instance.OnCheck;
			Check.performed -= instance.OnCheck;
			Check.canceled -= instance.OnCheck;
			escape.started -= instance.OnEscape;
			escape.performed -= instance.OnEscape;
			escape.canceled -= instance.OnEscape;
			map.started -= instance.OnMap;
			map.performed -= instance.OnMap;
			map.canceled -= instance.OnMap;
			tasks.started -= instance.OnTasks;
			tasks.performed -= instance.OnTasks;
			tasks.canceled -= instance.OnTasks;
			RestartScene.started -= instance.OnRestartScene;
			RestartScene.performed -= instance.OnRestartScene;
			RestartScene.canceled -= instance.OnRestartScene;
		}

		public void RemoveCallbacks(IPlayerActions instance)
		{
			if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IPlayerActions instance)
		{
			foreach (IPlayerActions playerActionsCallbackInterface in m_Wrapper.m_PlayerActionsCallbackInterfaces)
			{
				UnregisterCallbacks(playerActionsCallbackInterface);
			}
			m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct UIActions
	{
		private Controls m_Wrapper;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public bool enabled => Get().enabled;

		public UIActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Click.started -= instance.OnClick;
			Click.performed -= instance.OnClick;
			Click.canceled -= instance.OnClick;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IPlayerActions
	{
		void OnMoveScreen(InputAction.CallbackContext context);

		void OnConsole(InputAction.CallbackContext context);

		void OnOnLeftClick(InputAction.CallbackContext context);

		void OnOnRightClick(InputAction.CallbackContext context);

		void OnMove(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnFlashlight(InputAction.CallbackContext context);

		void OnUseitem(InputAction.CallbackContext context);

		void OnSelectcell(InputAction.CallbackContext context);

		void OnRun(InputAction.CallbackContext context);

		void OnCheck(InputAction.CallbackContext context);

		void OnEscape(InputAction.CallbackContext context);

		void OnMap(InputAction.CallbackContext context);

		void OnTasks(InputAction.CallbackContext context);

		void OnRestartScene(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnClick(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnNavigate(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Player;

	private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();

	private readonly InputAction m_Player_MoveScreen;

	private readonly InputAction m_Player_console;

	private readonly InputAction m_Player_OnLeftClick;

	private readonly InputAction m_Player_OnRightClick;

	private readonly InputAction m_Player_move;

	private readonly InputAction m_Player_interact;

	private readonly InputAction m_Player_flashlight;

	private readonly InputAction m_Player_useitem;

	private readonly InputAction m_Player_selectcell;

	private readonly InputAction m_Player_Run;

	private readonly InputAction m_Player_Check;

	private readonly InputAction m_Player_escape;

	private readonly InputAction m_Player_map;

	private readonly InputAction m_Player_tasks;

	private readonly InputAction m_Player_RestartScene;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Navigate;

	private int m_KeyboardMouseSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public PlayerActions Player => new PlayerActions(this);

	public UIActions UI => new UIActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public Controls()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"Controls\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Player\",\r\n            \"id\": \"c65abceb-867b-4354-88fa-6b9ce0e0da06\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"MoveScreen\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"a7aa0138-6e91-4496-879d-674015f67cf2\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"console\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"7d793d4c-b44b-465f-bc8c-9d18efb7003d\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OnLeftClick\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b987ef41-579e-403a-99da-905cf911cfc8\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OnRightClick\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b63ebc33-55ca-4cba-9952-4bdb17e732df\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"ad060a4d-7ee0-488e-9960-6f9ae86e8533\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"interact\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"bdcdc315-4432-4fcc-b7f8-734c5ef4d4f9\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"flash light\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"07f6eca8-01ec-468f-84ac-476ce3eb644f\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"use item\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c932cd5f-dcd8-4253-bbce-8f3fb0319049\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"select cell\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"dbe79368-1126-4dec-9586-8e3c4c8c5297\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Run\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"53c750cf-6fb3-4c76-85d2-7b0bdfdff8d0\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Check\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"7519a028-6b5c-4b07-a19c-507f937d7f62\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"escape\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"80e8decc-5378-4a9b-bfd8-d1b5988f3f57\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"map\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"97814209-8219-42ac-a601-68e9dba7a802\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"tasks\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"58ba4647-5c05-4668-893a-a2f6f611eb29\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RestartScene\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"7d49ae6c-8521-4d53-b87c-1179ab665f96\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d28fcdf2-0fb7-4e4a-860e-a5c4fbb74bbf\",\r\n                    \"path\": \"<Keyboard>/f5\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"console\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"2D Vector\",\r\n                    \"id\": \"5bac08f0-b850-4a67-8a9a-769b271d8bc3\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MoveScreen\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Up\",\r\n                    \"id\": \"e6b4e285-6aab-4e47-b405-84e040a5b1e5\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MoveScreen\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Up\",\r\n                    \"id\": \"fcfe3127-4b88-4fd2-b9cd-b91a563d8fa3\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MoveScreen\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Down\",\r\n                    \"id\": \"3f397539-5fb5-42b8-a4cf-42647b5bf395\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MoveScreen\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Down\",\r\n                    \"id\": \"882fd429-8c91-4595-b92e-07ee630092c8\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MoveScreen\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"998525a2-e28d-4574-957f-4bc78fd9c727\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OnLeftClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"acf0c0c9-23ed-4898-86d5-c9521371ffe4\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OnRightClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"arrows\",\r\n                    \"id\": \"8e1e4e1d-74f7-4b9d-a53e-aaa7452eb1c8\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"9491303b-26aa-4d91-8440-4d230ee1e2a6\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fa521d38-f4cd-4af2-9af9-47b4cfb629ac\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"ad\",\r\n                    \"id\": \"63f6c05e-d646-48c3-9f7e-f97e4a56c2a7\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"9be8ac5f-e137-473c-a797-8a44ebc329ef\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"2891b1dd-a50e-4e50-9cad-e66072377843\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5c1b217a-16cd-4b9c-8267-87c5c7ae988f\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a6b744df-eb74-4077-be3a-71fd40c6cc43\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"89d24875-461a-4fdf-a5cb-b7c75fbd84dd\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"flash light\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"dd106f5d-66be-4727-9f3c-88e33472de98\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"use item\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b58fdcf2-5fd3-43cf-aaa5-39af7d7f256a\",\r\n                    \"path\": \"<Keyboard>/numpad0\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"use item\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"42a1b114-8052-46e0-a413-c722068e60cc\",\r\n                    \"path\": \"<Keyboard>/1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0a04be73-f187-4f00-a07c-af64406ddd7c\",\r\n                    \"path\": \"<Keyboard>/2\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b59bf45e-7c3d-49be-8b90-d68e67510265\",\r\n                    \"path\": \"<Keyboard>/3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"52e38bb7-91f9-4e8b-9c7c-5668bd75f9db\",\r\n                    \"path\": \"<Keyboard>/numpad1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9ec3fa89-c51e-4099-9589-79b1de9e0729\",\r\n                    \"path\": \"<Keyboard>/numpad2\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1df554d3-7aa8-4b1f-9628-c9cfa14df9fb\",\r\n                    \"path\": \"<Keyboard>/numpad3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"select cell\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"28075d7b-12ad-466c-b860-756b0976adc8\",\r\n                    \"path\": \"<Keyboard>/shift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Run\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9f241646-11d2-4632-8226-9d3bace21754\",\r\n                    \"path\": \"<Keyboard>/tab\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Check\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2ca8dee7-8d31-42bd-b167-9c3fe2f512c4\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Check\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e5c543d2-dcd8-4789-8943-d122d2ac5dc1\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"escape\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"229d40d3-e559-44bf-96f2-6266e97d4b90\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"map\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d3b96312-c8f1-4d72-8853-ec3f20ce9cff\",\r\n                    \"path\": \"<Keyboard>/rightCtrl\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"map\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5128e2a3-1b93-45be-8995-6dedcafc6416\",\r\n                    \"path\": \"<Keyboard>/alt\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"tasks\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7c0dffc0-2c34-4369-9a9e-453c3504ce42\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"RestartScene\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"2df1a649-9b96-46f8-b8fc-c8e2fa47459e\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Click\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"d0fbc52b-4320-47a3-8380-9e1b21ba3d74\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"64fa73bd-960e-46aa-9c83-e86e64f762ef\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"4093e92a-c445-4384-86bb-faab3eff1159\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Submit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"232c26bf-9b83-4065-9454-ff11a0543a1e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"769e66f2-eed2-42a3-af03-5eb9b89817ea\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Keyboard\",\r\n                    \"id\": \"98d2c120-3d65-44c3-b3bc-a205ca722857\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"38ac14d5-1f9f-47eb-a997-49c4e4896528\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"c61d1237-de64-4789-a266-9a5ea629c9b9\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"a9da9e17-e5e8-43cb-8893-d3cb33d0c8ae\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"51fb705a-3192-49a9-a54a-4b2f3426ed0b\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"4e67f492-7430-4e74-bc5f-04cc31484847\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"6284df9c-ef8e-4c1d-92f2-4111276b1cf0\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"323855e3-976e-409c-9513-9603f4d0f497\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"8fe57171-56be-4966-a43b-01f0c3c73903\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b9fe9ad9-accb-489a-9e36-530b1b157cd1\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9f9cb445-1119-4bfc-bf80-42b8015a69ab\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b65fecdc-60ae-4903-a79d-be4097dd00a8\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8abe51fb-0801-43f8-9c7e-d35fcec5b1e8\",\r\n                    \"path\": \"<Keyboard>/backspace\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f506a900-00f1-4806-91de-7f79d50ada81\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"274a3bb3-2ac2-4494-b114-c05d988e713b\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
		m_Player_MoveScreen = m_Player.FindAction("MoveScreen", throwIfNotFound: true);
		m_Player_console = m_Player.FindAction("console", throwIfNotFound: true);
		m_Player_OnLeftClick = m_Player.FindAction("OnLeftClick", throwIfNotFound: true);
		m_Player_OnRightClick = m_Player.FindAction("OnRightClick", throwIfNotFound: true);
		m_Player_move = m_Player.FindAction("move", throwIfNotFound: true);
		m_Player_interact = m_Player.FindAction("interact", throwIfNotFound: true);
		m_Player_flashlight = m_Player.FindAction("flash light", throwIfNotFound: true);
		m_Player_useitem = m_Player.FindAction("use item", throwIfNotFound: true);
		m_Player_selectcell = m_Player.FindAction("select cell", throwIfNotFound: true);
		m_Player_Run = m_Player.FindAction("Run", throwIfNotFound: true);
		m_Player_Check = m_Player.FindAction("Check", throwIfNotFound: true);
		m_Player_escape = m_Player.FindAction("escape", throwIfNotFound: true);
		m_Player_map = m_Player.FindAction("map", throwIfNotFound: true);
		m_Player_tasks = m_Player.FindAction("tasks", throwIfNotFound: true);
		m_Player_RestartScene = m_Player.FindAction("RestartScene", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
