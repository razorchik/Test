
using UnityEngine;
using UnityEngine.UI;

internal sealed class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private Text playersCount;

    [SerializeField]
    private Text playerText;

    [SerializeField]
    private Text startSessionText;

    public void UpdatePlayerText(string name)
    {
        playerText.text = string.Format("Welcome: {0}", name);
    }

    public void UpdatePlayersCount(int count)
    {
        playersCount.text = string.Format("Players in lobby: {0}", count);
        startSessionText.gameObject.SetActive(count > 0);
    }
}
