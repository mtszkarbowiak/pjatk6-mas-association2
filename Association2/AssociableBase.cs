namespace Association2;

public abstract class AssociableBase
{
	private enum CallIndex
	{
		Original,
		Reflected,
	}

	private readonly Dictionary<Association, Dictionary<object, AssociableBase>> _associations = new();


	private void AddLink(Association association, AssociableBase target, object? qualifier, CallIndex callIndex)
	{
		// Find or create the link dictionary for the association
		if (!_associations.TryGetValue(association, out var links))
		{
			links = new Dictionary<object, AssociableBase>();
			_associations.Add(association, links);
		}

		// Validate link
		if (callIndex == CallIndex.Original)
		{
			// Validate types of source and target.
			{
				var selfType = GetType();
				var targetType = target.GetType();

				if (!association.SourceType.IsAssignableFrom(selfType))
				{
					throw new ArgumentException(
						$"The source type of the association is {association.SourceType}, but the type of the source is {selfType}.",
						nameof(association)
					);
				}

				if (!association.TargetType.IsAssignableFrom(targetType))
				{
					throw new ArgumentException(
						$"The target type of the association is {association.TargetType}, but the type of the target is {targetType}.",
						nameof(association)
					);
				}
			}

			// Validate cardinality.
			if (association.Cardinality is not null)
			{
				var count = links.Count;
				if (count >= association.Cardinality)
				{
					throw new ArgumentException(
						$"The cardinality of the association is {association.Cardinality}, but the number of links is {count}.",
						nameof(association)
					);
				}
			}

			// Validate XOR.
			foreach (var xor in association.Xor)
			{
				if (_associations.TryGetValue(xor, out var xorLinks))
				{
					if (xorLinks.ContainsKey(qualifier ?? target))
					{
						throw new ArgumentException(
							$"The XOR association {xor.Name} is already linked.",
							nameof(association)
						);
					}
				}
			}

			// Validate qualifier.
			if (association.QualifierType is not null)
			{
				if (qualifier is null)
				{
					throw new ArgumentException(
						$"The qualifier type of the association is {association.QualifierType}, but the qualifier is null.",
						nameof(association)
					);
				}

				if (!association.QualifierType.IsInstanceOfType(qualifier))
				{
					throw new ArgumentException(
						$"The qualifier type of the association is {association.QualifierType}, but the type of the qualifier is {qualifier.GetType()}.",
						nameof(association)
					);
				}
			}

			// Validate that the target is not already linked.
			if (links.ContainsKey(qualifier ?? target))
			{
				throw new ArgumentException(
					$"The target is already linked.",
					nameof(target)
				);
			}

			// Validate constraints.
			foreach (var constraint in association.Constraints)
			{
				if (!constraint.IsSatisfied(association, this, target, qualifier))
				{
					throw new ArgumentException(
						$"The constraint {constraint.Name} is not satisfied: {constraint}",
						nameof(association)
					);
				}
			}
		}

		// Add link
		links.Add(qualifier ?? target, target);
		OnLinkAdded(association, target, null);

		// Add reflected link
		if (callIndex == CallIndex.Original)
		{
			target.AddLink(association, this, null, CallIndex.Reflected);
		}
	}

	private bool RemoveLinkByTarget(Association association, AssociableBase target, CallIndex callIndex)
	{
		if (callIndex == CallIndex.Original && association.QualifierType is not null)
		{
			throw new ArgumentException(
				$"The qualifier type of the association is {association.QualifierType}, but the qualifier is null.",
				nameof(association)
			);
		}

		if (!_associations.TryGetValue(association, out var links))
		{
			return false;
		}

		if (!links.ContainsKey(target))
		{
			return false;
		}

		var removed = links.Remove(target);

		if (removed)
		{
			OnLinkRemoved(association, target, null);

			if (callIndex == CallIndex.Original)
			{
				target.RemoveLinkByTarget(association, this, CallIndex.Reflected);
			}
		}

		return removed;
	}

	private bool RemoveLinkByQualifier(Association association, object qualifier, CallIndex callIndex)
	{
		if (callIndex == CallIndex.Original && association.QualifierType is null)
		{
			throw new ArgumentException(
				$"The qualifier type of the association is null, but the qualifier is not null.",
				nameof(association)
			);
		}

		if (!_associations.TryGetValue(association, out var links))
		{
			return false;
		}

		if (!links.TryGetValue(qualifier, out var target))
		{
			return false;
		}

		var removed = links.Remove(qualifier);

		if (removed)
		{
			OnLinkRemoved(association, target, qualifier);

			if (callIndex == CallIndex.Original)
			{
				target.RemoveLinkByTarget(association, this, CallIndex.Reflected);
			}
		}

		return true;
	}


	protected virtual void OnLinkAdded(Association association, AssociableBase target, object? qualifier)
	{
	}

	protected virtual void OnLinkRemoved(Association association, AssociableBase target, object? qualifier)
	{
	}


	public void AddLink(Association association, AssociableBase target, object? qualifier = null)
	{
		AddLink(association, target, qualifier, CallIndex.Original);
	}

	public bool RemoveLinkByTarget(Association association, AssociableBase target)
	{
		return RemoveLinkByTarget(association, target, CallIndex.Original);
	}

	public bool RemoveLinkByQualifier(Association association, object qualifier)
	{
		return RemoveLinkByQualifier(association, qualifier, CallIndex.Original);
	}

	public IEnumerable<KeyValuePair<object, AssociableBase>> GetLinks(Association association)
	{
		if (!_associations.TryGetValue(association, out var links))
		{
			return Enumerable.Empty<KeyValuePair<object, AssociableBase>>();
		}

		return links;
	}
}