// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

using Mono.Unix;

using Hyena;

using FSpot;
using FSpot.Models;
using FSpot.UI.Dialog;

namespace FSpot.Tools.DevelopInUFraw
{
	// Batch Version
	public class DevelopInUFRawBatch : AbstractDevelopInUFRaw
	{
		public DevelopInUFRawBatch () : base ("ufraw-batch")
		{
		}

		public override void Run (object o, EventArgs e)
		{
			var pdialog = new ProgressDialog (Catalog.GetString ("Developing photos"),
													ProgressDialog.CancelButtonType.Cancel,
													App.Instance.Organizer.SelectedPhotos ().Length,
													App.Instance.Organizer.Window);
			Log.Information ("Executing DevelopInUFRaw extension in batch mode");

			foreach (Photo p in App.Instance.Organizer.SelectedPhotos ()) {
				bool cancelled = pdialog.Update (string.Format (Catalog.GetString ("Developing {0}"), p.Name));
				if (cancelled) {
					break;
				}

				DevelopPhoto (p);
			}
			pdialog.Destroy ();
		}
	}
}
