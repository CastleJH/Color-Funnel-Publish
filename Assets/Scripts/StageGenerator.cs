using System;
using System.Collections.Generic;


public class Cell
{
    public int answer;
    public int row, col;   //���� ��� �� ����
    public Queue<int> elems; //���� ���� �����

    //�μ� clr�� cell�� �� ä���. 0�̸� ���� ���� �ǹ���.
    public void fill(int clr)
    {
        answer = clr;
        if (clr == 0) elems.Clear();
        else for (int i = 0; i < 4; i++) elems.Enqueue(clr);
    }

    //cell c�� ������ �ش�. �ִ� �Ϳ� �����ϸ� false ��ȯ.
    public bool give(ref Cell c)
    {
        if (c.elems.Count == 4 || elems.Count == 0) return false;
        if (Math.Abs(row - c.row) != 1 && Math.Abs(col - c.col) != 1) return false;

        c.elems.Enqueue(elems.Dequeue());
        return true;
    }

    //cell�� ������ �ִ� ���������� ���ۿ� �����Ѵ�. ������ queue�� front�̴�.
    public void printCell(ref List<int> buffer)
    {
        buffer.Add(answer);
        
        for (int i = elems.Count; i < 4; i++) buffer.Add(0);

        for (int i = 0; i < elems.Count; i++)
        {
            buffer.Add(elems.Dequeue());
            elems.Enqueue(buffer[buffer.Count - 1]);
        }

    }

    //���� ������ 4�� �̸��̰ų� ������ �ִ� ���ҵ��� ������ ��� ������ ��찡 �ƴ϶�� true�� ��ȯ�Ѵ�. 
    public bool isMixed()
    {
        bool ret = false;
        if (elems.Count != 4) return true;

        int first = elems.Peek();
        for (int i = 0; i < elems.Count; i++)
        {
            if (elems.Peek() != first) ret = true;
            int e = elems.Dequeue();
            elems.Enqueue(e);
        }
        return ret;
    }
}

public class StageGenerator
{
    public Cell[,] cells = new Cell[4, 3];
    int[,] dxy = new int[,]{ {0, 1}, {0, -1}, {1, 0}, {-1, 0} };
    int[] wxy = { '��', '��', '��', '��' };

    public void InitializeGenerator()
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
            {
                cells[i, j] = new Cell();
                cells[i, j].row = i;
                cells[i, j].col = j;
                cells[i, j].elems = new Queue<int>();
            }
    }

    /*
     (���� 1������ 2������ 3������ 4������) * �� ����
     0�� empty�� �ǹ�
    
     ����)
     0 0 0 0 1 
     1 2 2 0 4 
     2 5 1 5 1
     ...
     0 2 7 7 3
     */

    public bool generateStage(ref List<int> buffer, int row, int col, int mix)
    {
        buffer.Clear();
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                cells[i, j].fill(0);

        int clrs = 0;
        int r, c, dir;

        //������ �����. ���ÿ� ���ۿ� �����Ѵ�.
            
        int tryCnt = 100;
        while (clrs < row * col)
        {
            r = UnityEngine.Random.Range(0, row);
            c = UnityEngine.Random.Range(0, col);
            if (cells[r, c].elems.Count == 0)
                cells[r, c].fill(clrs++);
            if (--tryCnt == 0) return false;
        }

        //���� cells���� �����Ͽ� �����ϰ� ���´�. 
        //(1)"���� ���� Ƚ��"�� mix�� �����ϰų�
        //(2)"���� Ƚ��"�� �ִ� �õ�ġ(mix * 100)�� ������ ������ ���´�.
        tryCnt = mix * 100;
        do
        {
            tryCnt--;

            //���� �� ����
            r = UnityEngine.Random.Range(0, row);
            c = UnityEngine.Random.Range(0, col);

            //���� ���� ����
            do dir = UnityEngine.Random.Range(0, 4);
            while (r + dxy[dir, 0] < 0 || r + dxy[dir, 0] >= row
            || c + dxy[dir, 1] < 0 || c + dxy[dir, 1] >= col);

            //���� ������ ���� ���� ���� �� "���� ���� Ƚ��" ����
            if (cells[r, c].give(ref cells[r + dxy[dir, 0], c + dxy[dir, 1]])) mix--;
        } while (mix > 0 && tryCnt > 0);

        //"���� Ƚ��"�� �ִ� �õ�ġ�� ������������ "���� ���� Ƚ��"�� mix�� �������� �������� ����.
        if (mix != 0) 
            return false;

        //��� ������ �����ϰ� �������� Ȯ���Ѵ�. ���н� ����.
        for (r = 0; r < row; r++)
            for (c = 0; c < col; c++)
                if (!cells[r, c].isMixed())
                    return false;

        //���� �� ���� ���� ����(==������ ����)�� ���ۿ� �����Ѵ�.
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                cells[i, j].printCell(ref buffer);

        return true;
    }
}

