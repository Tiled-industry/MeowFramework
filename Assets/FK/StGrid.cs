using System;
using System.Collections.Generic;

namespace Panty
{
    [Flags]
    public enum Dir4 : byte
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        All = 16,
    }
    // ��̬����
    [Serializable]
    public partial class StGrid
    {
        // �������½ǵ������ÿ�����ӵĿ�ȡ��߶�
        public float xMin, yMin, cw, ch;
        // ���������������
        public int row, colm;
        // ����������ܴ�С������*������
        public int Size => row * colm;
        // ���������һ������������
        public int SubRow => row >> 1;
        public int SubColm => colm >> 1;
        // ����������ܿ�Ⱥ͸߶�
        public float W => colm * cw;
        public float H => row * ch;
        // �����������ĵ�X��Y����
        public float CenterX => xMin + W * 0.5f;
        public float CenterY => yMin + H * 0.5f;
        // �����������Ͻǵ�X��Y����
        public float xMax => xMin + W;
        public float yMax => yMin + H;
        // ��������ĶԽ��߳���
        public float Hypotenuse => MathF.Sqrt(cw * cw + ch * ch);
        /// <summary>
        /// ���캯������ʼ���������������ÿ�����ӵĿ�ȡ��߶�
        /// </summary>
        /// <param name="row">����</param>
        /// <param name="colm">����</param>
        /// <param name="gw">�����ܿ��</param>
        /// <param name="gh">�����ܸ߶�</param>
        /// <param name="isCenter">�Ƿ�������Ϊԭ��</param>
        public StGrid(int row, int colm, float gw, float gh, bool isCenter = true)
        {
            this.row = row;
            this.colm = colm;
            if (isCenter)
            {
                xMin = -gw * 0.5f;
                yMin = -gh * 0.5f;
            }
            else
            {
                xMin = 0f;
                yMin = 0f;
            }
            cw = gw / colm;
            ch = gh / row;
        }
        /// <summary>
        /// ��һ�����캯����ʹ�����½������ÿ�����ӵĿ�ȡ��߶ȳ�ʼ������
        /// </summary>
        /// <param name="xMin">���½�X����</param>
        /// <param name="yMin">���½�Y����</param>
        /// <param name="cellW">ÿ�����ӵĿ��</param>
        /// <param name="cellH">ÿ�����ӵĸ߶�</param>
        /// <param name="numX">����</param>
        /// <param name="numY">����</param>
        public StGrid(float xMin, float yMin, float cellW, float cellH, int numX, int numY)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            cw = cellW;
            ch = cellH;
            colm = numX;
            row = numY;
        }
        /// <summary>
        /// ��ȡ�������ĵ����������
        /// </summary>
        /// <returns>�������ĵ����������</returns>
        public int CenterIndex_RowMajor()
        {
            return CellIndexToLinearIndex_RowMajor(row >> 1, colm >> 1);
        }
        /// <summary>
        /// ���������ĵ����������С
        /// </summary>
        /// <param name="deltaX">X��������ű���</param>
        /// <param name="deltaY">Y��������ű���</param>
        public void ScaleFromCenter(float deltaX, float deltaY)
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
        /// <summary>
        /// �϶����������С
        /// </summary>
        /// <param name="dir">�����ķ���</param>
        /// <param name="deltaX">X������϶�����</param>
        /// <param name="deltaY">Y������϶�����</param>
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
        /// <summary>
        /// ֱ�ӵ��������С
        /// </summary>
        /// <param name="dir">�����ķ���</param>
        /// <param name="deltaX">X����ĵ�����</param>
        /// <param name="deltaY">Y����ĵ�����</param>
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
        /// ��ȡĳ�������ϵ��ھ�����
        /// </summary>
        /// <param name="r">������</param>
        /// <param name="c">������</param>
        /// <param name="direction">����</param>
        /// <returns>�Ƿ�����ھ�����</returns>
        public bool GetNeighborIndex(ref int r, ref int c, Dir4 direction)
        {
            switch (direction)
            {
                case Dir4.Up: r += 1; break;
                case Dir4.Down: r -= 1; break;
                case Dir4.Left: c -= 1; break;
                case Dir4.Right: c += 1; break;
                case Dir4.Left | Dir4.Up:
                    r += 1;
                    c -= 1;
                    break;
                case Dir4.Left | Dir4.Down:
                    r -= 1;
                    c -= 1;
                    break;
                case Dir4.Right | Dir4.Up:
                    r += 1;
                    c += 1;
                    break;
                case Dir4.Right | Dir4.Down:
                    r -= 1;
                    c += 1;
                    break;
                default: return false;
            }
            return r >= 0 && r < row && c >= 0 && c < colm;
        }
        /// <summary>
        /// ��ȡ����������ھ����������ڱ߽紩Խ��
        /// </summary>
        /// <param name="index">��ǰ����</param>
        /// <param name="direction">����</param>
        public void GetWrappedNeighborIndex(ref int index, Dir4 direction)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            GetWrappedNeighborIndex(ref r, ref c, direction);
            index = CellIndexToLinearIndex_RowMajor(r, c);
        }
        /// <summary>
        /// ��ȡ����������ھ����������ڱ߽紩Խ��
        /// </summary>
        /// <param name="r">������</param>
        /// <param name="c">������</param>
        /// <param name="direction">����</param>
        public void GetWrappedNeighborIndex(ref int r, ref int c, Dir4 direction)
        {
            switch (direction)
            {
                case Dir4.Up: r = (r + 1) % row; break;
                case Dir4.Down: r = (r - 1 + row) % row; break;
                case Dir4.Left: c = (c - 1 + colm) % colm; break;
                case Dir4.Right: c = (c + 1) % colm; break;

                case Dir4.Left | Dir4.Up:
                    r = (r + 1) % row;
                    c = (c - 1 + colm) % colm;
                    break;
                case Dir4.Left | Dir4.Down:
                    r = (r - 1 + row) % row;
                    c = (c - 1 + colm) % colm;
                    break;
                case Dir4.Right | Dir4.Up:
                    r = (r + 1) % row;
                    c = (c + 1) % colm;
                    break;
                case Dir4.Right | Dir4.Down:
                    r = (r - 1 + row) % row;
                    c = (c + 1) % colm;
                    break;
            }
        }
        /// <summary>
        /// ��ʵ������ת��Ϊ�����е���������
        /// </summary>
        /// <param name="x">ʵ��X����</param>
        /// <param name="y">ʵ��Y����</param>
        /// <param name="r">������</param>
        /// <param name="c">������</param>
        public void CoordToCellIndex(float x, float y, out int r, out int c)
        {
            r = (int)((y - yMin) / ch);
            c = (int)((x - xMin) / cw);
        }
        /// <summary>
        /// ���������������ת��Ϊ�����������������������ӵײ���ʼ������
        /// </summary>
        /// <param name="rIndex">������</param>
        /// <param name="cIndex">������</param>
        /// <returns>��������</returns>
        public int InvCellIndexToLinearIndex_RowMajor(int rIndex, int cIndex) =>
                    (row - 1 - rIndex) * colm + cIndex;
        /// <summary>
        /// ���������������ת��Ϊ����������������
        /// </summary>
        /// <param name="rIndex">������</param>
        /// <param name="cIndex">������</param>
        /// <returns>��������</returns>
        public int CellIndexToLinearIndex_RowMajor(int rIndex, int cIndex) =>
            rIndex * colm + cIndex;
        /// <summary>
        /// ���������������ת��Ϊ����������������
        /// </summary>
        /// <param name="rIndex">������</param>
        /// <param name="cIndex">������</param>
        /// <returns>��������</returns>
        public int CellIndexToLinearIndex_ColMajor(int rIndex, int cIndex) =>
            cIndex * row + rIndex;
        /// <summary>
        /// ����������ת��Ϊ���������������������
        /// </summary>
        /// <param name="index">��������</param>
        /// <param name="r">������</param>
        /// <param name="c">������</param>
        public void LinearIndexToCellIndex_RowMajor(int index, out int r, out int c)
        {
            r = index / colm;
            c = index % colm;
        }
        /// <summary>
        /// ����������ת��Ϊ�����������꣨������
        /// </summary>
        /// <param name="index">��������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        public void LinearIndexToCoordCenter_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToCoordCenter(r, c, out x, out y);
        }
        /// <summary>
        /// ����������ת��Ϊ�������½����꣨������
        /// </summary>
        /// <param name="index">��������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        public void LinearIndexToWorldCoord_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToWorldCoord(r, c, out x, out y);
        }
        public void InvLinearIndexToWorldCoord_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToWorldCoord(row - 1 - r, c, out x, out y);
        }
        /// <summary>
        /// ���������������ת��Ϊ��������
        /// </summary>
        /// <param name="rIndex">������</param>
        /// <param name="cIndex">������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        public void CellIndexToCoordCenter(int rIndex, int cIndex, out float x, out float y)
        {
            x = xMin + (cIndex + 0.5f) * cw;
            y = yMin + (rIndex + 0.5f) * ch;
        }
        /// <summary>
        /// ���������������ת��Ϊʵ������
        /// </summary>
        /// <param name="rIndex">������</param>
        /// <param name="cIndex">������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        public void CellIndexToWorldCoord(int rIndex, int cIndex, out float x, out float y)
        {
            x = xMin + cIndex * cw;
            y = yMin + rIndex * ch;
        }
        /// <summary>
        /// ���ĳ�������Ƿ��������ڣ������ض�Ӧ�ı߽���Ϣ
        /// </summary>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        /// <param name="half">�߽ǵľ���</param>
        /// <returns>������Ϣ</returns>
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
        /// <summary>
        /// ���ĳ�����Ƿ���������
        /// </summary>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        /// <returns>�Ƿ���������</returns>
        public bool Contains(float x, float y) =>
            x >= xMin && x < xMax && y >= yMin && y < yMax;
        /// <summary>
        /// ˮƽ����ĳ��������
        /// </summary>
        /// <param name="cIndex">������</param>
        public void HorMirror(ref int cIndex) => cIndex = colm - 1 - cIndex;
        /// <summary>
        /// ��ֱ����ĳ��������
        /// </summary>
        /// <param name="rIndex">������</param>
        public void VerMirror(ref int rIndex) => rIndex = row - 1 - rIndex;
        /// <summary>
        /// ����������������������ö����
        /// </summary>
        /// <returns>����������ö��</returns>
        public IEnumerable<int> RowMajorLinear()
        {
            for (int i = 0, len = row * colm; i < len; i++)
                yield return i;
        }
        /// <summary>
        /// ����������������������ö����
        /// </summary>
        /// <returns>����������ö��</returns>
        public IEnumerable<(int r, int c)> RowMajorIndices()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (r, c);
        }
        /// <summary>
        /// ������������Ͻǿ�ʼ��������������ö����
        /// </summary>
        /// <returns>����������ö��</returns>
        public IEnumerable<(int r, int c)> RowMajorIndicesByLeftUp()
        {
            for (int r = row - 1; r >= 0; r--)
                for (int c = 0; c < colm; c++)
                    yield return (r, c);
        }
        /// <summary>
        /// ������������Ͻǿ�ʼ���������ö����
        /// </summary>
        /// <returns>�����ö��</returns>
        public IEnumerable<(float x, float y)> RowMajorCoordsByLeftUp()
        {
            for (int r = row - 1; r >= 0; r--)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + (r + 1) * ch);
        }
        /// <summary>
        /// �����������������ö��
        /// </summary>
        /// <returns>�����ö��</returns>
        public IEnumerable<(float x, float y)> RowMajorCoords()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + r * ch);
        }
        /// <summary>
        /// �������򴴽�һά����
        /// </summary>
        /// <typeparam name="T">����Ԫ������</typeparam>
        /// <param name="creator">��������</param>
        /// <returns>����������</returns>
        public T[] CreateArrayRowMajor<T>(Func<int, int, T> creator)
        {
            T[] arr = new T[row * colm];
            for (int r = 0, index = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    arr[index++] = creator(r, c);
            return arr;
        }
        /// <summary>
        /// �������򴴽���ά����
        /// </summary>
        /// <typeparam name="T">����Ԫ������</typeparam>
        /// <param name="creator">��������</param>
        /// <returns>�����Ķ�ά����</returns>
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