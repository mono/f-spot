using Gtk;
using Gdk;
using System;
using Mono.Posix;

namespace FSpot {
	public class SingleView {
		[Glade.Widget] Gtk.HBox toolbar_hbox;
		[Glade.Widget] Gtk.VBox info_vbox;
		[Glade.Widget] Gtk.ScrolledWindow image_scrolled;
		[Glade.Widget] Gtk.ScrolledWindow directory_scrolled;
		[Glade.Widget] Gtk.HPaned info_hpaned;

		[Glade.Widget] Gtk.CheckMenuItem side_pane_item;
		[Glade.Widget] Gtk.CheckMenuItem toolbar_item;
		[Glade.Widget] Gtk.CheckMenuItem filenames_item;
		
		[Glade.Widget] Gtk.MenuItem zoom_in;
		[Glade.Widget] Gtk.MenuItem zoom_out;

		[Glade.Widget] Gtk.Image near_image;
		[Glade.Widget] Gtk.Image far_image;

		[Glade.Widget] Gtk.Scale zoom_scale;

		[Glade.Widget] Label status_label;

		protected Glade.XML xml;
		private Gtk.Window window;
		PhotoImageView image_view;
		IconView directory_view;
		private Uri uri;
		
		InfoDialog metadata_dialog;
		
		UriCollection collection;
		
		FSpot.Delay slide_delay;

		FullScreenView fsview;

		private static Gtk.Tooltips toolTips = new Gtk.Tooltips ();

		public SingleView () : this (FSpot.Global.HomeDirectory) {}


		public SingleView (string path) : this (UriList.PathToFileUri (path)) 
		{
		}

		public SingleView (Uri uri) : this (new Uri [] { uri })
		{
		}

		public SingleView (UriList list) : this (list.ToArray ())
		{
		}

		public SingleView (Uri [] uris) 
		{
			string glade_name = "single_view";
			this.uri = uris [0];
			
			System.Console.WriteLine ("uri = {0}", uri.ToString ());

			xml = new Glade.XML (null, "f-spot.glade", glade_name, "f-spot");
			xml.Autoconnect (this);
			window = (Gtk.Window) xml.GetWidget (glade_name);
		
			LoadPreference (Preferences.VIEWER_WIDTH);
			LoadPreference (Preferences.VIEWER_MAXIMIZED);

			Gtk.Toolbar toolbar = new Gtk.Toolbar ();
			toolbar_hbox.PackStart (toolbar);
			GtkUtil.MakeToolbarButton (toolbar, "f-spot-rotate-270", new System.EventHandler (HandleRotate270Command));
			GtkUtil.MakeToolbarButton (toolbar, "f-spot-rotate-90", new System.EventHandler (HandleRotate90Command));

			toolbar.AppendSpace ();

			Widget fs_button = GtkUtil.MakeToolbarButton (toolbar, "f-spot-fullscreen", new System.EventHandler (HandleViewFullscreen));
			SetTip (fs_button, Catalog.GetString ("View photos fullscreen"));
			Widget ss_button = GtkUtil.MakeToolbarButton (toolbar, "f-spot-slideshow", new System.EventHandler (HandleViewSlideshow));
			SetTip (ss_button, Catalog.GetString ("View photos in a slideshow"));

			collection = new UriCollection (uris);

			TargetEntry [] dest_table = { new TargetEntry ("text/uri-list", 0, 0) };
			
			directory_view = new IconView (collection);
			directory_view.Selection.Changed += HandleSelectionChanged;
			directory_view.DragDataReceived += HandleDragDataReceived;
			Gtk.Drag.DestSet (directory_view, DestDefaults.All, dest_table, 
					DragAction.Copy | DragAction.Move); 
			directory_view.DisplayTags = false;
			directory_view.DisplayDates = false;
			directory_scrolled.Add (directory_view);

			ThumbnailGenerator.Default.OnPixbufLoaded += delegate { directory_view.QueueDraw (); };

			image_view = new PhotoImageView (collection);
			FSpot.Global.ModifyColors (image_view);
			FSpot.Global.ModifyColors (image_scrolled);
			image_view.ZoomChanged += HandleZoomChanged;
			image_view.Item.Changed += HandleItemChanged;
			image_view.DragDataReceived += HandleDragDataReceived;
			Gtk.Drag.DestSet (image_view, DestDefaults.All, dest_table,
					DragAction.Copy | DragAction.Move); 
			image_scrolled.Add (image_view);
			
			Window.ShowAll ();

			zoom_scale.ValueChanged += HandleZoomScaleValueChanged;
		
			LoadPreference (Preferences.VIEWER_SHOW_TOOLBAR);
			
			ShowSidebar = collection.Count > 1;

			near_image.SetFromStock ("f-spot-stock_near", Gtk.IconSize.SmallToolbar);
			far_image.SetFromStock ("f-spot-stock_far", Gtk.IconSize.SmallToolbar);

			slide_delay = new FSpot.Delay (new GLib.IdleHandler (SlideShow));

			LoadPreference (Preferences.VIEWER_SHOW_FILENAMES);

			Preferences.SettingChanged += OnPreferencesChanged;
			window.DeleteEvent += HandleDeleteEvent;
			
			collection.Changed += HandleCollectionChanged;
			UpdateStatusLabel ();
			
			if (collection.Count > 0)
				directory_view.Selection.Add (0);
		}

		public void HandleCollectionChanged (IBrowsableCollection collection)
		{
			if (collection.Count > 0) {
				Console.WriteLine ("Added selection");
				directory_view.Selection.Add (0);
			}

			if (collection.Count > 1)
				ShowSidebar = true;

			UpdateStatusLabel ();
		}

		public bool ShowSidebar {
			get {
				return info_vbox.Visible;
			}
			set {
				info_vbox.Visible = value;
				if (side_pane_item.Active != value)
					side_pane_item.Active = value;
			}
		}
		
		public bool ShowToolbar {
			get {
				return toolbar_hbox.Visible;
			}
			set {
				toolbar_hbox.Visible = value;
				if (toolbar_item.Active != value)
					toolbar_item.Active = value;
			}
		}

		private Uri CurrentUri
		{
			get { 
			 	return this.uri; 
			}
			set {
			 	this.uri = value;
				collection.Clear ();
				collection.LoadItems (new Uri[] { this.uri });
			}
		}

		void HandleRotate90Command (object sender, System.EventArgs args) 
		{
			RotateCommand command = new RotateCommand (this.Window);
			if (command.Execute (RotateDirection.Clockwise, new IBrowsableItem [] { image_view.Item.Current })) {
				collection.MarkChanged (image_view.Item.Index);
			}
		}

		void HandleRotate270Command (object sender, System.EventArgs args) 
		{
			RotateCommand command = new RotateCommand (this.Window);
			if (command.Execute (RotateDirection.Counterclockwise, new IBrowsableItem [] { image_view.Item.Current })) {
				collection.MarkChanged (image_view.Item.Index);
			}
		}		

		void HandleExportToFlickr (object sender, System.EventArgs args)
		{
			new FSpot.FlickrExport (new PhotoArray (directory_view.Selection.Items), false);
		}

		void HandleExportToPicasaweb (object sender, System.EventArgs args)
		{
			new FSpot.GoogleExport (new PhotoArray (directory_view.Selection.Items));
		}

		void HandleExportToGallery (object sender, System.EventArgs args)
		{
			new FSpot.GalleryExport (new PhotoArray (directory_view.Selection.Items)); 
		}

		void HandleExportToFolder (object sender, System.EventArgs args)
		{
			new FSpot.FolderExport (new PhotoArray (directory_view.Selection.Items));			
		}

		void HandleExportToCD (object sender, System.EventArgs args)
		{
			new FSpot.CDExport (new PhotoArray (directory_view.Selection.Items)); 
		}

		private void HandleSelectionChanged (FSpot.IBrowsableCollection selection) 
		{
			if (selection.Count > 0) {
				image_view.Item.Index = ((IconView.SelectionCollection)selection).Ids[0];
				image_view.Reload ();
				zoom_scale.Value = image_view.NormalizedZoom;
				if (metadata_dialog != null)
					metadata_dialog.InfoDisplay.Photo = image_view.Item.Current;
			}
			UpdateStatusLabel ();
		}

		private void HandleItemChanged (BrowsablePointer pointer, BrowsablePointerChangedArgs old)
		{
			directory_view.FocusCell = pointer.Index;
			directory_view.Selection.Clear ();
			if (collection.Count > 0) {
				directory_view.Selection.Add (directory_view.FocusCell);
				directory_view.ScrollTo (directory_view.FocusCell);
			}
		}

		private void HandleViewToolbar (object sender, System.EventArgs args)
		{
			ShowToolbar = toolbar_item.Active;
		}
		
		private void HandleHideSidePane (object sender, System.EventArgs args) 
		{
			ShowSidebar = false;
		}

		private void HandleViewSidePane (object sender, System.EventArgs args)
		{
			ShowSidebar = side_pane_item.Active;
		}

		private void HandleViewSlideshow (object sender, System.EventArgs args)
		{
			this.Window.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
			slide_delay.Start ();
		}
	
		private void HandleViewMetadata (object sender, System.EventArgs args)
		{
			if (metadata_dialog != null) {
				metadata_dialog.Present ();
				return;
			}
			
			metadata_dialog = new InfoDialog (window);
			metadata_dialog.InfoDisplay.Photo = image_view.Item.Current;
			
			metadata_dialog.ShowAll ();
			metadata_dialog.Destroyed += HandleMetadataDestroyed;
		}

		private void HandleViewFilenames (object sender, System.EventArgs args)
		{
			directory_view.DisplayFilenames = filenames_item.Active; 
			UpdateStatusLabel ();
		}

		private void HandleAbout (object sender, System.EventArgs args)
		{
			MainWindow.HandleAbout (sender, args);
		}

		private void HandleNewWindow (object sender, System.EventArgs args)
		{
			/* FIXME this needs to register witth the core */
			new SingleView (uri);
		}


		private void HandleOpenFolder (object sender, System.EventArgs args)
		{
			Open (FileChooserAction.SelectFolder);
		}

		private void HandleOpen (object sender, System.EventArgs args)
		{
			Open (FileChooserAction.Open);
		}

		private void Open (FileChooserAction action)
		{
			string title = Catalog.GetString ("Open");

			if (action == FileChooserAction.SelectFolder)
				title = Catalog.GetString ("Select Folder");

			FileChooserDialog chooser = new FileChooserDialog (title,
									   window,
									   action);

			chooser.AddButton (Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton (Stock.Open, ResponseType.Ok);

			chooser.SetUri (uri.ToString ());
			int response = chooser.Run ();

			if ((ResponseType) response == ResponseType.Ok)
				CurrentUri = new System.Uri (chooser.Uri);
			

			chooser.Destroy ();
		}

		private void HandleMetadataDestroyed (object sender, System.EventArgs args)
		{
			metadata_dialog = null;
		}
	
		private bool SlideShow ()
		{
			IBrowsableItem [] items = new IBrowsableItem [collection.Count];
			for (int i = 0; i < collection.Count; i++) {
				items [i] = collection [i];
			}

			FSpot.FullSlide full = new FSpot.FullSlide (Window, items);
			full.Play ();
			this.Window.GdkWindow.Cursor = null;
			return false;
		}

		private void HandleViewFullscreen (object sender, System.EventArgs args)
		{
			if (fsview != null)
				fsview.Destroy ();

			fsview = new FSpot.FullScreenView (collection);
			fsview.Destroyed += HandleFullScreenViewDestroy;

			fsview.View.Item.Index = image_view.Item.Index;
			fsview.Show ();
		}
		
		private void HandleFullScreenViewDestroy (object sender, System.EventArgs args)
		{
			directory_view.Selection.Clear ();
			if (fsview.View.Item.IsValid) 
				directory_view.Selection.Add (fsview.View.Item.Index);
			fsview = null;
		}
		
		public void HandleZoomOut (object sender, System.EventArgs args)
		{
			image_view.ZoomOut ();
		}

		public void HandleZoomOut (object sender, Gtk.ButtonPressEventArgs args)
		{
			image_view.ZoomOut ();
		}

		public void HandleZoomIn (object sender, Gtk.ButtonPressEventArgs args)
		{
			image_view.ZoomIn ();
		}

		public void HandleZoomIn (object sender, System.EventArgs args)
		{
			image_view.ZoomIn ();
		}

		private void HandleZoomScaleValueChanged (object sender, System.EventArgs args)
		{
			image_view.NormalizedZoom = zoom_scale.Value;
		}

		private void HandleZoomChanged (object sender, System.EventArgs args)
		{
			zoom_scale.Value = image_view.NormalizedZoom;

			// FIXME something is broken here
			//zoom_in.Sensitive = (zoom_scale.Value != 1.0);
			//zoom_out.Sensitive = (zoom_scale.Value != 0.0);
		}
	
		void HandleDeleteEvent (object sender, DeleteEventArgs args)
		{
			SavePreferences ();
			this.Window.Destroy ();
			args.RetVal = true;
		}

		void HandleDragDataReceived (object sender, DragDataReceivedArgs args) 
		{
		
		switch (args.Info) {
		case 0:
			/* 
			 * If the drop is coming from inside f-spot then we don't want to import 
			 */
			if (Gtk.Drag.GetSourceWidget (args.Context) != null)
				return;

			UriList list = new UriList (args.SelectionData);
			collection.LoadItems (list.ToArray());

			break;
		}

		Gtk.Drag.Finish (args.Context, true, false, args.Time);
		}

		private void UpdateStatusLabel ()
		{
			IBrowsableItem item = image_view.Item.Current;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (filenames_item.Active && item != null)
				sb.Append (item.Name + "  -  ");

			sb.AppendFormat (Catalog.GetPluralString ("{0} Photo", "{0} Photos", collection.Count), collection.Count);
			status_label.Text = sb.ToString ();

		}

		private void HandleFileClose (object sender, System.EventArgs args)
		{
			SavePreferences ();
			this.Window.Destroy ();
		}

		private void SavePreferences  ()
		{
			int width, height;
			window.GetSize (out width, out height);
		
			bool maximized = ((window.GdkWindow.State & Gdk.WindowState.Maximized) > 0);
			Preferences.Set (Preferences.VIEWER_MAXIMIZED, maximized);
		
			if (!maximized) {
				Preferences.Set (Preferences.VIEWER_WIDTH,	width);
				Preferences.Set (Preferences.VIEWER_HEIGHT,	height);
			}
		
			Preferences.Set (Preferences.VIEWER_SHOW_TOOLBAR,	toolbar_hbox.Visible);
			Preferences.Set (Preferences.VIEWER_SHOW_FILENAMES, filenames_item.Active);
		}

		private void HandleFileOpen (object sender, System.EventArgs args)
		{
			string open = null;
			
			FileChooserDialog file_selector =
				new FileChooserDialog ("Open", this.Window,
						       FileChooserAction.Open);
			
			file_selector.SetUri (uri.ToString ());
			int response = file_selector.Run ();
			
			if ((Gtk.ResponseType) response == Gtk.ResponseType.Ok) {
				open = file_selector.Filename;
				new FSpot.SingleView (open);
			}
			
			file_selector.Destroy ();
		}

		void OnPreferencesChanged (object sender, GConf.NotifyEventArgs args)
		{
			LoadPreference (args.Key);
		}

		void LoadPreference (String key)
		{
			object val = Preferences.Get (key);

			if (val == null)
				return;
			
			switch (key) {
			case Preferences.VIEWER_MAXIMIZED:
				if ((bool) val)
					window.Maximize ();
				else
					window.Unmaximize ();
				break;

			case Preferences.VIEWER_WIDTH:
			case Preferences.VIEWER_HEIGHT:
				window.SetDefaultSize((int) Preferences.Get(Preferences.VIEWER_WIDTH),
						(int) Preferences.Get(Preferences.VIEWER_HEIGHT));

				window.ReshowWithInitialSize();
				break;
			
			case Preferences.VIEWER_SHOW_TOOLBAR:
				if (toolbar_item.Active != (bool) val)
					toolbar_item.Active = (bool) val;

				toolbar_hbox.Visible = (bool) val;
				break;

			case Preferences.VIEWER_SHOW_FILENAMES:
				if (filenames_item.Active != (bool) val)
					filenames_item.Active = (bool) val;
				break;
			}
		}

		public Gtk.Window Window {
			get { 
				return window;
			}
		}


		public static void SetTip (Widget widget, string tip)
		{
			toolTips.SetTip (widget, tip, null);
		}
	}
}
