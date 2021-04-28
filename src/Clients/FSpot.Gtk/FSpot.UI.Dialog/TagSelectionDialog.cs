//
// TagSelectionDialog.cs
//
// Author:
//   Mike Gemünde <mike@gemuende.de>
//   Eric Faehnrich <misterfright@gmail.com>
//   Ruben Vermeersch <ruben@savanne.be>
//
// Copyright (C) 2009-2010 Novell, Inc.
// Copyright (C) 2009 Mike Gemünde
// Copyright (C) 2010 Eric Faehnrich
// Copyright (C) 2010 Ruben Vermeersch
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Gtk;

using FSpot.Database;
using FSpot.Models;

namespace FSpot.UI.Dialog
{
	public class TagSelectionDialog : BuilderDialog
	{
#pragma warning disable 649
		[GtkBeans.Builder.Object] Gtk.ScrolledWindow tag_selection_scrolled;
#pragma warning restore 649

		readonly TagSelectionWidget tagSelectionWidget;

		public TagSelectionDialog (TagStore tags) : base ("tag_selection_dialog.ui", "tag_selection_dialog")
		{
			tagSelectionWidget = new TagSelectionWidget (tags);
			tag_selection_scrolled.Add (tagSelectionWidget);
			tagSelectionWidget.Show ();
		}

		public new IEnumerable<Tag> Run ()
		{
			int response = base.Run ();
			if ((ResponseType)response == ResponseType.Ok)
				return tagSelectionWidget.TagHighlight;

			return null;
		}

		public new void Hide ()
		{
			base.Hide ();
		}
	}
}
