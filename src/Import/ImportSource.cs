using Hyena;
using System;

namespace FSpot.Import
{
    public interface ImportSource {
        string Name { get; }
        string IconName { get; }

        void StartPhotoScan (ImportController controller);
        void Deactivate ();
    }

	public class ImportInfoVersion : IBrowsableItemVersion {
		public string Name { get { return String.Empty; } }
		public bool IsProtected { get { return true; } }
		public SafeUri BaseUri { get; set; }
		public string Filename { get; set; }

		public SafeUri Uri { get { return BaseUri.Append (Filename); } }
	}
}
