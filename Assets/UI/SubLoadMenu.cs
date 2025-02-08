using UnityEngine;
/// <summary>
/// Sluzi ako zobrazovac vsetkych savevov
/// </summary>
public class SubMenuLoad : MonoBehaviour
{
    [SerializeField] GameObject save;
    [SerializeField] Transform content;
    [SerializeField] AudioSource source;
    void Start()
    {
        for (int i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        World[] saves = FileManager.GetSavedWorlds();

        for (int i = 0; i < saves.Length; i++)
        {
            MainUISave mus = Instantiate(save, content).GetComponent<MainUISave>();
            mus.SetUp(ref saves[i]);
            mus.SetAudioSource(source);
        }
    }
}
