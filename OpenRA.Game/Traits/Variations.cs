#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

namespace OpenRA.Traits
{
	[Desc("This actor has variations.")]
	public class VariationsInfo : ITraitInfo
	{
		[FieldLoader.Require, Desc("Defines the list of variations this actor can turn into. Listing an entry multiple times will increase it's chance.")]
		public string[] Variations = { };

		public object Create(ActorInitializer init) { return new Variations(init.Self, this); }
	}

	public class Variations
	{
		public Variations(Actor self, VariationsInfo info) { }
	}

	public class PreventVariationsInit : IActorInit<bool>
	{
		[FieldFromYamlKey]
		readonly bool value = true;
		public PreventVariationsInit() { }
		public PreventVariationsInit(bool init) { value = init; }
		public bool Value(World world) { return value; }
	}
}
