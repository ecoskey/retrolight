using System;
using Data;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Util {
    public static class MathUtils {
        /*public static int NextMultipleOf(int n, int factor) {
            if (n <= 0) return 1;
            if (n % factor == 0) return n;
            return n + (factor - (n % factor));
        }*/

        //returns minimum x such that x * factor >= n
        /*public static int NextMultipleOf(int n, int factor) {
            if (n <= 0) return 1;
            if (n % factor == 0) return n / factor;
            return n / factor + 1;
        }*/

        public static int NextMultipleOf(int n, int factor) =>
            Math.Max(1, (n + factor - 1) / factor);
    }
}