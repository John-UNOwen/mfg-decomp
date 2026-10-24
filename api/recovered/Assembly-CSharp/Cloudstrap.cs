using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Cloudstrap
{
	public enum UserEventType
	{
		None,
		Play,
		Gacha1,
		Gacha5,
		MusicGacha
	}

	public static class UserEvent
	{
		private static Dictionary<UserEventType, bool> _userEventActiveFlags = new Dictionary<UserEventType, bool>();

		public static bool IsPlayEvent(UserEventType userEvent)
		{
			if (userEvent == UserEventType.Play)
			{
				return true;
			}
			return false;
		}

		public static void Begin(UserEventType userevent)
		{
			if (userevent == UserEventType.None)
			{
				return;
			}
			if (IsCloudstrapEnable)
			{
				if (!ActiveFlag_IsActive(userevent))
				{
					DebugLog("prev count");
					NativeMethods.cstrap_uevent_begin(ToUserEventKind(userevent));
					DebugLog("after count");
				}
				ActiveFlag_Upsert(userevent, isActive: true);
			}
			if (IsPlayEvent(userevent))
			{
				ScopeStatus = StreamingScopeStatus.Connect;
			}
		}

		public static void End(UserEventType userevent)
		{
			if (userevent == UserEventType.None)
			{
				return;
			}
			if (IsPlayEvent(userevent))
			{
				ScopeStatus = StreamingScopeStatus.None;
			}
			if (IsCloudstrapEnable)
			{
				if (ActiveFlag_IsActive(userevent))
				{
					DebugLog("prev count");
					NativeMethods.cstrap_uevent_end(ToUserEventKind(userevent));
					DebugLog("after count");
				}
				ActiveFlag_Upsert(userevent, isActive: false);
			}
		}

		public static bool NeedCompensation(UserEventType userevent)
		{
			return GetCompensationCount(userevent) > 0;
		}

		private static int GetCompensationCount(UserEventType userevent)
		{
			if (userevent == UserEventType.None)
			{
				return 0;
			}
			int result = 0;
			if (IsCloudstrapEnable)
			{
				result = NativeMethods.cstrap_restart_get_count(ToUserEventKind(userevent));
			}
			return result;
		}

		public static void DebugLog(string msg = "")
		{
			if (!IsCloudstrapEnable)
			{
				return;
			}
			foreach (UserEventType value in Enum.GetValues(typeof(UserEventType)))
			{
				if (value != 0)
				{
					NativeMethods.cstrap_uevent_get_callcnt_begin(ToUserEventKind(value));
					NativeMethods.cstrap_uevent_get_callcnt_end(ToUserEventKind(value));
					NativeMethods.cstrap_restart_get_count(ToUserEventKind(value));
				}
			}
		}

		private static int ToUserEventKind(UserEventType userevent)
		{
			return (int)userevent;
		}

		private static void ActiveFlag_Upsert(UserEventType userEventType, bool isActive)
		{
			if (_userEventActiveFlags.ContainsKey(userEventType))
			{
				_userEventActiveFlags[userEventType] = isActive;
			}
			else
			{
				_userEventActiveFlags.Add(userEventType, isActive);
			}
		}

		private static bool ActiveFlag_IsActive(UserEventType userEventType)
		{
			if (_userEventActiveFlags.ContainsKey(userEventType))
			{
				return _userEventActiveFlags[userEventType];
			}
			return false;
		}
	}

	private static class NativeMethods
	{
		[DllImport("cloudstrap.exe")]
		public static extern bool cstrap_uevent_have_session();

		[DllImport("cloudstrap.exe")]
		public static extern void cstrap_uevent_begin(int userevent_kind);

		[DllImport("cloudstrap.exe")]
		public static extern void cstrap_uevent_end(int userevent_kind);

		[DllImport("cloudstrap.exe")]
		public static extern int cstrap_uevent_get_callcnt_begin(int userevent_kind);

		[DllImport("cloudstrap.exe")]
		public static extern int cstrap_uevent_get_callcnt_end(int userevent_kind);

		[DllImport("cloudstrap.exe")]
		public static extern int cstrap_restart_get_count(int userevent_kind);
	}

	public static StreamingScopeStatus ScopeStatus;

	private static bool IsCloudstrapEnable => GameUtility.IsCloudStreaming;

	public static bool IsDisconnect => ScopeStatus == StreamingScopeStatus.Disconnect;

	public static bool HaveSession()
	{
		if (IsCloudstrapEnable)
		{
			return NativeMethods.cstrap_uevent_have_session();
		}
		return true;
	}
}
