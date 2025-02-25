using System.Collections.Generic;
using UnityEngine;

public class miniMap : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] RectTransform map;
    [SerializeField] Vector2 offset = new (0,0);
    [SerializeField] float modifier = 1;
    [SerializeField] GameObject RP; // remote player => vzdialeny hrac
    
    private List<RectTransform> rPlayers = new();

    /// <summary>
    /// Animuje pohyb hracov po mape
    /// </summary>
    void Update()
    {
        // Nastavuje poziciou mapy ukazovatel pozicie hlavneho hraca
        if (game.playerLives && game.LocalPlayer != null)
        {
            Vector2 pos = game.LocalPlayer.transform.position * modifier;
            pos += offset;

            map.anchoredPosition = pos;
        }

        // Vytvori ukazovaatele pre hracov na mape
        while (rPlayers.Count < game.RemotePlayers.Count)
            rPlayers.Add(Instantiate(RP, map).GetComponent<RectTransform>());
            
        // Odstrani nepotrebne ukazovatele
        for (int i = rPlayers.Count-1; game.RemotePlayers.Count-1 < i; i--)
        {
            Destroy(rPlayers[i].gameObject);
            rPlayers.RemoveAt(i);
        }
        
        // Nastavi pozicie pre hracov
        for (int i = 0; i < game.RemotePlayers.Count; i++)
        {
            if (game.RemotePlayers[i] != null)
                rPlayers[i].localPosition = OnMapPosition(game.RemotePlayers[i]);
            else
                game.RemotePlayers.RemoveAt(i);
        }
    }
    Vector2 OnMapPosition(Transform playerPos)
    {
        Vector2 pos = playerPos.position * (-1) * modifier;
        pos -= offset;

        return pos;
    }
}
