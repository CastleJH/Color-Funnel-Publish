using System;
using System.Collections.Generic;


public class Cell
{
    public int answer;
    public int row, col;   //셀의 행과 열 정보
    public Queue<int> elems; //셀이 가진 색깔들

    //인수 clr로 cell을 꽉 채운다. 0이면 비우는 것을 의미함.
    public void fill(int clr)
    {
        answer = clr;
        if (clr == 0) elems.Clear();
        else for (int i = 0; i < 4; i++) elems.Enqueue(clr);
    }

    //cell c에 색깔을 준다. 주는 것에 실패하면 false 반환.
    public bool give(ref Cell c)
    {
        if (c.elems.Count == 4 || elems.Count == 0) return false;
        if (Math.Abs(row - c.row) != 1 && Math.Abs(col - c.col) != 1) return false;

        c.elems.Enqueue(elems.Dequeue());
        return true;
    }

    //cell이 가지고 있는 색깔정보를 버퍼에 저장한다. 왼쪽이 queue의 front이다.
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

    //원소 개수가 4개 미만이거나 가지고 있는 원소들의 색깔이 모두 동일한 경우가 아니라면 true를 반환한다. 
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
    int[] wxy = { '왼', '오', '위', '아' };

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
     (정답 1번원소 2번원소 3번원소 4번원소) * 셀 개수
     0은 empty를 의미
    
     예시)
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

        //정답을 만든다. 동시에 버퍼에 저장한다.
            
        int tryCnt = 100;
        while (clrs < row * col)
        {
            r = UnityEngine.Random.Range(0, row);
            c = UnityEngine.Random.Range(0, col);
            if (cells[r, c].elems.Count == 0)
                cells[r, c].fill(clrs++);
            if (--tryCnt == 0) return false;
        }

        //정답 cells에서 시작하여 랜덤하게 섞는다. 
        //(1)"섞기 성공 횟수"가 mix에 도달하거나
        //(2)"섞기 횟수"가 최대 시도치(mix * 100)에 도달할 때까지 섞는다.
        tryCnt = mix * 100;
        do
        {
            tryCnt--;

            //섞을 셀 선택
            r = UnityEngine.Random.Range(0, row);
            c = UnityEngine.Random.Range(0, col);

            //섞을 방향 성택
            do dir = UnityEngine.Random.Range(0, 4);
            while (r + dxy[dir, 0] < 0 || r + dxy[dir, 0] >= row
            || c + dxy[dir, 1] < 0 || c + dxy[dir, 1] >= col);

            //섞기 성공시 섞은 정보 저장 후 "섞기 성공 횟수" 갱신
            if (cells[r, c].give(ref cells[r + dxy[dir, 0], c + dxy[dir, 1]])) mix--;
        } while (mix > 0 && tryCnt > 0);

        //"섞기 횟수"가 최대 시도치에 도달했음에도 "섞기 성공 횟수"가 mix에 도달하지 못했으면 에러.
        if (mix != 0) 
            return false;

        //모든 셀들이 적당하게 섞였는지 확인한다. 실패시 에러.
        for (r = 0; r < row; r++)
            for (c = 0; c < col; c++)
                if (!cells[r, c].isMixed())
                    return false;

        //섞은 후 최종 셀의 형태(==출제할 문제)를 버퍼에 저장한다.
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                cells[i, j].printCell(ref buffer);

        return true;
    }
}

