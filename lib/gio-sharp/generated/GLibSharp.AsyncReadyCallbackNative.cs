// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLibSharp {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[GLib.CDeclCallback]
	internal delegate void AsyncReadyCallbackNative(IntPtr source_object, IntPtr res, IntPtr user_data);

	internal class AsyncReadyCallbackInvoker {

		AsyncReadyCallbackNative native_cb;
		IntPtr __data;
		GLib.DestroyNotify __notify;

		~AsyncReadyCallbackInvoker ()
		{
			if (__notify == null)
				return;
			__notify (__data);
		}

		internal AsyncReadyCallbackInvoker (AsyncReadyCallbackNative native_cb) : this (native_cb, IntPtr.Zero, null) {}

		internal AsyncReadyCallbackInvoker (AsyncReadyCallbackNative native_cb, IntPtr data) : this (native_cb, data, null) {}

		internal AsyncReadyCallbackInvoker (AsyncReadyCallbackNative native_cb, IntPtr data, GLib.DestroyNotify notify)
		{
			this.native_cb = native_cb;
			__data = data;
			__notify = notify;
		}

		internal GLib.AsyncReadyCallback Handler {
			get {
				return new GLib.AsyncReadyCallback(InvokeNative);
			}
		}

		void InvokeNative (GLib.Object source_object, GLib.AsyncResult res)
		{
			native_cb (source_object == null ? IntPtr.Zero : source_object.Handle, res == null ? IntPtr.Zero : res.Handle, __data);
		}
	}

	internal class AsyncReadyCallbackWrapper {

		public void NativeCallback (IntPtr source_object, IntPtr res, IntPtr user_data)
		{
			try {
				managed (GLib.Object.GetObject (source_object), GLib.AsyncResultAdapter.GetObject (res, false));
				if (release_on_call)
					gch.Free ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		bool release_on_call = false;
		GCHandle gch;

		public void PersistUntilCalled ()
		{
			release_on_call = true;
			gch = GCHandle.Alloc (this);
		}

		internal AsyncReadyCallbackNative NativeDelegate;
		GLib.AsyncReadyCallback managed;

		public AsyncReadyCallbackWrapper (GLib.AsyncReadyCallback managed)
		{
			this.managed = managed;
			if (managed != null)
				NativeDelegate = new AsyncReadyCallbackNative (NativeCallback);
		}

		public static GLib.AsyncReadyCallback GetManagedDelegate (AsyncReadyCallbackNative native)
		{
			if (native == null)
				return null;
			AsyncReadyCallbackWrapper wrapper = (AsyncReadyCallbackWrapper) native.Target;
			if (wrapper == null)
				return null;
			return wrapper.managed;
		}
	}
#endregion
}
