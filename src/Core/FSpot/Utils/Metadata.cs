//
// Metadata.cs
//
// Author:
//   Ruben Vermeersch <ruben@savanne.be>
//
// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using FSpot.FileSystem;
using Hyena;

using TagLib;
using Log = Hyena.Log;

namespace FSpot.Utils
{
	public static class MetadataUtils
	{
		public static TagLib.Image.File Parse (SafeUri uri)
		{
			// Detect mime-type
			string mime = new DotNetFile ().GetMimeType (uri);

			if (mime.StartsWith ("application/x-extension-")) {
				// Works around broken metadata detection - https://bugzilla.gnome.org/show_bug.cgi?id=624781
				mime = $"taglib/{mime.Substring (24)}";
			}

			// Parse file
			var res = new TagLibFileAbstraction { Uri = uri };
			var sidecar_uri = GetSidecarUri (uri);
			var sidecar_res = new TagLibFileAbstraction { Uri = sidecar_uri };

			TagLib.Image.File file;
			try {
				file = TagLib.File.Create (res, mime, ReadStyle.Average) as TagLib.Image.File;
			} catch (Exception) {
				Log.Debug ($"Loading of metadata failed for file: {uri}, trying extension fallback");

				try {
					file = TagLib.File.Create (res, ReadStyle.Average) as TagLib.Image.File;
				} catch (Exception e) {
					Log.Debug ($"Loading of metadata failed for file: {uri}");
					Log.DebugException (e);
					return null;
				}
			}

			// Load XMP sidecar
			if (System.IO.File.Exists (sidecar_uri.AbsolutePath))
				file.ParseXmpSidecar (sidecar_res);

			return file;
		}

		public static void SaveSafely (this TagLib.Image.File metadata, SafeUri photoUri, bool alwaysSidecar)
		{
			if (alwaysSidecar || !metadata.Writeable || metadata.PossiblyCorrupt) {
				if (!alwaysSidecar && metadata.PossiblyCorrupt) {
					Log.Warning ($"Metadata of file {photoUri} may be corrupt, refusing to write to it, falling back to XMP sidecar.");
				}

				var sidecar_res = new TagLibFileAbstraction () { Uri = GetSidecarUri (photoUri) };

				metadata.SaveXmpSidecar (sidecar_res);
			} else {
				metadata.Save ();
			}
		}

		delegate SafeUri GenerateSideCarName (SafeUri photo_uri);
		static readonly GenerateSideCarName[] SidecarNameGenerators = {
			(p) => new SafeUri (p.AbsoluteUri + ".xmp"),
			(p) => p.ReplaceExtension (".xmp"),
		};

		public static SafeUri GetSidecarUri (SafeUri photoUri)
		{
			// First probe for existing sidecar files, use the one that's found.
			foreach (var generator in SidecarNameGenerators) {
				var name = generator (photoUri);
				if (System.IO.File.Exists (name.AbsolutePath)) {
					return name;
				}
			}

			// Fall back to the default strategy.
			return SidecarNameGenerators[0] (photoUri);
		}
	}
}
