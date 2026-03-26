/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Themes;
using System;

namespace Luster.Motion.Assests.Themes.AvalonDock
{
	/// <inheritdoc/>
	public class Vs2013LightTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/Luster.Motion.Assests;component/Themes/AvalonDock/LightTheme.xaml",
				UriKind.Relative);
		}
	}
}
