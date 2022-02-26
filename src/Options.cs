namespace MixerSet
{
	public static class Options
	{
		public static Commands ArgAction = Commands.None;
		public static float ArgVol = 0.0f;
		public static string ArgAppName = null;

		public static void Usage()
		{
			Console.WriteLine("Usage: "
				+"\n" + nameof(MixerSet) + " (command) [options]"
				+"\n Commands: "
				+"\n    list                Lists apps in volume mixer"
				+"\n    reset [n]           Resets all volumes to 0 or n"
				+"\n    app (appname) (n)   Sets appname volume to n"
			);
		}

		public static bool ParseArgs(string[] args)
		{
			for(int a=0; a<args.Length; a++)
			{
				string arg = args[0];
				if (arg == "list" || arg == "l") {
					ArgAction = Commands.List;
				} else if (arg == "reset" || arg == "r") {
					ArgAction = Commands.Reset;
					if (++a >= args.Length || !float.TryParse(args[a],out ArgVol)) {
						ArgVol = 0;
					}
				} else if (arg == "app" || arg == "a") {
					ArgAction = Commands.App;
					if (++a >= args.Length || String.IsNullOrEmpty(args[a])) {
						Console.Error.WriteLine("Error: missing or bad application name");
						return false;
					} else {
						ArgAppName = args[a];
					}
					if (++a >= args.Length || !float.TryParse(args[a],out ArgVol)) {
						Console.Error.WriteLine("Error: missing or bad volume parameter");
						return false;
					}
				}
			}
			return true;
		}
	}
}