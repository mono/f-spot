// Copyright (C) 2008 Novell, Inc.
// Copyright (C) 2008 Ruben Vermeersch
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FSpot.ColorAdjustment;

using Gdk;

using Mono.Unix;

namespace FSpot.Editors
{
	class DesaturateEditor : Editor
	{
		public DesaturateEditor () : base (Catalog.GetString ("Desaturate"), "color-desaturate")
		{
			// FIXME: need tooltip Catalog.GetString ("Convert the photo to black and white")
			CanHandleMultiple = true;
		}

		protected override Pixbuf Process (Pixbuf input, Cms.Profile input_profile)
		{
			var desaturate = new Desaturate (input, input_profile);
			return desaturate.Adjust ();
		}
	}
}
