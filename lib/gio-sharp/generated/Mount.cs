// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;

#region Autogenerated code
	public interface Mount : GLib.IWrapper {

		event System.EventHandler PreUnmount;
		event System.EventHandler Changed;
		event System.EventHandler Unmounted;
		string GuessContentTypeSync(bool force_rescan, GLib.Cancellable cancellable);
		bool UnmountFinish(GLib.AsyncResult result);
		GLib.Volume Volume { 
			get;
		}
		GLib.File Root { 
			get;
		}
		bool CanUnmount { 
			get;
		}
		GLib.Icon Icon { 
			get;
		}
		bool CanEject();
		void Shadow();
		bool UnmountWithOperationFinish(GLib.AsyncResult result);
		void EjectWithOperation(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void GuessContentType(bool force_rescan, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void Remount(GLib.MountMountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string GuessContentTypeFinish(GLib.AsyncResult result);
		bool RemountFinish(GLib.AsyncResult result);
		void Eject(GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string Name { 
			get;
		}
		GLib.Drive Drive { 
			get;
		}
		void UnmountWithOperation(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void Unmount(GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string Uuid { 
			get;
		}
		void Unshadow();
		bool EjectWithOperationFinish(GLib.AsyncResult result);
		bool IsShadowed { 
			get;
		}
		bool EjectFinish(GLib.AsyncResult result);
	}
#endregion
}
