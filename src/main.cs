using Gtk;
using Gnome;
using System;
using System.IO;

public class Driver {

	public static void Main (string [] args)
	{
		Application.Init ();
		Program program = new Program (FSpot.Defines.PACKAGE, FSpot.Defines.VERSION, Modules.UI, args);

		Gnome.Vfs.Vfs.Initialize ();
		StockIcons.Initialize ();

		Mono.Posix.Catalog.Init ("f-spot", FSpot.Defines.LOCALE_DIR);

		// FIXME: Error checking is non-existant here...

		string base_directory = FSpot.Global.BaseDirectory;
		if (! File.Exists (base_directory))
			Directory.CreateDirectory (base_directory);

		Db db = new Db (Path.Combine (base_directory, "photos.db"), true);
		
		Gtk.Window.DefaultIconList = new Gdk.Pixbuf [] {PixbufUtils.LoadFromAssembly ("f-spot-logo.png")};

		new MainWindow (db);
		
		ParseCommands (args);

		program.Run ();
	}

	private static void  ParseCommands (string [] args) 
	{
		for (int i = 0; i < args.Length; i++) {
			if (args [i] == "--import") {
				if (++i < args.Length && (File.Exists (args [i]) || Directory.Exists (args[i]))) {
					MainWindow.Toplevel.ImportFile (args [i]);
				} else {
					System.Console.WriteLine ("no valid path to import from");
				}
			}
		}
	}
}
