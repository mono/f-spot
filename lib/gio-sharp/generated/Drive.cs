// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;

#region Autogenerated code
	public interface Drive : GLib.IWrapper {

		event System.EventHandler Changed;
		event System.EventHandler EjectButton;
		event System.EventHandler Disconnected;
		event System.EventHandler StopButton;
		void Stop(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void PollForMedia(GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool StartFinish(GLib.AsyncResult result);
		GLib.Icon Icon { 
			get;
		}
		bool HasMedia { 
			get;
		}
		bool CanEject();
		void Start(GLib.DriveStartFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string EnumerateIdentifiers();
		GLib.List Volumes { 
			get;
		}
		bool IsMediaCheckAutomatic { 
			get;
		}
		void EjectWithOperation(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool CanStartDegraded();
		GLib.DriveStartStopType StartStopType { 
			get;
		}
		bool PollForMediaFinish(GLib.AsyncResult result);
		bool StopFinish(GLib.AsyncResult result);
		void Eject(GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string Name { 
			get;
		}
		string GetIdentifier(string kind);
		bool IsMediaRemovable { 
			get;
		}
		bool HasVolumes { 
			get;
		}
		bool EjectWithOperationFinish(GLib.AsyncResult result);
		bool CanStop();
		bool CanStart();
		bool CanPollForMedia();
		bool EjectFinish(GLib.AsyncResult result);
	}
#endregion
}
