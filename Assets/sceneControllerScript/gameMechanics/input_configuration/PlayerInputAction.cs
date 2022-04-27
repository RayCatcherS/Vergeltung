//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/sceneControllerScript/gameMechanics/input_configuration/PlayerInputAction.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInputAction : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputAction()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputAction"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""183332d7-dbb6-4782-8743-114209d597af"",
            ""actions"": [
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""0063a5bd-fa30-4334-bb18-21ea86bfd5cd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AnalogMovement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""4a37ee72-ad4a-49b5-ad30-e8d0ca7f7cb2"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AnalogRotation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""3820c136-5067-4b97-9e65-b337b7ecf912"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InventaryNextWeapon"",
                    ""type"": ""Button"",
                    ""id"": ""3acacdaa-7154-4605-a87d-d488fc921e75"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InventaryPreviewWeapon"",
                    ""type"": ""Button"",
                    ""id"": ""8ea119c8-2d7e-4e86-aafb-906319194511"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InventaryUseWeaponItem"",
                    ""type"": ""Button"",
                    ""id"": ""0cfc43b8-a725-4d5f-906f-d9e7032fb676"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""PutAwayExtractWeapon"",
                    ""type"": ""Button"",
                    ""id"": ""293a1bf6-3879-4892-8ac4-5320e0ea32c9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a45388a0-0dcc-4b02-a0ea-218c33445c2e"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""32d4ced5-f819-4979-a5d7-e710991cbb5c"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AnalogMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""dd62e76b-eed5-4771-83c5-46a640e4a564"",
                    ""path"": ""<XInputController>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c3838c12-7738-4527-a161-40676921ecbe"",
                    ""path"": ""<XInputController>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""75049201-0454-4ae3-a3c1-14558bc9314b"",
                    ""path"": ""<XInputController>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""19326fc8-49bd-4f4e-8f13-fcb670d618ea"",
                    ""path"": ""<XInputController>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""4bce17dc-2fb4-4a71-821e-2b2f9f5d62cf"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogRotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""right"",
                    ""id"": ""32175300-65ef-43dd-afcc-06da0045a586"",
                    ""path"": ""<XInputController>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""f2625b89-48fa-4263-beee-3cb011099c4f"",
                    ""path"": ""<XInputController>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""5df52a33-0573-45c6-ba2b-c8dd9e6c677a"",
                    ""path"": ""<XInputController>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""c6101fc7-d41d-4c8f-a04b-0c848635ecaa"",
                    ""path"": ""<XInputController>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""AnalogRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5d1d137d-6c35-43c1-ae63-765b09bcc6cd"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""InventaryNextWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3f62d7a8-60d3-4dc4-a87f-64158c0fdac4"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""InventaryPreviewWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""61fa808a-f83b-4862-a38f-eb571fcef398"",
                    ""path"": ""<XInputController>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""InventaryUseWeaponItem"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1dbb6f2f-65f7-4916-b0c9-c339edfccb86"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""PutAwayExtractWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""46a38724-d632-48d9-a8c2-a42e2f21609a"",
            ""actions"": [
                {
                    ""name"": ""Action"",
                    ""type"": ""PassThrough"",
                    ""id"": ""1a54737e-0f00-46dc-a6a5-480909ad815c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MenuNavigation"",
                    ""type"": ""Value"",
                    ""id"": ""36351262-daa6-4744-8486-c6896bedb80d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3f02bb0f-9374-424b-a568-d783456e2591"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""de39514a-3124-489c-a719-e117df35cf92"",
                    ""path"": ""<XInputController>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""MenuNavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Xbox Control Scheme"",
            ""bindingGroup"": ""Xbox Control Scheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Run = m_Player.FindAction("Run", throwIfNotFound: true);
        m_Player_AnalogMovement = m_Player.FindAction("AnalogMovement", throwIfNotFound: true);
        m_Player_AnalogRotation = m_Player.FindAction("AnalogRotation", throwIfNotFound: true);
        m_Player_InventaryNextWeapon = m_Player.FindAction("InventaryNextWeapon", throwIfNotFound: true);
        m_Player_InventaryPreviewWeapon = m_Player.FindAction("InventaryPreviewWeapon", throwIfNotFound: true);
        m_Player_InventaryUseWeaponItem = m_Player.FindAction("InventaryUseWeaponItem", throwIfNotFound: true);
        m_Player_PutAwayExtractWeapon = m_Player.FindAction("PutAwayExtractWeapon", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Action = m_UI.FindAction("Action", throwIfNotFound: true);
        m_UI_MenuNavigation = m_UI.FindAction("MenuNavigation", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

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
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Run;
    private readonly InputAction m_Player_AnalogMovement;
    private readonly InputAction m_Player_AnalogRotation;
    private readonly InputAction m_Player_InventaryNextWeapon;
    private readonly InputAction m_Player_InventaryPreviewWeapon;
    private readonly InputAction m_Player_InventaryUseWeaponItem;
    private readonly InputAction m_Player_PutAwayExtractWeapon;
    public struct PlayerActions
    {
        private @PlayerInputAction m_Wrapper;
        public PlayerActions(@PlayerInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Run => m_Wrapper.m_Player_Run;
        public InputAction @AnalogMovement => m_Wrapper.m_Player_AnalogMovement;
        public InputAction @AnalogRotation => m_Wrapper.m_Player_AnalogRotation;
        public InputAction @InventaryNextWeapon => m_Wrapper.m_Player_InventaryNextWeapon;
        public InputAction @InventaryPreviewWeapon => m_Wrapper.m_Player_InventaryPreviewWeapon;
        public InputAction @InventaryUseWeaponItem => m_Wrapper.m_Player_InventaryUseWeaponItem;
        public InputAction @PutAwayExtractWeapon => m_Wrapper.m_Player_PutAwayExtractWeapon;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Run.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @AnalogMovement.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogMovement;
                @AnalogMovement.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogMovement;
                @AnalogMovement.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogMovement;
                @AnalogRotation.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogRotation;
                @AnalogRotation.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogRotation;
                @AnalogRotation.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAnalogRotation;
                @InventaryNextWeapon.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryNextWeapon;
                @InventaryNextWeapon.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryNextWeapon;
                @InventaryNextWeapon.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryNextWeapon;
                @InventaryPreviewWeapon.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryPreviewWeapon;
                @InventaryPreviewWeapon.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryPreviewWeapon;
                @InventaryPreviewWeapon.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryPreviewWeapon;
                @InventaryUseWeaponItem.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryUseWeaponItem;
                @InventaryUseWeaponItem.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryUseWeaponItem;
                @InventaryUseWeaponItem.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInventaryUseWeaponItem;
                @PutAwayExtractWeapon.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPutAwayExtractWeapon;
                @PutAwayExtractWeapon.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPutAwayExtractWeapon;
                @PutAwayExtractWeapon.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPutAwayExtractWeapon;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @AnalogMovement.started += instance.OnAnalogMovement;
                @AnalogMovement.performed += instance.OnAnalogMovement;
                @AnalogMovement.canceled += instance.OnAnalogMovement;
                @AnalogRotation.started += instance.OnAnalogRotation;
                @AnalogRotation.performed += instance.OnAnalogRotation;
                @AnalogRotation.canceled += instance.OnAnalogRotation;
                @InventaryNextWeapon.started += instance.OnInventaryNextWeapon;
                @InventaryNextWeapon.performed += instance.OnInventaryNextWeapon;
                @InventaryNextWeapon.canceled += instance.OnInventaryNextWeapon;
                @InventaryPreviewWeapon.started += instance.OnInventaryPreviewWeapon;
                @InventaryPreviewWeapon.performed += instance.OnInventaryPreviewWeapon;
                @InventaryPreviewWeapon.canceled += instance.OnInventaryPreviewWeapon;
                @InventaryUseWeaponItem.started += instance.OnInventaryUseWeaponItem;
                @InventaryUseWeaponItem.performed += instance.OnInventaryUseWeaponItem;
                @InventaryUseWeaponItem.canceled += instance.OnInventaryUseWeaponItem;
                @PutAwayExtractWeapon.started += instance.OnPutAwayExtractWeapon;
                @PutAwayExtractWeapon.performed += instance.OnPutAwayExtractWeapon;
                @PutAwayExtractWeapon.canceled += instance.OnPutAwayExtractWeapon;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Action;
    private readonly InputAction m_UI_MenuNavigation;
    public struct UIActions
    {
        private @PlayerInputAction m_Wrapper;
        public UIActions(@PlayerInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Action => m_Wrapper.m_UI_Action;
        public InputAction @MenuNavigation => m_Wrapper.m_UI_MenuNavigation;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Action.started -= m_Wrapper.m_UIActionsCallbackInterface.OnAction;
                @Action.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnAction;
                @Action.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnAction;
                @MenuNavigation.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMenuNavigation;
                @MenuNavigation.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMenuNavigation;
                @MenuNavigation.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMenuNavigation;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Action.started += instance.OnAction;
                @Action.performed += instance.OnAction;
                @Action.canceled += instance.OnAction;
                @MenuNavigation.started += instance.OnMenuNavigation;
                @MenuNavigation.performed += instance.OnMenuNavigation;
                @MenuNavigation.canceled += instance.OnMenuNavigation;
            }
        }
    }
    public UIActions @UI => new UIActions(this);
    private int m_XboxControlSchemeSchemeIndex = -1;
    public InputControlScheme XboxControlSchemeScheme
    {
        get
        {
            if (m_XboxControlSchemeSchemeIndex == -1) m_XboxControlSchemeSchemeIndex = asset.FindControlSchemeIndex("Xbox Control Scheme");
            return asset.controlSchemes[m_XboxControlSchemeSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnRun(InputAction.CallbackContext context);
        void OnAnalogMovement(InputAction.CallbackContext context);
        void OnAnalogRotation(InputAction.CallbackContext context);
        void OnInventaryNextWeapon(InputAction.CallbackContext context);
        void OnInventaryPreviewWeapon(InputAction.CallbackContext context);
        void OnInventaryUseWeaponItem(InputAction.CallbackContext context);
        void OnPutAwayExtractWeapon(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnAction(InputAction.CallbackContext context);
        void OnMenuNavigation(InputAction.CallbackContext context);
    }
}
