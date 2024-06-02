using System;
using System.Collections.Generic;

namespace Panty
{
    // ��̬����
    [Serializable]
    public partial class StGrid
    {
        // ���½� �� ���ӿ��
        public float xMin, yMin, cw, ch;
        // ����������
        public int row, colm;
        public float W => colm * cw;
        public float H => row * ch;
        public float CenterX => xMin + W * 0.5f;
        public float CenterY => yMin + H * 0.5f;
        public float xMax => xMin + W;
        public float yMax => yMin + H;
        // ��ȡ�����б�߳���
        public float Hypotenuse => MathF.Sqrt(cw * cw + ch * ch);

        public StGrid(int row, int colm, float gw, float gh)
        {
            this.row = row;
            this.colm = colm;
            xMin = -gw * 0.5f;
            yMin = -gh * 0.5f;
            cw = gw / colm;
            ch = gh / row;
        }
        public StGrid(float xMin, float yMin, float cellW, float cellH, int numX, int numY)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            cw = cellW;
            ch = cellH;
            colm = numX;
            row = numY;
        }
        public void ResizeByCenter(float deltaX, float deltaY)
        {
            float w = W;
            float h = H;
            // ʹ��������ƽ���������⿪����
            float dragMagnitudeSquared = deltaX * deltaX + deltaY * deltaY;
            // ͨ���Ƚ�ƽ��������������С���ǷŴ� ����Ŵ󣬷�����С
            int sign = (deltaX * w + deltaY * h > 0 ? 1 : -1);
            // �������ű��������⿪���ţ�ֱ����ƽ���ı���������
            float scale = sign * MathF.Sqrt(dragMagnitudeSquared / (w * w + h * h)) + 1;
            // Ӧ������
            float newWidth = w * scale;
            float newHeight = h * scale;
            // ���������С
            cw = newWidth / colm;
            ch = newHeight / row;
            // ���ݵ������������ʼ��
            xMin = xMin + w * 0.5f - newWidth * 0.5f;
            yMin = yMin + h * 0.5f - newHeight * 0.5f;
        }
        public void DragResize(Dir4 dir, float deltaX, float deltaY)
        {
            switch (dir)
            {
                case Dir4.Left | Dir4.Up:
                    cw -= deltaX / colm;
                    ch += deltaY / row;
                    xMin += deltaX;
                    break;
                case Dir4.Right | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    break;
                case Dir4.Left | Dir4.Down:
                    cw -= deltaX / colm;
                    ch -= deltaY / row;
                    xMin += deltaX;
                    yMin += deltaY;
                    break;
                case Dir4.Right | Dir4.Down:
                    cw += deltaX / colm;
                    ch -= deltaY / row;
                    yMin += deltaY;
                    break;
            }
        }
        public void Resize(Dir4 dir, float deltaX, float deltaY)
        {
            switch (dir)
            {
                case Dir4.Up:
                    ch += deltaY / row;
                    break;
                case Dir4.Down:
                    ch += deltaY / row;
                    yMin -= deltaY;
                    break;
                case Dir4.Left:
                    cw += deltaX / colm;
                    xMin -= deltaX;
                    break;
                case Dir4.Right:
                    cw += deltaX / colm;
                    break;
                case Dir4.Left | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    xMin -= deltaX;
                    break;
                case Dir4.Right | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    break;
                case Dir4.Left | Dir4.Down:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    xMin -= deltaX;
                    yMin -= deltaY;
                    break;
                case Dir4.Right | Dir4.Down:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    yMin -= deltaY;
                    break;
                case Dir4.All:
                    cw += deltaX * 2f / colm;
                    ch += deltaY * 2f / row;
                    xMin -= deltaX;
                    yMin -= deltaY;
                    break;
            }
        }
        /// <summary>
        /// ���������ȡXY���� ��ʼ�㵽�����Ĳ� ���� ���
        /// </summary>
        public (int r, int c) CoordToCellIndex(float x, float y) =>
            ((int)((y - yMin) / ch), (int)((x - xMin) / cw));
        public int CellIndexToLinearIndex_RowMajor(int rIndex, int cIndex) =>
            rIndex * colm + cIndex;
        public int CellIndexToLinearIndex_ColMajor(int rIndex, int cIndex) =>
            cIndex * row + rIndex;
        public (int r, int c) LinearIndexToCellIndex_RowMajor(int index) =>
            (index / colm, index % colm);
        public (float x, float y) CellIndexToCoordCenter(int rIndex, int cIndex) =>
            (xMin + (cIndex + 0.5f) * cw, yMin + (rIndex + 0.5f) * ch);
        public (float x, float y) CellIndexToWorldCoord(int rIndex, int cIndex) =>
            (xMin + cIndex * cw, yMin + rIndex * ch);
        public Dir4 CheckEdgeCorner(float x, float y, float half)
        {
            // �������½ǵľ������½ǵ�
            float minx = xMin - half;
            float miny = yMin - half;
            float maxx = xMin + half;
            float maxy = yMin + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Left | Dir4.Down;
            float maxY = yMax;
            minx = xMin - half;
            miny = maxY - half;
            maxx = xMin + half;
            maxy = maxY + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Left | Dir4.Up;
            float maxX = xMax;
            minx = maxX - half;
            miny = yMin - half;
            maxx = maxX + half;
            maxy = yMin + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Right | Dir4.Down;
            minx = maxX - half;
            miny = maxY - half;
            maxx = maxX + half;
            maxy = maxY + half;
            return x >= minx && x < maxx && y >= miny && y < maxy ?
                Dir4.Right | Dir4.Up : Dir4.None;
        }
        public bool Contains(float x, float y) =>
            x >= xMin && x < xMax && y >= yMin && y < yMax;
        public void HorMirror(ref int cIndex) => cIndex = colm - 1 - cIndex;
        public void VerMirror(ref int rIndex) => rIndex = row - 1 - rIndex;
        public IEnumerable<int> RowMajorLinear()
        {
            for (int i = 0, len = row * colm; i < len; i++)
                yield return i;
        }
        public IEnumerable<(int r, int c)> RowMajorIndices()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (r, c);
        }
        public IEnumerable<(float x, float y)> RowMajorCoordsByLeftUp()
        {
            for (int r = row - 1; r >= 0; r--)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + (r + 1) * ch);
        }
        public IEnumerable<(float x, float y)> RowMajorCoords()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + r * ch);
        }
        public T[] CreateArrayRowMajor<T>(Func<int, int, T> creator)
        {
            T[] arr = new T[row * colm];
            for (int r = 0, index = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    arr[index++] = creator(r, c);
            return arr;
        }
        public T[,] Create2DArrayRowMajor<T>(Func<int, int, T> creator)
        {
            T[,] array = new T[row, colm];
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    array[r, c] = creator(r, c);
            return array;
        }
    }
}