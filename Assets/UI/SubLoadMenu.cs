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
    /// Obnoví ponuku uloženia <br />
    /// Načíta všetky uložené svety a vytvorí pre každý zápis v ponuke
    /// </summary>
    void OnEnable()
    {
        // Vynuluje ponuku -> vymaže všetky staré zápisy uloženia
        for (int i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        // Získa všetky cesty k uloženým súborom
        World[] saves = FileManager.GetSavedWorlds();

        // Pre každý získany svet
        for (int i = 0; i < saves.Length; i++)
        {
            // Vytvorí zápis v zozname
            MainUISave mus = Instantiate(save, content).GetComponent<MainUISave>();

            // Nastaví zápisu hodnoty sveta
            mus.SetUp(ref saves[i]);

            // Nastaví zvuk prehraný po kliknutí 
            mus.SetAudioSource(source);
        }
    }
}
