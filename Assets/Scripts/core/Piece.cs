using System.Collections;
using UnityEngine;
using static BoardManager;                // PieceType eri�imi

public class Piece : MonoBehaviour
{
    /* Ta��n rengi/tipi */
    public PieceType Type { get; private set; }

    /* BoardManager yeni ta� olu�tururken �a��r�r */
    public void Init(PieceType type, Sprite sprite, Vector3 pos, bool instant = true)
    {
        Type = type;
        GetComponent<SpriteRenderer>().sprite = sprite;

        if (instant)
            transform.position = pos;
        else
            StartCoroutine(SmoothMove(pos, 0.15f));
    }

    /* Takas & d��me esnas�nda �a�r�l�r */
    public void MoveTo(Tile targetTile, bool instant)
    {
        if (instant)
            transform.position = targetTile.transform.position;
        else
            StartCoroutine(SmoothMove(targetTile.transform.position, 0.15f));
    }

    /* Basit Lerp animasyonu (DOTween yok) */
    IEnumerator SmoothMove(Vector3 target, float time)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = target;
    }
}
