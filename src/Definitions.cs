namespace MixerSet
{
	public enum Commands {
		None = 0,
		List = 1,
		Reset = 2,
		App = 3,
		Mute = 4
	}

	struct ApplicationInfo
	{
		public string Name;
		public AudioSessionState State;
		public uint ProcId;
		public string ProcName;
	}
}