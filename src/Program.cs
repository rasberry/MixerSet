using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace MixerSet
{
	public class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || !Options.ParseArgs(args)) {
				Options.Usage();
				return;
			}

			switch(Options.ArgAction)
			{
			case Commands.List: ActionList(); break;
			case Commands.Reset: ActionReset(Options.ArgVol); break;
			case Commands.App: ActionApp(Options.ArgAppName,Options.ArgVol); break;
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

			volume.GetMasterVolume(out float level);
			return level * 100;
		}

		static bool? GetApplicationMute(uint processId)
		{
			ISimpleAudioVolume volume = GetVolumeObject(processId);
			if (volume == null) {
				return null;
			}

			volume.GetMute(out bool mute);
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

		static IEnumerable<ApplicationInfo> EnumerateApplications()
		{
			// get the speakers (1st render + multimedia) device
			IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
			deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice speakers);

			// activate the session manager. we need the enumerator
			Guid IID_IAudioSessionManager2 = GetGuid(typeof(IAudioSessionManager2));
			speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
			IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			mgr.GetSessionEnumerator(out IAudioSessionEnumerator sessionEnumerator);
			sessionEnumerator.GetCount(out int count);

			for (int i = 0; i < count; i++)
			{
				sessionEnumerator.GetSession(i, out IAudioSessionControl2 ctl);
				int errgdn = ctl.GetDisplayName(out string dn);
				int errgpi = ctl.GetProcessId(out uint procid);
				ctl.GetState(out AudioSessionState state);
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
			case AudioSessionState.Active: return "Playing";
			case AudioSessionState.Expired: return "Expired";
			case AudioSessionState.Inactive: return "Silent";
			}
			return "Unkown";
		}

		static ISimpleAudioVolume GetVolumeObject(uint processId)
		{
			// get the speakers (1st render + multimedia) device
			IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
			deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice speakers);

			// activate the session manager. we need the enumerator
			Guid IID_IAudioSessionManager2 = GetGuid(typeof(IAudioSessionManager2));
			speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
			IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			mgr.GetSessionEnumerator(out IAudioSessionEnumerator sessionEnumerator);
			sessionEnumerator.GetCount(out int count);

			// search for an audio session with the required process id
			ISimpleAudioVolume volumeControl = null;
			for (int i = 0; i < count; i++)
			{
				sessionEnumerator.GetSession(i, out IAudioSessionControl2 ctl);
				ctl.GetProcessId(out uint pid);
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
