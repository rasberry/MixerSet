using System.Runtime.InteropServices;

namespace MixerSet
{
	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	class MMDeviceEnumerator
	{
	}

	enum EDataFlow
	{
		eRender,
		eCapture,
		eAll,
		EDataFlow_enum_count
	}

	enum ERole
	{
		eConsole,
		eMultimedia,
		eCommunications,
		ERole_enum_count
	}

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMDeviceEnumerator
	{
		int NotImpl1();

		[PreserveSig]
		int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

		// the rest is not implemented
	}

	[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMDevice
	{
		[PreserveSig]
		int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

		// the rest is not implemented
	}

	[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioSessionManager2
	{
		int NotImpl_GetAudioSessionControl();
		int NotImpl_GetSimpleAudioVolume();

		[PreserveSig]
		int GetSessionEnumerator(out IAudioSessionEnumerator sessionEnum);

		// the rest is not implemented
	}

	[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioSessionEnumerator
	{
		[PreserveSig]
		int GetCount(out int sessionCount);

		[PreserveSig]
		int GetSession(int sessionCount, out IAudioSessionControl2 session);
	}

	[Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioSessionControl2
	{
		//== IAudioSessionControl

		[PreserveSig]
		int GetState(out AudioSessionState state);

		[PreserveSig]
		int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string name);

		[PreserveSig]
		int SetDisplayName(string value, Guid eventContext);

		[PreserveSig]
		int GetIconPath(out IntPtr path);

		// Assigns a display icon to the current session.
		[PreserveSig]
		int SetIconPath(string value, Guid eventContext);

		[PreserveSig]
		int GetGroupingParam(out Guid groupingParam);

		[PreserveSig]
		int SetGroupingParam(Guid Override, Guid eventcontext);

		[PreserveSig]
		int NotImpl_RegisterAudioSessionNotification(/*IAudioSessionEvents NewNotifications*/);

		[PreserveSig]
		int NotImpl_UnregisterAudioSessionNotification(/*IAudioSessionEvents NewNotifications*/);

		//== IAudioSessionControl2

		[PreserveSig]
		int GetSessionIdentifier([Out] [MarshalAs(UnmanagedType.LPWStr)] out string sessionId);

		[PreserveSig]
		int GetSessionInstanceIdentifier([Out] [MarshalAs(UnmanagedType.LPWStr)] out string instanceId);

		[PreserveSig]
		int GetProcessId(out UInt32 retvVal);

		[PreserveSig]
		int IsSystemSoundsSession();

		[PreserveSig]
		int SetDuckingPreference(bool optOut);
	}

	[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ISimpleAudioVolume
	{
		[PreserveSig]
		int SetMasterVolume(float fLevel, ref Guid eventContext);

		[PreserveSig]
		int GetMasterVolume(out float pfLevel);

		[PreserveSig]
		int SetMute(bool bMute, ref Guid eventContext);

		[PreserveSig]
		int GetMute(out bool pbMute);
	}

	enum AudioSessionState
	{
		Inactive = 0,
		Active = 1,
		Expired = 2
	}
}
