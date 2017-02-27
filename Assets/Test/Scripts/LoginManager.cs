using System;

using GameSparks.Api.Requests;
using GameSparks.Api.Responses;

using UnityEngine;
using UnityEngine.UI;

internal sealed class LoginManager : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private InputField passwordField;

    public event Action onRegistration = delegate { };
    
    public void Login()
    {
        var request = new RegistrationRequest();

        request.SetDisplayName(inputField.text);
        request.SetUserName(inputField.text);
        request.SetPassword(passwordField.text);
        request.Send(OnRegistration);
    }

    private void OnRegistration(RegistrationResponse response)
    {
        if (response.HasErrors)
        {
            if (response.NewPlayer != true)
            {
                var request = new AuthenticationRequest();

                request.SetUserName(inputField.text);
                request.SetPassword(passwordField.text);
                request.Send(OnAuthentication);

                Debug.LogWarning("GSM| Existing User, Switching to Authentication");
            }
            else
            {
                Debug.LogWarning("GSM| Error Registration User" + response.Errors.JSON);
            }

            return;
        }

        OnRegistrationComplete();
    }

    private void OnAuthentication(AuthenticationResponse response)
    {
        if (response.HasErrors)
        {
            Debug.LogWarning("GSM| Error Authenticating User" + response.Errors.JSON);
            return;
        }

        OnRegistrationComplete();
    }

    private void OnRegistrationComplete()
    {
        gameObject.SetActive(false);
        onRegistration();
    }
}
