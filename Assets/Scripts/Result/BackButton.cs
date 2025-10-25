using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        // 戻るボタンがクリックされたときの処理
        SceneManager.LoadScene("SelectMusicScene");
    }
}
