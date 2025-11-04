using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    private ValidatedSyncVar<int> _testVar = new(100);
    
    private void OnEnable()
    {
        _testVar.serverValidation += ServerValidator;
        _testVar.onValidationFail += OnValidationFail;

        _testVar.onChangedWithOld += OnChanged;
    }

    private void OnDisable()
    {
        _testVar.serverValidation -= ServerValidator;
        _testVar.onValidationFail -= OnValidationFail;
        
        _testVar.onChangedWithOld -= OnChanged;
    }

    private bool ServerValidator(int oldValue, int newValue)
    {
        if (oldValue > newValue)
            return false;
        
        return true;
    }

    private void OnValidationFail(int failedValue, int authoritativeValue)
    {
        Debug.Log($"Validation failed on {failedValue}. Returning to {authoritativeValue}");
    }

    private void OnChanged(int oldValue, int newValue, bool validated)
    {
        Debug.Log($"Value changed: {oldValue} -> {newValue} | Validated: {validated}");
    }
    

    private void Update()
    {
        if (!isOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            _testVar.value += 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            _testVar.value -= 1;
    }
}
