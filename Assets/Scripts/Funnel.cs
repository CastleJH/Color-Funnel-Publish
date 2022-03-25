using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Funnel : MonoBehaviour
{
    public GameManager manager;
    public SpriteRenderer highlight;
    public SpriteRenderer[] spriteRenderers;
    public LinkedList<int> colorElems;
    public BoxCollider2D collider;
    
    public Vector2 originalPosition;
    public int row;
    public int col;

    void Start()
    {
        colorElems = new LinkedList<int>();
        collider = gameObject.GetComponent<BoxCollider2D>();
    }

    public void SetOriginalPosition(int maxRow, int maxCol, int _row, int _col)
    {
        row = _row;
        col = _col;
        originalPosition.y = 0.65f * (maxRow - 1) - row * 1.3f;
        originalPosition.x = -0.65f * (maxCol - 1) + col * 1.3f;
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    public bool IsUnited()
    {
        if (colorElems.Count == 0) return true;
        if (colorElems.Count == 4)
        {
            for (int i = 1; i < 4; i++)
                if (spriteRenderers[i].color != spriteRenderers[0].color)
                    return false;
            return true;
        }
        return false;
    }

    public void InitializeColor(int a, int b, int c, int d)
    {
        colorElems.Clear();

        if (a != 0) colorElems.AddLast(a);
        spriteRenderers[0].color = manager.intToColor[a];

        if (b != 0) colorElems.AddLast(b);
        spriteRenderers[1].color = manager.intToColor[b];

        if (c != 0) colorElems.AddLast(c);
        spriteRenderers[2].color = manager.intToColor[c];

        if (d != 0) colorElems.AddLast(d);
        spriteRenderers[3].color = manager.intToColor[d];
    }

    public void Fill(int clr)
    {
        for (int i = 0; i < 4; i++)
        {
            colorElems.AddLast(clr);
            spriteRenderers[i].color = manager.intToColor[clr + 1];
        }
    }

    public void Empty()
    {
        colorElems.Clear();
        for (int i = 0; i < 4; i++) spriteRenderers[i].color = manager.intToColor[0];
    }

    public bool GiveColor(Funnel funnel)
    {
        if (funnel.colorElems.Count < 4 && colorElems.Count > 0)
        {
            Color clr = spriteRenderers[0].color;
            for (int i = 0; i < colorElems.Count - 1; i++)
                spriteRenderers[i].color = spriteRenderers[i + 1].color;
            spriteRenderers[colorElems.Count - 1].color = manager.intToColor[0];

            funnel.colorElems.AddLast(colorElems.First.Value);
            colorElems.RemoveFirst();
            funnel.spriteRenderers[funnel.colorElems.Count - 1].color = clr;

            return true;
        }
        return false;
    }

    public bool RegainColor(Funnel funnel)
    {
        if (colorElems.Count < 4 && funnel.colorElems.Count > 0)
        {
            Color clr = funnel.spriteRenderers[funnel.colorElems.Count - 1].color;
            for (int i = colorElems.Count - 1; i >= 0; i--)
                spriteRenderers[i + 1].color = spriteRenderers[i].color;
            spriteRenderers[0].color = clr;

            colorElems.AddFirst(funnel.colorElems.Last.Value);
            funnel.colorElems.RemoveLast();
            funnel.spriteRenderers[funnel.colorElems.Count].color = manager.intToColor[0];
            return true;
        }
        return false;
    }
}
