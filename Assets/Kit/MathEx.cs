using System;

namespace Panty
{
    #region �����ʼ�
    // 2��N�η�    => [1 << n]
    // N*2��E�η�  => [n << e]
    // N/2��E�η�  => [n >> e]
    // 1=-1ȡ��    => [~n + 1] 
    // AND �ж�o�� => [(n & 1) == 0]
    // (int) Math.Log10(damage) + 1 �������ֳ���
    // (char)('0' + n % 10) ��һ�����������λ(��λ)ת��Ϊ��Ӧ���ַ���ʾ '0'= 48(ASCII)
    // ��������ֵ��������� => [i - count/2] �����ż�� ��Ҫ +0.5f ������
    // �Ƿ���0-Max��Χ�� => [(uint)n < max] ���n��һ��������ת��uintԽ�磬���һ���ܴ������
    // cur / max �� 0 - max ӳ�䵽 0 - 1;
    // cur / (max - min) ����Сֵ-���ֵ�ķ�Χ ӳ�䵽 0 - 1
    // maxA / max * cur; ���ֵA to ���ֵB [��Сֵ����0�����]
    #endregion
    public static class MathEx
    {
        public const float PI = 3.14159274F;
        public const float PI2 = PI * 2f;
        public const float Rad90 = PI * 0.5f;
        public const float Rad45 = PI * 0.25f;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;

        // �����ֵ [λ�����Ż�] -2147483648 ����� 
        public static int Abs(this int n) => (n + (n >> 31)) ^ (n >> 31);
        public static float Abs(this float a) => a >= 0f ? a : -a;

        public static void Max(this ref float x, float y) => x = x > y ? x : y;
        public static void Min(this ref float x, float y) => x = x < y ? x : y;
        public static void Max(this ref int x, int y) => x = x > y ? x : y;
        public static void Min(this ref int x, int y) => x = x < y ? x : y;

        public static void Clamp(this ref float v, float min, float max) => v = v < min ? min : (v > max ? max : v);
        public static void Clamp(this ref int v, int min, int max) => v = v < min ? min : (v > max ? max : v);
        public static void Clamp01(this ref float v) => v = v < 0f ? 0f : (v > 1f ? 1f : v);

        public static void MoveTowards(this ref float cur, float target, float step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0f ? step : -step) : target;
        public static void MoveTowardsF(this ref int cur, int target, int step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0 ? step : -step) : target;

        public static void Swap<T>(this ref T a, ref T b) where T : struct { T c = a; a = b; b = c; }
        public static float Normalize(this float cur, float min, float max) => (max - cur) / (max - min);
    }
    public static class RandomEx
    {
        private static Random random = new Random();
        public static int Range(int max) => random.Next(max);
        public static int Range(int min, int max) => random.Next(min, max);
        public static float NextFloat(float min, float max) => ((float)random.NextDouble() * (max - min)) + min;
        public static double NextDouble(double min, double max) => random.NextDouble() * (max - min) + min;
        public static double Range01() => random.NextDouble();
    }
}