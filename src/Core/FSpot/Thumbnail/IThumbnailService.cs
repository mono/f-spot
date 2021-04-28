// Copyright (C) 2016 Daniel Köb
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


using Gdk;

using Hyena;

namespace FSpot.Thumbnail
{
	public interface IThumbnailService
	{
		Pixbuf GetThumbnail (SafeUri fileUri, ThumbnailSize size);
		Pixbuf TryLoadThumbnail (SafeUri fileUri, ThumbnailSize size);
		void DeleteThumbnails (SafeUri fileUri);
	}
}
