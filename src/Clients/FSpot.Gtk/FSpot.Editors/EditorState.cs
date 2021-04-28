// Copyright (c) 2017 SUSE LINUX Products GmbH, Nuernberg, Germany.
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FSpot.Core;
using FSpot.Widgets;

using Gdk;

namespace FSpot.Editors
{
	// TODO: Move EditorNode to FSpot.Extionsions?

	public class EditorState
	{
		/// <summary>
		/// The area selected by the user.
		/// </summary>
		public Rectangle Selection { get; set; }

		/// <summary>
		/// The images selected by the user.
		/// </summary>
		public IPhoto[] Items { get; set; }

		/// <summary>
		/// The view, into which images are shown (null if we are in the browse view).
		/// </summary>
		public PhotoImageView PhotoImageView { get; set; }

		/// <summary>
		/// Has a portion of the image been selected?
		/// </summary>
		public bool HasSelection {
			get => Selection != Rectangle.Zero;
		}

		/// <summary>
		/// Is the user in browse mode?
		/// </summary>
		public bool InBrowseMode {
			get => PhotoImageView == null;
		}
	}
}
