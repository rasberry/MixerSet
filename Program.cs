using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace MixerSet
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || !ParseArgs(args)) {
				Usage();
				return;
			}

			switch(ArgAction)
			{
			case Commands.List: ActionList(); break;
			case Commands.Reset: ActionReset(ArgVol); break;
			case Commands.App: ActionApp(ArgAppName,ArgVol); break;
			}
		}

		static void ActionList()
		{
			int index = 0;
			foreach (ApplicationInfo app in EnumerateApplications())
			{
				Console.WriteLine(
					(index == 0 ? "" : "\n")
					+ "#"+index+": " + Path.GetFileNameWithoutExtension(app.ProcName)
					+ "\nState: "+AudioSessionStateToDisplay(app.State)
					+ "\t Mute: "+GetApplicationMute(app.ProcId)
					+ "\t Vol: "+GetApplicationVolume(app.ProcId)
				);
				index++;
			}
		}

		static void ActionReset(float vol)
		{
			foreach (ApplicationInfo app in EnumerateApplications())
			{
				SetApplicationVolume(app.ProcId,vol);
			}
		}

		static void ActionApp(string name, float vol)
		{
			int index = 0;
			foreach (ApplicationInfo app in EnumerateApplications())
			{
				string sindex = index.ToString();
				string pname = Path.GetFileNameWithoutExtension(app.ProcName);
				//Console.WriteLine(name+" == "+sindex+" ("+(name == sindex)+") "+name+" == "+pname+" ("+(0 == String.Compare(name,pname,true))+")");
				if (name == sindex || 0 == String.Compare(name,pname,true))
				{
					var prev = GetApplicationVolume(app.ProcId);
					SetApplicationVolume(app.ProcId, vol);
					Console.WriteLine(pname+" "+prev+" -> "+vol);
					break;
				}
				index++;
			}
		}

		static Commands ArgAction = Commands.None;
		static float ArgVol = 0.0f;
		static string ArgAppName = null;

		enum Commands {
			None
			,List
			,Reset
			,App
		}

		static void Usage()
		{
			Console.WriteLine("Usage: "
				+"\n" + nameof(MixerSet) + "(command) [options]"
				+"\n Commands: "
				+"\n    list                Lists apps in volume mixer"
				+"\n    reset [n]           Resets all volumes to 0 or n"
				+"\n    app (appname) (n)   Sets appname volume to n"
			);
		}

		static bool ParseArgs(string[] args)
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

		static Guid GetGuid(Type t)
		{
			var att = t.GetTypeInfo().GetCustomAttribute<GuidAttribute>();
			return new Guid(att.Value);
		}

		static float? GetApplicationVolume(uint processId)
		{
			ISimpleAudioVolume volume = GetVolumeObject(processId);
			if (volume == null) {
				return null;
			}

			float level;
			volume.GetMasterVolume(out level);
			return level * 100;
		}

		static bool? GetApplicationMute(uint processId)
		{
			ISimpleAudioVolume volume = GetVolumeObject(processId);
			if (volume == null) {
				return null;
			}

			bool mute;
			volume.GetMute(out mute);
			return mute;
		}

		static void SetApplicationVolume(uint processId, float level)
		{
			ISimpleAudioVolume volume = GetVolumeObject(processId);
			if (volume == null) {
				return;
			}

			Guid guid = Guid.Empty;
			volume.SetMasterVolume(level / 100, ref guid);
		}

		static void SetApplicationMute(uint processId, bool mute)
		{
			ISimpleAudioVolume volume = GetVolumeObject(processId);
			if (volume == null) {
				return;
			}

			Guid guid = Guid.Empty;
			volume.SetMute(mute, ref guid);
		}

		public struct ApplicationInfo
		{
			public string Name;
			public AudioSessionState State;
			public uint ProcId;
			public string ProcName;
		}

		static IEnumerable<ApplicationInfo> EnumerateApplications()
		{
			// get the speakers (1st render + multimedia) device
			IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
			IMMDevice speakers;
			deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

			// activate the session manager. we need the enumerator
			Guid IID_IAudioSessionManager2 = GetGuid(typeof(IAudioSessionManager2));
			object o;
			speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
			IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			IAudioSessionEnumerator sessionEnumerator;
			mgr.GetSessionEnumerator(out sessionEnumerator);
			int count;
			sessionEnumerator.GetCount(out count);

			for (int i = 0; i < count; i++)
			{
				IAudioSessionControl2 ctl;
				sessionEnumerator.GetSession(i, out ctl);
				string dn;
				int errgdn = ctl.GetDisplayName(out dn);
				uint procid;
				int errgpi = ctl.GetProcessId(out procid);
				AudioSessionState state;
				ctl.GetState(out state);
				bool isss = ctl.IsSystemSoundsSession() == 0;
				string fn = isss ? "System" : Process.GetProcessById((int)procid).MainModule.FileName;
				//Console.WriteLine("=> "+errgdn+"|"+errgpi+"|"+isss+"|"+AudioSessionStateToDisplay(state)+"|"+procid+"|"+dn+"|"+fn);

				yield return new ApplicationInfo {
					Name = dn
					,State = state
					,ProcId = procid
					,ProcName = fn
				};
				Marshal.ReleaseComObject(ctl);
			}
			Marshal.ReleaseComObject(sessionEnumerator);
			Marshal.ReleaseComObject(mgr);
			Marshal.ReleaseComObject(speakers);
			Marshal.ReleaseComObject(deviceEnumerator);
		}

		static string AudioSessionStateToDisplay(AudioSessionState state)
		{
			switch(state)
			{
			case AudioSessionState.AudioSessionStateActive: return "Playing";
			case AudioSessionState.AudioSessionStateExpired: return "Expired";
			case AudioSessionState.AudioSessionStateInactive: return "Silent";
			}
			return "Unkown";
		}

		static ISimpleAudioVolume GetVolumeObject(uint processId)
		{
			// get the speakers (1st render + multimedia) device
			IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
			IMMDevice speakers;
			deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

			// activate the session manager. we need the enumerator
			Guid IID_IAudioSessionManager2 = GetGuid(typeof(IAudioSessionManager2));
			object o;
			speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
			IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			IAudioSessionEnumerator sessionEnumerator;
			mgr.GetSessionEnumerator(out sessionEnumerator);
			int count;
			sessionEnumerator.GetCount(out count);

			// search for an audio session with the required process id
			ISimpleAudioVolume volumeControl = null;
			for (int i = 0; i < count; i++)
			{
				IAudioSessionControl2 ctl;
				sessionEnumerator.GetSession(i, out ctl);
				uint pid;
				ctl.GetProcessId(out pid);
				if (processId == pid)
				{
					volumeControl = ctl as ISimpleAudioVolume;
					break;
				}
				Marshal.ReleaseComObject(ctl);
			}
			Marshal.ReleaseComObject(sessionEnumerator);
			Marshal.ReleaseComObject(mgr);
			Marshal.ReleaseComObject(speakers);
			Marshal.ReleaseComObject(deviceEnumerator);
			return volumeControl;
		}
	}
}