using Hyena;
using FSpot.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FSpot.Import
{
    public enum ImportEvent {
        SourceChanged,
        PhotoScanStarted,
        PhotoScanFinished,
        ImportStarted,
        ImportFinished,
        ImportError
    }

    public class ImportController
    {
        public PhotoList Photos { get; private set; }

        public ImportController ()
        {
            Photos = new PhotoList ();
            LoadPreferences ();
        }

        ~ImportController ()
        {
            DeactivateSource (ActiveSource);
        }

#region Import Preferences

        private bool copy_files;
        private bool recurse_subdirectories;
        private bool duplicate_detect;

        public bool CopyFiles {
            get { return copy_files; }
            set { copy_files = value; SavePreferences (); }
        }

        public bool RecurseSubdirectories {
            get { return recurse_subdirectories; }
            set { recurse_subdirectories = value; SavePreferences (); RescanPhotos (); }
        }

        public bool DuplicateDetect {
            get { return duplicate_detect; }
            set { duplicate_detect = value; SavePreferences (); }
        }

        void LoadPreferences ()
        {
            copy_files = Preferences.Get<bool> (Preferences.IMPORT_COPY_FILES);
            recurse_subdirectories = Preferences.Get<bool> (Preferences.IMPORT_INCLUDE_SUBFOLDERS);
            duplicate_detect = Preferences.Get<bool> (Preferences.IMPORT_CHECK_DUPLICATES);
        }

        void SavePreferences ()
        {
            Preferences.Set(Preferences.IMPORT_COPY_FILES, copy_files);
            Preferences.Set(Preferences.IMPORT_INCLUDE_SUBFOLDERS, recurse_subdirectories);
            Preferences.Set(Preferences.IMPORT_CHECK_DUPLICATES, duplicate_detect);
        }

#endregion

#region Source Scanning

        private List<ImportSource> sources;
        public List<ImportSource> Sources {
            get {
                if (sources == null)
                    sources = ScanSources ();
                return sources;
            }
        }

        List<ImportSource> ScanSources ()
        {
		    var monitor = GLib.VolumeMonitor.Default;
            var sources = new List<ImportSource> ();
            foreach (var mount in monitor.Mounts) {
                var root = new SafeUri (mount.Root.Uri, true);
                var icon = (mount.Icon as GLib.ThemedIcon).Names [0];
                sources.Add (new FileImportSource (root, mount.Name, icon));
            }
            return sources;
        }

#endregion

#region Status Reporting

        public delegate void ImportProgressHandler (int current, int total);
        public event ImportProgressHandler ProgressUpdated;

        public delegate void ImportEventHandler (ImportEvent evnt);
        public event ImportEventHandler StatusEvent;

        void FireEvent (ImportEvent evnt)
        {
            var h = StatusEvent;
            if (h != null)
                h (evnt);
        }

        void ReportProgress (int current, int total)
        {
            var h = ProgressUpdated;
            if (h != null)
                h (current, total);
        }

#endregion

#region Source Switching

        private ImportSource active_source;
        public ImportSource ActiveSource {
            set {
                if (value == active_source)
                    return;
                var old_source = active_source;
                active_source = value;
                FireEvent (ImportEvent.SourceChanged);
                RescanPhotos ();
                DeactivateSource (old_source);
            }
            get {
                return active_source;
            }
        }

        void DeactivateSource (ImportSource source)
        {
            if (source == null)
                return;
            source.Deactivate ();
        }

        void RescanPhotos ()
        {
            Photos.Clear ();
            ActiveSource.StartPhotoScan (this);
            FireEvent (ImportEvent.PhotoScanStarted);
        }

#endregion

#region Source Progress Signalling

        // These are callbacks that should be called by the sources.

        public void PhotoScanFinished ()
        {
            FireEvent (ImportEvent.PhotoScanFinished);
        }

#endregion

#region Importing

        Thread ImportThread;

        public void StartImport ()
        {
            if (ImportThread != null)
                throw new Exception ("Import already running!");

            FireEvent (ImportEvent.ImportStarted);
            ImportThread = ThreadAssist.Spawn (() => DoImport ());
        }

        public void CancelImport ()
        {

        }

        Stack<SafeUri> created_directories;
        List<uint> imported_photos;
        PhotoStore store = App.Instance.Database.Photos;
        RollStore rolls = FSpot.App.Instance.Database.Rolls;

        void DoImport ()
        {
            created_directories = new Stack<SafeUri> ();
            imported_photos = new List<uint> ();
            Roll roll = rolls.Create ();

            EnsureDirectory (Global.PhotoUri);

            try {
                int i = 0;
                int total = Photos.Count;
                foreach (var info in Photos.Items) {
                    ThreadAssist.ProxyToMain (() => ReportProgress (i++, total));
                    ImportPhoto (info, roll.Id);
                }

                FinishImport ();
            } catch (Exception e) {
                RollbackImport ();
                throw e;
            } finally {
                if (imported_photos.Count == 0)
                    rolls.Remove (roll);
                imported_photos = null;
                created_directories = null;
                Photo.ResetMD5Cache ();
                DeactivateSource (ActiveSource);
                Photos.Clear ();
                System.GC.Collect ();
            }
        }

        void FinishImport ()
        {
            ImportThread = null;
            FireEvent (ImportEvent.ImportFinished);
        }

        void RollbackImport ()
        {
        }

        void ImportPhoto (IBrowsableItem item, uint roll_id)
        {
            var destination = FindImportDestination (item.DefaultVersion.Uri);

            // Do duplicate detection
            if (DuplicateDetect && store.CheckForDuplicate (item.DefaultVersion.Uri, destination)) {
                return;
            }

            // Copy into photo folder.
			if (item.DefaultVersion.Uri != destination) {
				var file = GLib.FileFactory.NewForUri (item.DefaultVersion.Uri);
				var new_file = GLib.FileFactory.NewForUri (destination);
				file.Copy (new_file, GLib.FileCopyFlags.AllMetadata, null, null);
            }

            // Import photo
            var photo = store.Create (destination,
                                      item.DefaultVersion.Uri,
                                      roll_id);

            // FIXME: Add tags, import xmp crap
            imported_photos.Add (photo.Id);
        }

        SafeUri FindImportDestination (SafeUri uri)
        {
            if (!CopyFiles)
                return uri; // Keep it at the same place

            // Find a new unique location inside the photo folder
            string name = uri.GetFilename ();
            DateTime time;
            using (FSpot.ImageFile img = FSpot.ImageFile.Create (uri)) {
                time = img.Date;
            }

            var dest_uri = Global.PhotoUri.Append (time.Year.ToString ())
                                          .Append (String.Format ("{0:D2}", time.Month))
                                          .Append (String.Format ("{0:D2}", time.Day));
            EnsureDirectory (dest_uri);

            // If the destination we'd like to use is the file itself return that
            if (dest_uri.Append (name) == uri)
                return uri;

            // Find an unused name
            int i = 1;
            var dest = dest_uri.Append (name);
            var file = GLib.FileFactory.NewForUri (dest);
            while (file.Exists) {
                var filename = uri.GetFilenameWithoutExtension ();
                var extension = uri.GetExtension ();
                dest = dest_uri.Append (String.Format ("{0}-{1}{2}", filename, i++, extension));
                file = GLib.FileFactory.NewForUri (dest);
            }

            return dest;
        }

        void EnsureDirectory (SafeUri uri)
        {
            var parts = uri.AbsolutePath.Split('/');
            SafeUri current = new SafeUri (uri.Scheme + ":///", true);
            for (int i = 0; i < parts.Length; i++) {
                current = current.Append (parts [i]);
                var file = GLib.FileFactory.NewForUri (current);
                if (!file.Exists) {
                    created_directories.Push (current);
                    Log.Debug ("Creating "+current);
                    file.MakeDirectory (null);
                }
            }
        }

#endregion

    }
}
