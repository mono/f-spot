// Copyright (C) 2016 Daniel Köb
// Copyright (C) 2003-2010 Novell, Inc.
// Copyright (C) 2009-2010 Anton Keks
// Copyright (C) 2003 Ettore Perazzoli
// Copyright (C) 2010 Ruben Vermeersch
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

using FSpot.Models;

using Gtk;

using Mono.Unix;

namespace FSpot.UI.Dialog
{
	public class VersionNameDialog : BuilderDialog
	{
		readonly Photo photo;

#pragma warning disable 649
		[GtkBeans.Builder.Object] Button ok_button;
		[GtkBeans.Builder.Object] Entry version_name_entry;
		[GtkBeans.Builder.Object] Label prompt_label;
		[GtkBeans.Builder.Object] Label already_in_use_label;
#pragma warning restore 649

		public enum RequestType
		{
			Create,
			Rename
		}

		readonly RequestType requestType;

		void Update ()
		{
			string newName = version_name_entry.Text;

			if (photo.VersionNameExists (newName) && !(requestType == RequestType.Rename && newName == photo.GetVersion (photo.DefaultVersionId).Name)) {
				already_in_use_label.Markup = "<small>This name is already in use</small>";
				ok_button.Sensitive = false;
				return;
			}

			already_in_use_label.Text = string.Empty;

			ok_button.Sensitive = newName.Length != 0;
		}

		void HandleVersionNameEntryChanged (object obj, EventArgs args)
		{
			Update ();
		}

		public VersionNameDialog (RequestType request_type, Photo photo, Window parent_window) : base ("VersionNameDialog.ui", "version_name_dialog")
		{
			this.requestType = request_type;
			this.photo = photo;

			switch (request_type) {
			case RequestType.Create:
				Title = Catalog.GetString ("Create New Version");
				prompt_label.Text = Catalog.GetString ("Name:");
				break;

			case RequestType.Rename:
				Title = Catalog.GetString ("Rename Version");
				prompt_label.Text = Catalog.GetString ("New name:");
				version_name_entry.Text = photo.GetVersion (photo.DefaultVersionId).Name;
				version_name_entry.SelectRegion (0, -1);
				break;
			}

			version_name_entry.Changed += HandleVersionNameEntryChanged;
			version_name_entry.ActivatesDefault = true;

			TransientFor = parent_window;
			DefaultResponse = ResponseType.Ok;

			Update ();
		}

		public ResponseType Run (out string name)
		{
			ResponseType response = (ResponseType)Run ();

			name = version_name_entry.Text;
			if (requestType == RequestType.Rename && name == photo.GetVersion (photo.DefaultVersionId).Name)
				response = ResponseType.Cancel;

			Destroy ();

			return response;
		}
	}
}
