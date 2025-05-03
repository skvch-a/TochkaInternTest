using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            data.Add(line.ToCharArray().ToList());
        }

        return data;
    }

    static int Solve(List<List<char>> grid)
    {
        var graph = BuildGraph(grid, out var keyIndexCount);
        var allKeysMask = (1 << keyIndexCount) - 1;
        var minDistDict = new Dictionary<long, int>();
        var curPos = new [] { 0, 1, 2, 3 };
        var startState = PackState(curPos, 0);
        var pq = new PriorityQueue<long, int>();
        minDistDict[startState] = 0;
        pq.Enqueue(startState, 0);

        while (pq.Count > 0)
        { 
            pq.TryDequeue(out var curState, out var curDist);
            if (minDistDict[curState] < curDist)
                continue;
            
            UnpackState(curState, curPos, out var curMask);
            if (curMask == allKeysMask)
                return curDist;

            for (var i = 0; i < 4; i++)
            {
                foreach (var (v, reqMask, dist) in graph[curPos[i]])
                {
                    var keyBit = 1 << (v - 4);
                    if ((curMask & reqMask) != reqMask || (curMask & keyBit) != 0)
                        continue;
                    
                    var newDist = curDist + dist;
                    var newPos = curPos.ToArray();
                    newPos[i] = v;
                    
                    var newState = PackState(newPos, curMask | keyBit);
                    if (minDistDict.TryGetValue(newState, out var oldDist) && newDist >= oldDist)
                        continue;

                    minDistDict[newState] = newDist;
                    pq.Enqueue(newState, newDist);
                }
            }
        }

        return -1;
    }

    private static long PackState(int[] robotsPos, int keyMask)
    {
        long s = keyMask;
        s |= (long)robotsPos[0] << 26;
        s |= (long)robotsPos[1] << 31;
        s |= (long)robotsPos[2] << 36;
        s |= (long)robotsPos[3] << 41;
        return s;
    }

    private static void UnpackState(long state, int[] robotsPos, out int keyMask)
    {
        keyMask = (int)(state & ((1 << 26) - 1));
        robotsPos[0] = (int)((state >> 26) & 0x1F);
        robotsPos[1] = (int)((state >> 31) & 0x1F);
        robotsPos[2] = (int)((state >> 36) & 0x1F);
        robotsPos[3] = (int)((state >> 41) & 0x1F);
    }

    private static List<(int v, int reqMask, int dist)> GetEdges(
        List<List<char>> grid,
        int startRow,
        int startCol,
        Dictionary<char, int> keyIndex)
    {
        var rowsCount = grid.Count;
        var columnsCount = grid[0].Count;
        var edges = new List<(int v, int reqMask, int dist)>();
        var visited = new bool[rowsCount, columnsCount];
        var queue = new Queue<(int row, int col, int mask, int dist)>();
        visited[startRow, startCol] = true;
        queue.Enqueue((startRow, startCol, 0, 0));

        while (queue.Count > 0)
        {
            var (row, col, mask, dist) = queue.Dequeue();
            for (var direction = 0; direction < 4; direction++)
            {
                var newRow = row + "2101"[direction] - '1';
                var newCol = col + "1210"[direction] - '1';

                if (newRow < 0 || newRow >= rowsCount || newCol < 0 || newCol >= columnsCount ||
                    visited[newRow, newCol])
                    continue;

                var ch = grid[newRow][newCol];
                if (ch == '#')
                    continue;

                var newMask = mask;
                if (ch is >= 'A' and <= 'Z')
                    newMask |= 1 << Array.IndexOf(doors_char, ch);

                visited[newRow, newCol] = true;
                if (ch is >= 'a' and <= 'z')
                    edges.Add((keyIndex[ch] + 4, newMask, dist + 1));

                queue.Enqueue((newRow, newCol, newMask, dist + 1));
            }
        }

        return edges;
    }

    private static List<List<(int v, int reqMask, int dist)>> BuildGraph(List<List<char>> grid, out int keyIndexCount)
    {
        var rowsCount = grid.Count;
        var columnsCount = grid[0].Count;
        var keyIndex = new Dictionary<char, int>();
        var points = new List<(int, int)>();

        for (var i = 0; i < rowsCount; i++)
        for (var j = 0; j < columnsCount; j++)
        {
            var ch = grid[i][j];
            switch (ch)
            {
                case '@':
                    points.Add((i, j));
                    break;
                case >= 'a' and <= 'z':
                    keyIndex.TryAdd(ch, 0);
                    break;
            }
        }

        var idx = 0;
        var sortedKeys = keyIndex.Keys.OrderBy(c => c).ToList();
        foreach (var k in sortedKeys)
            keyIndex[k] = idx++;

        foreach (var k in sortedKeys)
        {
            for (var i = 0; i < rowsCount; i++)
            for (var j = 0; j < columnsCount; j++)
                if (grid[i][j] == k)
                    points.Add((i, j));
        }

        var graph = new List<List<(int v, int reqMask, int dist)>>(points.Count);
        foreach (var (row, column) in points)
            graph.Add(GetEdges(grid, row, column, keyIndex));

        keyIndexCount = keyIndex.Count;
        return graph;
    }

    static void Main()
    {
        var data = GetInput();
        var result = Solve(data);

        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}