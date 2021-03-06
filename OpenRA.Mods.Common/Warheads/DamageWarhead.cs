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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Warheads
{
	public abstract class DamageWarhead : Warhead
	{
		[Desc("How much (raw) damage to deal.")]
		public readonly int Damage = 0;

		[Desc("Types of damage that this warhead causes. Leave empty for no damage.")]
		public readonly HashSet<string> DamageTypes = new HashSet<string>();

		[Desc("Damage percentage versus each armortype.")]
		public readonly Dictionary<string, int> Versus = new Dictionary<string, int>();

		public override bool IsValidAgainst(Actor victim, Actor firedBy)
		{
			if (Damage < 0 && victim.GetDamageState() == DamageState.Undamaged)
				return false;

			return base.IsValidAgainst(victim, firedBy);
		}

		/// <summary>
		/// Apply damage modifiers of all enabled Armor traits the actor has.
		/// </summary>
		public int DamageVersus(Actor victim)
		{
			var armor = victim.TraitsImplementing<Armor>()
				.Where(a => !a.IsTraitDisabled && a.Info.Type != null && Versus.ContainsKey(a.Info.Type))
				.Select(a => Versus[a.Info.Type]);

			return Util.ApplyPercentageModifiers(100, armor);
		}

		/// <summary>
		/// In this DamageVersus overload, we only want to apply Armor modifiers that are listed
		/// in ArmorTypes of the HitShape the warhead landed in.
		/// </summary>
		public int DamageVersus(Actor victim, HitShapeInfo shape)
		{
			var armor = victim.TraitsImplementing<Armor>()
				.Where(a => !a.IsTraitDisabled && a.Info.Type != null && Versus.ContainsKey(a.Info.Type) &&
					(!shape.ArmorTypes.Any() || shape.ArmorTypes.Contains(a.Info.Type)))
				.Select(a => Versus[a.Info.Type]);

			return Util.ApplyPercentageModifiers(100, armor);
		}

		public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			// Used by traits that damage a single actor, rather than a position
			if (target.Type == TargetType.Actor)
				DoImpact(target.Actor, firedBy, damageModifiers);
			else if (target.Type != TargetType.Invalid)
				DoImpact(target.CenterPosition, firedBy, damageModifiers);
		}

		public abstract void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers);

		public virtual void DoImpact(Actor victim, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			if (!IsValidAgainst(victim, firedBy))
				return;

			var damage = Util.ApplyPercentageModifiers(Damage, damageModifiers.Append(DamageVersus(victim)));
			victim.InflictDamage(firedBy, new Damage(damage, DamageTypes));
		}

		/// <summary>
		/// Only apply modifiers valid for a specific HitShape trait.
		/// </summary>
		public virtual void DoImpact(Actor victim, Actor firedBy, HitShapeInfo shape, IEnumerable<int> damageModifiers)
		{
			if (!IsValidAgainst(victim, firedBy))
				return;

			var damage = Util.ApplyPercentageModifiers(Damage, damageModifiers.Append(DamageVersus(victim, shape)));
			victim.InflictDamage(firedBy, new Damage(damage, DamageTypes));
		}
	}
}
