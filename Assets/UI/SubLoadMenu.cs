using UnityEngine;
/// <summary>
/// Sluzi ako zobrazovac vsetkych suborov ulozenia
/// </summary>
public class SubMenuLoad : MonoBehaviour
{
    [SerializeField] GameObject save;
    [SerializeField] Transform content;
    [SerializeField] AudioSource source;
    /// <summary>
    /// Nacita vsetky ulozenia a vytvori pre kazdy objekt v ponuke
    /// </summary>
    void Start()
    {
        // Vynuluje ponuku -> vymaze vsetky stare objekty ulozenia
        for (int i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        // Ziska vsetky cesty k uloznym suborom
        World[] saves = FileManager.GetSavedWorlds();

        for (int i = 0; i < saves.Length; i++)
        {
            MainUISave mus = Instantiate(save, content).GetComponent<MainUISave>();
            mus.SetUp(ref saves[i]);
            mus.SetAudioSource(source);
        }
    }
}
