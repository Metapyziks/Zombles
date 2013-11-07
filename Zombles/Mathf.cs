namespace Zombles
{
    public static class Mathf
    {
        public static float Floor(float val)
        {
            if (val >= 0f) {
                return (int) val;
            } else {
                int i = (int) val;
                return i == val ? i : (int) (val - 1);
            }
        }

        public static float Ceil(float val)
        {
            if (val <= 0f) {
                return (int) val;
            } else {
                int i = (int) val;
                return i == val ? i : (int) (val + 1);
            }
        }
    }
}
