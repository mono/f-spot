// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

using Hyena;

using FSpot.Models;

namespace FSpot.Tools.DevelopInUFraw
{
	// GUI Version
	public class DevelopInUFRaw : AbstractDevelopInUFRaw
	{
		public DevelopInUFRaw() : base("ufraw")
		{
		}

		public override void Run (object o, EventArgs e)
		{
			Log.Information ("Executing DevelopInUFRaw extension");

			foreach (Photo p in App.Instance.Organizer.SelectedPhotos ()) {
				DevelopPhoto (p);
			}
		}
	}
}
