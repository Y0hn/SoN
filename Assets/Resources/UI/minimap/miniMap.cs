using UnityEngine;

public class miniMap : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] RectTransform map;
    [SerializeField] Vector2 offset = new (0,0);
    [SerializeField] float modifier = 1;
    /// <summary>
    /// Animuje pohyb lokalneho hraca
    /// </summary>
    void FixedUpdate()
    {
        if (game.playerLives && game.LocalPlayer != null)
        {
            Vector2 pos = game.LocalPlayer.transform.position * modifier;
            pos += offset;

            map.anchoredPosition = pos;
        }
    }
}
