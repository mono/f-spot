// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLibSharp {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[GLib.CDeclCallback]
	internal delegate bool IOSchedulerJobFuncNative(IntPtr job, IntPtr cancellable, IntPtr user_data);

	internal class IOSchedulerJobFuncInvoker {

		IOSchedulerJobFuncNative native_cb;
		IntPtr __data;
		GLib.DestroyNotify __notify;

		~IOSchedulerJobFuncInvoker ()
		{
			if (__notify == null)
				return;
			__notify (__data);
		}

		internal IOSchedulerJobFuncInvoker (IOSchedulerJobFuncNative native_cb) : this (native_cb, IntPtr.Zero, null) {}

		internal IOSchedulerJobFuncInvoker (IOSchedulerJobFuncNative native_cb, IntPtr data) : this (native_cb, data, null) {}

		internal IOSchedulerJobFuncInvoker (IOSchedulerJobFuncNative native_cb, IntPtr data, GLib.DestroyNotify notify)
		{
			this.native_cb = native_cb;
			__data = data;
			__notify = notify;
		}

		internal GLib.IOSchedulerJobFunc Handler {
			get {
				return new GLib.IOSchedulerJobFunc(InvokeNative);
			}
		}

		bool InvokeNative (GLib.IOSchedulerJob job, GLib.Cancellable cancellable)
		{
			bool result = native_cb (job == null ? IntPtr.Zero : job.Handle, cancellable == null ? IntPtr.Zero : cancellable.Handle, __data);
			return result;
		}
	}

	internal class IOSchedulerJobFuncWrapper {

		public bool NativeCallback (IntPtr job, IntPtr cancellable, IntPtr user_data)
		{
			try {
				bool __ret = managed (job == IntPtr.Zero ? null : (GLib.IOSchedulerJob) GLib.Opaque.GetOpaque (job, typeof (GLib.IOSchedulerJob), false), GLib.Object.GetObject(cancellable) as GLib.Cancellable);
				if (release_on_call)
					gch.Free ();
				return __ret;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
				return false;
			}
		}

		bool release_on_call = false;
		GCHandle gch;

		public void PersistUntilCalled ()
		{
			release_on_call = true;
			gch = GCHandle.Alloc (this);
		}

		internal IOSchedulerJobFuncNative NativeDelegate;
		GLib.IOSchedulerJobFunc managed;

		public IOSchedulerJobFuncWrapper (GLib.IOSchedulerJobFunc managed)
		{
			this.managed = managed;
			if (managed != null)
				NativeDelegate = new IOSchedulerJobFuncNative (NativeCallback);
		}

		public static GLib.IOSchedulerJobFunc GetManagedDelegate (IOSchedulerJobFuncNative native)
		{
			if (native == null)
				return null;
			IOSchedulerJobFuncWrapper wrapper = (IOSchedulerJobFuncWrapper) native.Target;
			if (wrapper == null)
				return null;
			return wrapper.managed;
		}
	}
#endregion
}
