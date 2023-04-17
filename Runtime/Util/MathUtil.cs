namespace Util {
    public static class MathUtil {
        public static int NextMultipleOf(int n, int factor) {
            if (n <= 0) return 1;
            if (n % factor == 0) return n;
            return n + (factor - (n % factor));
        }
    }
}