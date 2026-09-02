namespace RaymarchEngine.Core.Rendering
{
    /// <summary>
    /// Screen resolution
    /// </summary>
    public class Resolution
    {
        private int width;
        private int height;

        /// <summary>
        /// Create new screen resolution
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Resolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Screen resolution width in px
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Screen resolution height in px
        /// </summary>
        public int Height => height;
    }
}