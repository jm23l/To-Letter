using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName;

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public TMP_Text uidText;

    void Start() { uidText.text = AuthManager.Name; }

    public void Logout() { AuthManager.Logout(); }       // 로그아웃 버튼
}
