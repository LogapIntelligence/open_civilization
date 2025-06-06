using open_civilization.Example;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace open_civilization
{

    // Program entry point
    class Program
    {
        static void Main(string[] args)
        {
            TextOnCube();
        }

        public static void TextOnCube()
        {
            var game = new TextOnCubeExample(new GameWindowSettings
            {

            }, new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Simple FPS Text Display Example - Running Average FPS",
            });

            game.Run();
        }

        public static void TextRenderExample()
        {
            var game = new TextRenderingExample();
            game.Run();
        }
        public static void RotatingCube()
        {
            var game = new RotatingCubeExample();
            game.Run();
        }
    }
}
