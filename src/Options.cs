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
				+"\n    (l)ist                Lists apps in volume mixer"
				+"\n    (r)eset [n]           Resets all volumes to 0 or n"
				+"\n    (a)pp (appname) (n)   Sets appname volume to n"
				+"\n    (m)ute (appname)      Mutes or un-mutes appname"
			);
		}

		public static bool ParseArgs(string[] args)
		{
			for(int a=0; a<args.Length; a++)
			{
				string arg = args[0];
				if (arg == "list" || arg == "l") {
					ArgAction = Commands.List;
				}
				else if (arg == "reset" || arg == "r") {
					ArgAction = Commands.Reset;
					if (++a >= args.Length || !float.TryParse(args[a],out ArgVol)) {
						ArgVol = 0;
					}
				}
				else if (arg == "app" || arg == "a") {
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
				else if (arg == "mute" || arg == "m") {
					ArgAction = Commands.Mute;
					if (++a >= args.Length || String.IsNullOrEmpty(args[a])) {
						Console.Error.WriteLine("Error: missing or bad application name");
						return false;
					} else {
						ArgAppName = args[a];
					}
				}
			}
			return true;
		}
	}
}