// Copyright (C) 2003-2010 Novell, Inc.
// Copyright (C) 2003 Ettore Perazzoli
// Copyright (C) 2010 Peter Goetz
// Copyright (C) 2004-2006 Larry Ewing
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gtk;
using Gdk;

using Mono.Unix;

using FSpot.Database;
using FSpot.Models;
using FSpot.Settings;

using Hyena;

namespace FSpot.UI.Dialog
{
	public class CreateTagDialog : BuilderDialog
	{
#pragma warning disable 649
		[GtkBeans.Builder.Object] Button create_button;
		[GtkBeans.Builder.Object] Entry tag_name_entry;
		[GtkBeans.Builder.Object] Label prompt_label;
		[GtkBeans.Builder.Object] Label already_in_use_label;
		[GtkBeans.Builder.Object] ComboBox category_option_menu;
		[GtkBeans.Builder.Object] CheckButton auto_icon_checkbutton;
#pragma warning restore 649

		readonly TagStore tag_store;
		List<Tag> categories;

		public CreateTagDialog (TagStore tag_store) : base ("CreateTagDialog.ui", "create_tag_dialog")
		{
			this.tag_store = tag_store;
		}

		void PopulateCategories (ICollection<Tag> categories, Tag parent)
		{
			foreach (Tag tag in parent.Children.Where (x => x.IsCategory)) {
				categories.Add (tag);
				PopulateCategories (categories, tag);
			}
		}

		string Indentation (Tag category)
		{
			int indentations = 0;
			for (var parent = category.Category; parent?.Category != null; parent = parent.Category)
				indentations++;

			return new string (' ', indentations * 2);
		}

		void PopulateCategoryOptionMenu ()
		{
			categories = new List<Tag> { tag_store.RootCategory };
			PopulateCategories (categories, tag_store.RootCategory);

			var categoryStore = new ListStore (typeof (Pixbuf), typeof (string));

			foreach (var category in categories) {
				categoryStore.AppendValues (category?.TagIcon?.SizedIcon, $"{Indentation (category)}{category.Name}");
			}

			category_option_menu.Sensitive = true;

			category_option_menu.Model = categoryStore;
			using var iconRenderer = new CellRendererPixbuf { Width = (int)Tag.TagIconSize };
			category_option_menu.PackStart (iconRenderer, true);

			using var textRenderer = new CellRendererText {
				Alignment = Pango.Alignment.Left, Width = 150
			};

			category_option_menu.PackStart (textRenderer, true);
			category_option_menu.AddAttribute (iconRenderer, "pixbuf", 0);
			category_option_menu.AddAttribute (textRenderer, "text", 1);
			category_option_menu.ShowAll ();
		}

		bool TagNameExistsInCategory (string name, Tag category)
		{
			foreach (Tag tag in category.Children) {
				if (string.Compare (tag.Name, name, true) == 0)
					return true;

				if (tag.IsCategory && TagNameExistsInCategory (name, tag))
					return true;
			}

			return false;
		}

		void Update ()
		{
			if (string.IsNullOrEmpty(tag_name_entry.Text)) {
				create_button.Sensitive = false;
				already_in_use_label.Markup = string.Empty;
			} else if (TagNameExistsInCategory (tag_name_entry.Text, tag_store.RootCategory)) {
				create_button.Sensitive = false;
				already_in_use_label.Markup = "<small>" + Catalog.GetString ("This name is already in use") + "</small>";
			} else {
				create_button.Sensitive = true;
				already_in_use_label.Markup = string.Empty;
			}
		}

		void HandleTagNameEntryChanged (object sender, EventArgs args)
		{
			Update ();
		}

		Tag Category {
			get {
				if (categories.Count == 0)
					return tag_store.RootCategory;
				return categories[category_option_menu.Active];
			}
			set {
				if ((value != null) && (categories.Count > 0)) {
					//System.Console.WriteLine("TagCreateCommand.set_Category(" + value.Name + ")");
					for (int i = 0; i < categories.Count; i++) {
						var category = categories[i];
						// should there be an equals type method?
						if (value.Id == category.Id) {
							category_option_menu.Active = i;
							return;
						}
					}
				} else {
					category_option_menu.Active = 0;
				}
			}
		}

		public Tag Execute (IEnumerable<Tag> selection)
		{
			Tag defaultCategory;
			if (selection.Any ()) {
				if (selection.First ().IsCategory)
					defaultCategory = selection.First ();
				else
					defaultCategory = selection.First ().Category;
			} else {
				defaultCategory = tag_store.RootCategory;
			}

			DefaultResponse = ResponseType.Ok;

			Title = Catalog.GetString ("Create New Tag");
			prompt_label.Text = Catalog.GetString ("Name of New Tag:");
			auto_icon_checkbutton.Active = Preferences.Get<bool> (Preferences.TagIconAutomatic);

			PopulateCategoryOptionMenu ();
			Category = defaultCategory;
			Update ();
			tag_name_entry.GrabFocus ();

			var response = (ResponseType)Run ();

			Tag newTag = null;
			if (response == ResponseType.Ok) {
				bool autoicon = auto_icon_checkbutton.Active;
				Preferences.Set (Preferences.TagIconAutomatic, autoicon);
				try {
					var parentCategory = Category;

					newTag = tag_store.CreateTag (parentCategory, tag_name_entry.Text, autoicon, true);
				} catch (Exception ex) {
					// FIXME error dialog.
					Log.Exception (ex);
				}
			}

			Destroy ();
			return newTag;
		}
	}
}
