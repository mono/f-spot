// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Mike Gemünde
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Gtk;
using Gdk;

using Hyena.Gui;

using FSpot.Core;

namespace FSpot.Widgets
{
	/// <summary>
	///    Renders a text caption with the date of the photo. This class is not based on
	///    TextCaptionRenderer, because it uses caching of the dates.
	/// </summary>
	public class ThumbnailDateCaptionRenderer : ThumbnailCaptionRenderer
	{
		readonly Dictionary<string, Pango.Layout> cache = new Dictionary<string, Pango.Layout> ();

		public override int GetHeight (Widget widget, int width)
		{
			return widget.Style.FontDescription.MeasureTextHeight (widget.PangoContext);
		}

		public override void Render (Drawable window,
									 Widget widget,
									 Rectangle cellArea,
									 Rectangle exposeArea,
									 StateType cellState,
									 IPhoto photo)
		{
			string date_text = null;

			if (photo is IInvalidPhotoCheck check && check.IsInvalid)
				return;

			if (cellArea.Width > 200) {
				date_text = photo.UtcTime.ToString ();
			} else {
				date_text = photo.UtcTime.ToShortDateString ();
			}

			if (!cache.TryGetValue (date_text, out var layout)) {
				layout = new Pango.Layout (widget.PangoContext);
				layout.SetText (date_text);

				cache.Add (date_text, layout);
			}

			Rectangle layout_bounds;
			layout.GetPixelSize (out layout_bounds.Width, out layout_bounds.Height);

			layout_bounds.Y = cellArea.Y;
			layout_bounds.X = cellArea.X + (cellArea.Width - layout_bounds.Width) / 2;

			if (layout_bounds.IntersectsWith (exposeArea)) {
				Style.PaintLayout (widget.Style, window, cellState,
								   true, exposeArea, widget, "IconView",
								   layout_bounds.X, layout_bounds.Y,
								   layout);
			}
		}
	}
}
