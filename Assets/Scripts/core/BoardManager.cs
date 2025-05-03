using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip swapClip;    // Inspector’dan sürükle-bırak için
    public AudioClip matchClip;   // eşleşme sesi

    private AudioSource audioSource;

    [Header("Match FX")]
    public GameObject matchBurstPrefab;


    /* 4 renk/tip – sprites[] sırasıyla aynı olmalı */
    public enum PieceType { Blue, Green, Yellow, Red }

    [Header("Board")]
    public Vector2Int boardSize = new(5, 5);
    public Tile tilePrefab;
    public Piece piecePrefab;
    public Sprite[] sprites;               // 4 ikon

    [SerializeField]
    private Color[] tintColors = {
    new Color32( 80,170,255,255),   // Mavi
    new Color32( 80,210, 80,255),   // Yeşil
    new Color32(255,220, 50,255),   // Sarı
    new Color32(255, 70, 70,255)    // Kırmızı
};


    /* Runtime verileri */
    private Tile[,] tiles;
    private Piece[,] pieces;
    private bool isSwapping;

    /*──────────────────────────────*/

   // void Awake() => GenerateBoard();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        GenerateBoard();
    }



    void GenerateBoard()
    {
        tiles = new Tile[boardSize.x, boardSize.y];
        pieces = new Piece[boardSize.x, boardSize.y];

        float halfX = (boardSize.x - 1) * 0.5f;
        float halfY = (boardSize.y - 1) * 0.5f;

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                // — merkezlenmiş pozisyon —
                Vector3 pos = new Vector3(
                    x - halfX,    // X sola kaydır
                    halfY - y,    // Y yukarıdan aşağı
                    0f
                );

                // 1) Tile oluştur
                var tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = pos;
                tiles[x, y] = tile;

                // 2) Piece oluştururken asla ardışık 3'lük yapmayacak şekilde renk seç
                int idx;
                do
                {
                    idx = Random.Range(0, sprites.Length);
                }
                // Eğer solunda iki aynı renk var ise tekrar
                while ((x >= 2 &&
                        pieces[x - 1, y] != null &&
                        pieces[x - 2, y] != null &&
                        pieces[x - 1, y].Type == (PieceType)idx &&
                        pieces[x - 2, y].Type == (PieceType)idx)
                       ||
                       // Eğer altında iki aynı renk var ise tekrar
                       (y >= 2 &&
                        pieces[x, y - 1] != null &&
                        pieces[x, y - 2] != null &&
                        pieces[x, y - 1].Type == (PieceType)idx &&
                        pieces[x, y - 2].Type == (PieceType)idx)
                      );

                // 3) Taşı instantiate edip init et
                var piece = Instantiate(piecePrefab, transform);
                piece.Init((PieceType)idx, sprites[idx], pos, true);
                piece.GetComponent<SpriteRenderer>().color = tintColors[idx];

                pieces[x, y] = piece;
            }
        }

        // (Opsiyonel) Kamera yakınlaştırmayı board yüksekliğine göre ayarlayın
        Camera.main.orthographicSize = boardSize.y * 0.6f;
    }





    /*──────────────────────────────*/

    public void TrySwap(Vector2Int a, Vector2Int b)
    {
        if (isSwapping) return;
        audioSource.PlayOneShot(swapClip);
        StartCoroutine(SwapRoutine(a, b));
        if (!InBounds(a) || !InBounds(b)) return;
        if (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) != 1) return;

        StartCoroutine(SwapRoutine(a, b));
    }

    bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.x < boardSize.x && c.y >= 0 && c.y < boardSize.y;

    IEnumerator SwapRoutine(Vector2Int a, Vector2Int b)
    {
        isSwapping = true;

        Piece pa = pieces[a.x, a.y];
        Piece pb = pieces[b.x, b.y];

        // 1) Hücrelere hareket
        pa.MoveTo(tiles[b.x, b.y], false);
        pb.MoveTo(tiles[a.x, a.y], false);

        // 2) Küçük “tilt” animasyonu
        pa.transform.DORotate(new Vector3(0, 0, 15), 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        pb.transform.DORotate(new Vector3(0, 0, -15), 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 3) Hareket tamamlanana kadar bekle
        yield return new WaitForSeconds(0.15f);

        // 4) Dizide yer değiştir
        pieces[a.x, a.y] = pb;
        pieces[b.x, b.y] = pa;

        if (FindMatches().Count == 0)
        {
            // eşleşme yoksa geri al
            pa.MoveTo(tiles[a.x, a.y], false);
            pb.MoveTo(tiles[b.x, b.y], false);
            yield return new WaitForSeconds(0.15f);
            pieces[a.x, a.y] = pa;
            pieces[b.x, b.y] = pb;
        }
        else
        {
            // eşleşme varsa temizle/doldur zincirini çalıştır
            yield return StartCoroutine(ClearAndFillRoutine());
        }

        isSwapping = false;
    }

    /*────────────────  EŞLEŞME BUL  ────────────────*/
    HashSet<Piece> FindMatches()
    {
        var matchSet = new HashSet<Piece>();

        /* Yatay */
        for (int y = 0; y < boardSize.y; y++)
        {
            int run = 1;
            for (int x = 1; x < boardSize.x; x++)
            {
                bool same = pieces[x, y] && pieces[x - 1, y] &&
                            pieces[x, y].Type == pieces[x - 1, y].Type;

                if (same)
                {
                    run++;
                    if (x == boardSize.x - 1 && run >= 3)
                        for (int k = 0; k < run; k++)
                            matchSet.Add(pieces[x - k, y]);
                }
                else
                {
                    if (run >= 3)
                        for (int k = 1; k <= run; k++)
                            matchSet.Add(pieces[x - k, y]);
                    run = 1;
                }
            }
        }

        /* Dikey */
        for (int x = 0; x < boardSize.x; x++)
        {
            int run = 1;
            for (int y = 1; y < boardSize.y; y++)
            {
                bool same = pieces[x, y] && pieces[x, y - 1] &&
                            pieces[x, y].Type == pieces[x, y - 1].Type;

                if (same)
                {
                    run++;
                    if (y == boardSize.y - 1 && run >= 3)
                        for (int k = 0; k < run; k++)
                            matchSet.Add(pieces[x, y - k]);
                }
                else
                {
                    if (run >= 3)
                        for (int k = 1; k <= run; k++)
                            matchSet.Add(pieces[x, y - k]);
                    run = 1;
                }
            }
        }

        return matchSet;
    }

    /*──────────────  TEMİZLE • DÜŞÜR • DOLDUR  ─────────────*/
    IEnumerator ClearAndFillRoutine()
    {
        while (true)
        {
            var match = FindMatches();
            if (match.Count == 0) break;

            DestroyPieces(match);
            yield return new WaitForSeconds(0.1f);

            for (int x = 0; x < boardSize.x; x++)
                CollapseColumn(x);

            yield return new WaitForSeconds(0.15f);

            RefillBoard();
            yield return new WaitForSeconds(0.15f);
        }
    }

    void DestroyPieces(HashSet<Piece> match)
    {
        foreach (Piece p in match)
        {
            if (p == null) continue;
            Vector2Int c = GetCoordsOf(p);
            if (!InBounds(c)) continue;

            // Eşleşme sesi
            audioSource.PlayOneShot(matchClip);

            // Delegate + animasyon vs...
            OnPieceCleared?.Invoke(p.Type);
            pieces[c.x, c.y] = null;
            StartCoroutine(PopAndDestroy(p));
        }
    }

    IEnumerator PopAndDestroy(Piece p)
    {
        // a) Ölçek “pop” animasyonu (büyüyüp küçülme)
        Vector3 orig = p.transform.localScale;
        Vector3 target = orig * 1.3f;
        float duration = 0.1f;
        float t = 0f;

        // Büyüme
        while (t < duration)
        {
            t += Time.deltaTime;
            p.transform.localScale = Vector3.Lerp(orig, target, t / duration);
            yield return null;
        }
        // Küçülme
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            p.transform.localScale = Vector3.Lerp(target, orig, t / duration);
            yield return null;
        }

        // b) Patlama efektini oluştur
        Instantiate(matchBurstPrefab, p.transform.position, Quaternion.identity);

        // c) Gerçek nesneyi yok et
        Destroy(p.gameObject);
    }







    void CollapseColumn(int x)
    {
        int writeRow = boardSize.y - 1;

        for (int y = boardSize.y - 1; y >= 0; y--)
        {
            if (pieces[x, y] != null)
            {
                if (y != writeRow)
                {
                    pieces[x, writeRow] = pieces[x, y];
                    pieces[x, y] = null;
                    pieces[x, writeRow].MoveTo(tiles[x, writeRow], false);
                }
                writeRow--;
            }
        }
    }

    void RefillBoard()
    {
        for (int x = 0; x < boardSize.x; x++)
            for (int y = 0; y < boardSize.y; y++)
            {
                if (pieces[x, y] == null)
                {
                    int idx = Random.Range(0, sprites.Length);
                    var piece = Instantiate(piecePrefab, transform);
                    piece.Init((PieceType)idx, sprites[idx], tiles[x, y].transform.position, false);
                    pieces[x, y] = piece;
                    piece.GetComponent<SpriteRenderer>().color = tintColors[idx];

                }
            }
    }

    /* Yardımcı: parçanın koordinatını bul */
    Vector2Int GetCoordsOf(Piece p)
    {
        for (int x = 0; x < boardSize.x; x++)
            for (int y = 0; y < boardSize.y; y++)
                if (pieces[x, y] == p)
                    return new Vector2Int(x, y);
        return new Vector2Int(-1, -1);
    }

    public System.Action<PieceType> OnPieceCleared;

    

    /* ResetBoard() seviyeye göre tahtayı baştan kurmak için */
    public void ResetBoard()
    {
        foreach (Transform t in transform) Destroy(t.gameObject);
        GenerateBoard();
    }



}
