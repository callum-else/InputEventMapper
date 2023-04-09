using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputEventMapper : MonoBehaviour
{
    private bool _isInitialised = false;
    private PlayerInput _playerInput;
    private Dictionary<string, InputAction> _actionCache;
    private UnityEvent _actionMappings = new UnityEvent();
    
    private void OnEnable()
    {
        if (!_isInitialised)
        {
            _playerInput = GetComponent<PlayerInput>();
            _actionCache = new Dictionary<string, InputAction>();
            _actionMappings.Invoke();
            _isInitialised = true;
        }
    }

    private void ModifyActionForPhase(string inputAction, BindablePhase phase, Action<InputAction.CallbackContext> onAction, bool isSubscribing)
    {
        if (_isInitialised)
        {
            if (!_actionCache.ContainsKey(inputAction))
                _actionCache.Add(inputAction, _playerInput.actions.FindAction(inputAction));

            var action = _actionCache[inputAction];

            if (action != null)
            {
                if (phase.HasFlag(BindablePhase.Started) || phase.HasFlag(BindablePhase.All))
                {
                    if (isSubscribing) action.started += onAction;
                    else action.started -= onAction;
                }

                if (phase.HasFlag(BindablePhase.Performed) || phase.HasFlag(BindablePhase.All))
                {
                    if (isSubscribing) action.performed += onAction;
                    else action.performed -= onAction;
                }

                if (phase.HasFlag(BindablePhase.Canceled) || phase.HasFlag(BindablePhase.All))
                {
                    if (isSubscribing) action.canceled += onAction;
                    else action.canceled -= onAction;
                }
            }
        }
        else
            _actionMappings.AddListener(() => ModifyActionForPhase(inputAction, phase, onAction, isSubscribing));
    }

    public void Subscribe(string inputAction, BindablePhase phase, Action<InputAction.CallbackContext> onAction)
    {
        ModifyActionForPhase(inputAction, phase, onAction, isSubscribing: true);
        Debug.Log($"Subscribed {onAction.Method.DeclaringType.Name}.{onAction.Method.Name} to {inputAction}.{phase}");
    }

    public void Unsubscribe(string inputAction, BindablePhase phase, Action<InputAction.CallbackContext> onAction)
        => ModifyActionForPhase(inputAction, phase, onAction, isSubscribing: false);
}

public enum BindablePhase : byte
{
    Started = 0,
    Performed = 1,
    Canceled = 2,
    All = 4,
}