using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace explorer
{
    class Program {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "LearnOpenTK - Camera",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };
            // 'using' ensures proper disposal of resources when the Game object is no longer needed
            using (Game game = new Game(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Run();
            }

            {
                // Start the game loop
                // The Run() method usually contains the main update-render loop

            } // At this point, the 'game' object is automatically disposed, freeing any resources it used
        }
    }

}

